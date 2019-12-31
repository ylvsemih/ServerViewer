using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using temperaturemois.Manager;


namespace temperaturemois.Manager
{
    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.Session.GetString("KullaniciEmail");
            //var response = context.HttpContext.Request.Path.Value.ToLower().Contains("admin");

            if (user == null)
            {
                context.Result = new RedirectResult("/Home/Register");
            }

        }
    }

}
