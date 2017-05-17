﻿using Cofoundry.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofoundry.Web
{
    /// <summary>
    /// Extends the IMvcBuilder configuration to allow for modular configuration
    /// of Mvc services
    /// </summary>
    public class CofoundryMvcBuilderConfiguration : IMvcBuilderConfiguration
    {
        private readonly IEnumerable<IMvcJsonOptionsConfiguration> _mvcJsonOptionsConfigurations;
        private readonly IEnumerable<IMvcOptionsConfiguration> _mvcOptionsConfigurations;
        private readonly IEnumerable<IRazorViewEngineOptionsConfiguration> _razorViewEngineOptionsConfigurations;

        public CofoundryMvcBuilderConfiguration(
            IEnumerable<IMvcJsonOptionsConfiguration> mvcJsonOptionsConfigurations,
            IEnumerable<IMvcOptionsConfiguration> mvcOptionsConfigurations,
            IEnumerable<IRazorViewEngineOptionsConfiguration> razorViewEngineOptionsConfigurations
            )
        {
            _mvcJsonOptionsConfigurations = mvcJsonOptionsConfigurations;
            _mvcOptionsConfigurations = mvcOptionsConfigurations;
            _razorViewEngineOptionsConfigurations = razorViewEngineOptionsConfigurations;
        }

        /// <summary>
        /// Configures Mvc services. Runs after AddMvc in the service
        /// configuration pipeline.
        /// </summary>
        /// <param name="mvcBuilder">IMvcBuilder to configure.</param>
        public void Configure(IMvcBuilder mvcBuilder)
        {
            foreach (var config in EnumerableHelper
                .Enumerate(_mvcJsonOptionsConfigurations)
                .OrderByDescending(o => o is CofoundryMvcJsonOptionsConfiguration))
            {
                mvcBuilder.Services.Configure<MvcJsonOptions>(o => config.Configure(o));
            }

            foreach (var config in EnumerableHelper.Enumerate(_mvcOptionsConfigurations))
            {
                mvcBuilder.Services.Configure<MvcOptions>(o => config.Configure(o));
            }

            foreach (var config in EnumerableHelper
                .Enumerate(_razorViewEngineOptionsConfigurations)
                .OrderByDescending(o => o is CofoundryRazorViewEngineOptionsConfiguration))
            {
                mvcBuilder.Services.Configure<RazorViewEngineOptions>(o => config.Configure(o));
            }
        }
    }
}