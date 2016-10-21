using AzureScaleLeetTreats.Data;
using AzureScaleLeetTreats.ShardUtilities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace AzureScaleLeetTreats.Web.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.ShopperName = Request.IsAuthenticated ? ShopperName : "";
            base.OnActionExecuting(filterContext);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception is Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.ShardManagementException && !filterContext.ExceptionHandled)
            {
                var err = (Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.ShardManagementException)filterContext.Exception;
                if (err.ErrorCode == Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.ShardManagementErrorCode.MappingNotFoundForKey)
                {
                    SignOutUser();
                    filterContext.Result = new RedirectResult("~/");
                    filterContext.ExceptionHandled = true;
                    return;
                }
            }
            
            base.OnException(filterContext);
        }

        protected string GetClaimValue(string claimType)
        {
            if (!Request.IsAuthenticated) return null;

            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            Claim claim = authenticationManager.User.Claims.Where(c => c.Type == claimType).Single();
            return claim.Value;
        }

        protected int ShopperId
        {
            get
            {
                string claim = GetClaimValue(ClaimTypes.NameIdentifier);
                return claim != null ? int.Parse(claim) : 0;
            }
        }

        protected string ShopperName
        {
            get
            {
                return GetClaimValue(ClaimTypes.Name);
            }
        }

        protected StoreDataContext CreateDataContext()
        {
            return ShardManager.CreateStoreDataContext(ShopperId);
        }

        protected void SignInUser(int id, string userName)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, userName));
            var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            var authenticationManager = Request.GetOwinContext().Authentication;
            authenticationManager.SignIn(identity);
        }

        protected void SignOutUser()
        {
            var authenticationManager = Request.GetOwinContext().Authentication;
            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}