﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Cofoundry.Samples.UserAreas
{
    public static class UrlLibrary
    {
        #region customers

        public static string CustomerRoot()
        {
            return "/partners";
        }

        public static string CustomerDefault()
        {
            return CustomerRoot();
        }

        public static string CustomerLogin(string returnUrl = null)
        {
            var url = CustomerRoot() + "/auth/login";

            if (returnUrl != null)
            {
                url += "?returnUrl=" + WebUtility.UrlEncode(returnUrl);
            }

            return url;
        }

        public static string CustomerForgotPassword()
        {
            return CustomerRoot() + "/auth/forgot-password";
        }

        public static string CustomerLogout()
        {
            return PartnerRoot() + "/auth/logout";
        }

        #endregion

        #region partners

        public static string PartnerRoot()
        {
            return "/partners";
        }

        public static string PartnerLogin(string returnUrl = null)
        {
            var url = PartnerRoot() + "/auth/login";

            if (returnUrl != null)
            {
                url += "?returnUrl=" + WebUtility.UrlEncode(returnUrl);
            }

            return url;
        }

        public static string PartnerForgotPassword()
        {
            return PartnerRoot() + "/auth/forgot-password";
        }

        public static string PartnerLogout()
        {
            return PartnerRoot() + "/auth/logout";
        }

        public static string PartnerDefault()
        {
            return PartnerWelcome();
        }

        public static string PartnerWelcome()
        {
            return PartnerRoot() + "/welcome";
        }

        #endregion
    }
}