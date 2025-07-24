using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationPolicy.AdminOrSelfUserId
{
    public class SelfAccountIDHandler : AuthorizationHandler<SelfAccountIDReq, int>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SelfAccountIDReq requirement, int accountID)
        {
            string idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            if (idClaim == accountID.ToString()) context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
