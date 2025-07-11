﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationPolicy.AdminOrSelfUserId
{
    public class AdminOrSelfAccountIDHandler : AuthorizationHandler<AdminOrSelfAccountIDReq, int>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOrSelfAccountIDReq requirement, int userID)
        {

            string roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            string idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            if (roleClaim.ToString() == "Admin" || idClaim == userID.ToString()) context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
