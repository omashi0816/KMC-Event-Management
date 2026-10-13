using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using System;

namespace KMCEventClient.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var http = context.HttpContext;
            var path = http.Request.Path;

            // Allow access to the Auth controller actions to avoid redirect loops
            if (path.StartsWithSegments("/Auth", StringComparison.OrdinalIgnoreCase))
            {
                base.OnActionExecuting(context);
                return;
            }

            // Check session token
            try
            {
                var token = http.Session.GetString("JwtToken");
                if (string.IsNullOrWhiteSpace(token))
                {
                    context.Result = new RedirectToActionResult("Login", "Auth", null);
                    return;
                }
            }
            catch
            {
                // if session fails, redirect to login
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
