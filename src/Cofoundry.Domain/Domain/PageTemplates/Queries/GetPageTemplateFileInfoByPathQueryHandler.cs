﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cofoundry.Domain.CQS;
using Cofoundry.Core;

namespace Cofoundry.Domain
{
    public class GetPageTemplateFileInfoByPathQueryHandler 
        : IAsyncQueryHandler<GetPageTemplateFileInfoByPathQuery, PageTemplateFileInfo>
        , IPermissionRestrictedQueryHandler<GetPageTemplateFileInfoByPathQuery, PageTemplateFileInfo>
    {
        #region constructor

        private readonly IQueryExecutor _queryExecutor;
        private readonly IPageTemplateViewFileLocator _viewLocator;
        private readonly PageTemplateCustomEntityTypeMapper _pageTemplateCustomEntityTypeMapper;

        public GetPageTemplateFileInfoByPathQueryHandler(
            IPageTemplateViewFileLocator viewLocator,
            IQueryExecutor queryExecutor,
            PageTemplateCustomEntityTypeMapper pageTemplateCustomEntityTypeMapper
            )
        {
            _queryExecutor = queryExecutor;
            _viewLocator = viewLocator;
            _pageTemplateCustomEntityTypeMapper = pageTemplateCustomEntityTypeMapper;
        }

        #endregion

        #region execution

        public async Task<PageTemplateFileInfo> ExecuteAsync(GetPageTemplateFileInfoByPathQuery query, IExecutionContext executionContext)
        {
            var view = await _viewLocator.ReadViewFileAsync(query.FullPath);

            if (view == null)
            {
                throw new ApplicationException("View file not found: " + query.FullPath);
            }

            var pageTemplateFileInfo = await ParseViewFile(view, true, executionContext);

            return pageTemplateFileInfo;
        }

        private async Task<PageTemplateFileInfo> ParseViewFile(string viewFile, bool isRootFile, IExecutionContext executionContext)
        {
            const string PLACEHOLDER_FUNC = "Cofoundry.UI.PageTemplateSection";
            const string CUSTOM_ENTITY_PLACEHOLDER_FUNC = "Cofoundry.UI.CustomEntitySection";
            const string PARTIAL_FUNC = "Html.Partial";
            const string RENDER_PARTIAL_FUNC = "Html.RenderPartial";

            var pageTemplateFileInfo = new PageTemplateFileInfo();
            pageTemplateFileInfo.PageType = PageType.Generic;

            var sections = new List<PageTemplateFileSection>();

            using (var sr = new StringReader(viewFile))
            {
                string line;
                bool parseCustomModelType = isRootFile;

                while ((line = sr.ReadLine()) != null)
                {
                    if (parseCustomModelType)
                    {
                        SetCustomModelTypeFields(pageTemplateFileInfo, line, executionContext);
                        parseCustomModelType = false;
                    }
                    else if (line.Contains(PLACEHOLDER_FUNC))
                    {
                        var sectionName = ParseFunctionParameter(line, PLACEHOLDER_FUNC);
                        sections.Add(new PageTemplateFileSection() { Name = sectionName });
                    }
                    else if (line.Contains(CUSTOM_ENTITY_PLACEHOLDER_FUNC))
                    {
                        var sectionName = ParseFunctionParameter(line, CUSTOM_ENTITY_PLACEHOLDER_FUNC);
                        sections.Add(new PageTemplateFileSection() { Name = sectionName, IsCustomEntitySection = true });
                    }
                    else if (line.Contains(PARTIAL_FUNC))
                    {
                        sections.AddRange(await ParsePartialView(line, PARTIAL_FUNC, executionContext));
                    }
                    else if (line.Contains(RENDER_PARTIAL_FUNC))
                    {
                        sections.AddRange(await ParsePartialView(line, RENDER_PARTIAL_FUNC, executionContext));
                    }
                }
            }

            pageTemplateFileInfo.Sections = sections.ToArray();

            return pageTemplateFileInfo;
        }

        private void SetCustomModelTypeFields(PageTemplateFileInfo pageTemplateFileInfo, string line, IExecutionContext ex)
        {
            const string CUSTOM_ENTITY_MODEL_REGEX = @"CustomEntityDetailsPageViewModel<(\w+)";

            var match = Regex.Match(line, CUSTOM_ENTITY_MODEL_REGEX);
            if (match.Success)
            {
                var modelType = _pageTemplateCustomEntityTypeMapper.Map(match.Groups[1].Value);

                if (modelType == null)
                {
                    throw new ApplicationException("ICustomEntityDisplayModel<T> of type " + match.Value + " not registered");
                }

                var query = new GetCustomEntityDefinitionMicroSummaryByDisplayModelTypeQuery();
                query.DisplayModelType = modelType;

                pageTemplateFileInfo.CustomEntityDefinition = _queryExecutor.Execute(query, ex);
                EntityNotFoundException.ThrowIfNull(pageTemplateFileInfo.CustomEntityDefinition, modelType);
                pageTemplateFileInfo.CustomEntityModelType = match.Groups[1].Value;

                pageTemplateFileInfo.PageType = PageType.CustomEntityDetails;
            }
        }

        private async Task<IEnumerable<PageTemplateFileSection>> ParsePartialView(string textLine, string partialFuncName, IExecutionContext executionContext)
        {
            var partialName = ParseFunctionParameter(textLine, partialFuncName);
            var partialPath = _viewLocator.ResolvePageTemplatePartialViewPath(partialName);

            Debug.Assert(!string.IsNullOrEmpty(partialPath), "Partial View file not found: " + partialName);

            if (string.IsNullOrEmpty(partialPath)) return Enumerable.Empty<PageTemplateFileSection>();
            var partialFile = await _viewLocator.ReadViewFileAsync(partialPath);

            return (await ParseViewFile(partialFile, false, executionContext)).Sections;
        }
        
        private string ParseFunctionParameter(string textLine, string functionName)
        {
            var startFunc = functionName + "(\"";

            int start = textLine.IndexOf(startFunc) + startFunc.Length;
            var parameterValue = textLine.Substring(start);
            parameterValue = parameterValue.Substring(0, parameterValue.IndexOf('"'));

            return parameterValue;
        }

        #endregion

        #region Permission

        public IEnumerable<IPermissionApplication> GetPermissions(GetPageTemplateFileInfoByPathQuery query)
        {
            yield return new CompositePermissionApplication(new PageTemplateCreatePermission(), new PageTemplateUpdatePermission());
        }

        #endregion
    }
}