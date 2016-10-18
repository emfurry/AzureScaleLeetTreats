using AzureScaleLeetTreats.Data;
using AzureScaleLeetTreats.Data.Model;
using AzureScaleLeetTreats.Web.Models;
using Microsoft.AspNet.Identity;
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
        private static Random _rnd = new Random();

        private string GetClaimValue(string claimType)
        {
            if (!Request.IsAuthenticated) return null;

            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            Claim claim = authenticationManager.User.Claims.Where(c => c.Type == claimType).Single();
            return claim.Value;
        }

        private int? GetShopperId()
        {
            string claim = GetClaimValue(ClaimTypes.NameIdentifier);
            return Int32.Parse(claim);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            var model = new RegistrationModel();
            return View(model);
        }

        [Authorize]
        public ActionResult Catalog()
        {
            var model = new CatalogModel { ImageBaseUrl = "~/Content/products/" };
            using (var db = new StoreDataContext())
            {
                model.Products = db.Products.ToArray();
            }
            return View(model);
        }

        private void LoginShopper(Shopper shopper)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, shopper.UserName));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, shopper.ShopperID.ToString()));
            var id = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignIn(id);
        }

        [HttpPost]
        public ActionResult Register(RegistrationModel model)
        {
            if (Request.IsAuthenticated)
            {
                var authenticationManager = HttpContext.GetOwinContext().Authentication;
                authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return Redirect("~/");
            }

            Shopper newShopper;
            using (var db = new StoreDataContext())
            {
                newShopper = new Shopper
                {
                    UserName = model.UserName,
                    CreateDate = DateTime.UtcNow
                };
                db.Shoppers.Add(newShopper);
                db.SaveChanges();
            }

            LoginShopper(newShopper);

            return RedirectToAction("Catalog");
        }

        public ActionResult Buy(int productId)
        {
            var model = new BuyModel { ImageBaseUrl = "~/Content/products/" };
            using (var db = new StoreDataContext())
            {
                model.Product = db.Products.Where(p => p.ProductID == productId).Single();
            }
            return View(model);
        }

        private static string SelectWinner(string ai, string shopper)
        {
            if (ai == shopper)
                return "tie";
            else if (ai == "rock" && shopper == "paper")
                return "shopper";
            else if (ai == "paper" && shopper == "scissors")
                return "shopper";
            else if (ai == "scissors" && shopper == "rock")
                return "shopper";
            else
                return "ai";
        }

        [Authorize]
        [HttpPost]
        public JsonResult DecideWinner(int productId, string selection)
        {
            var value = _rnd.Next(3);
            string aiSelection;
            if (value < 1)
                aiSelection = "rock";
            else if (value < 2)
                aiSelection = "paper";
            else
                aiSelection = "scissors";

            string winner = SelectWinner(aiSelection, selection);

            if (winner == "shopper")
            {
                int shopperId = GetShopperId().Value;
                using (var db = new StoreDataContext())
                {
                    var order = new Order { ShopperID = shopperId, ProductID = productId };
                    db.Orders.Add(order);
                    db.SaveChanges();
                }
            }

            return Json(new { winner = winner, aiselection = aiSelection });
        }
    }
}