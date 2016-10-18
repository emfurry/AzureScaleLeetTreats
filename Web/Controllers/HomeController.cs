using AzureScaleLeetTreats.Data;
using AzureScaleLeetTreats.Data.Model;
using AzureScaleLeetTreats.ShardUtilities;
using AzureScaleLeetTreats.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureScaleLeetTreats.Web.Controllers
{
    public class HomeController : BaseController
    {
        private static Random _rnd = new Random();

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
            using (var db = CreateDataContext())
            {
                model.Products = db.Products.ToArray();
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Register(RegistrationModel model)
        {
            if (Request.IsAuthenticated)
            {
                SignOut();
                return Redirect("~/");
            }

            var shopper = new Shopper
            {
                UserName = model.UserName,
                CreateDate = DateTime.UtcNow
            };

            ShardManager.CreateShopper(shopper);

            SignIn(shopper.ShopperID, shopper.UserName);

            return RedirectToAction("Catalog");
        }

        [Authorize]
        public ActionResult Buy(int productId)
        {
            var model = new BuyModel { ImageBaseUrl = "~/Content/products/" };
            using (var db = CreateDataContext())
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
                using (var db = CreateDataContext())
                {
                    var order = new Order { ShopperID = ShopperId, ProductID = productId };
                    db.Orders.Add(order);
                    db.SaveChanges();
                }
            }

            return Json(new { winner = winner, aiselection = aiSelection });
        }
    }
}