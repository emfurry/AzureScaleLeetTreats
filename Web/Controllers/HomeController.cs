using AzureScaleLeetTreats.Data;
using AzureScaleLeetTreats.Data.Model;
using AzureScaleLeetTreats.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace AzureScaleLeetTreats.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        private void LoginShopper(Shopper shopper)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, shopper.NickName));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, shopper.ShopperID.ToString()));
            var id = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignIn(id);
        }

        [HttpPost]
        public ActionResult Register(RegistrationModel model)
        {
            Shopper newShopper;
            using (var db = new StoreDataContext())
            {
                newShopper = new Shopper
                {
                    NickName = model.Nickname,
                    CreateDate = DateTime.UtcNow
                };
                db.Shoppers.Add(newShopper);
                db.SaveChanges();
            }

            LoginShopper(newShopper);

            return RedirectToAction("Index");
        }
    }
}