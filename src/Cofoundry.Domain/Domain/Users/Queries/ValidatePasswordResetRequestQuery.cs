﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofoundry.Domain.CQS;

namespace Cofoundry.Domain
{
    public class ValidatePasswordResetRequestQuery : IQuery<PasswordResetRequestAuthenticationResult>
    {
        /// <summary>
        /// A unique identifier required to authenticate when 
        /// resetting the password. May be Guid.Empty if the
        /// value could not be parsed correctly.
        /// </summary>
        [Required]
        public Guid UserPasswordResetRequestId { get; set; }

        /// <summary>
        /// A token used to authenticate when resetting the password.
        /// May be null if the token was not present in the querystring.
        /// </summary>
        [Required]
        public string Token { get; set; }

        [Required]
        [StringLength(3)]
        public string UserAreaCode { get; set; }
    }
}
