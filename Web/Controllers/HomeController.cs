using AzureScaleLeetTreats.Data;
using AzureScaleLeetTreats.Data.Model;
using AzureScaleLeetTreats.ShardUtilities;
using AzureScaleLeetTreats.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
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
            if (Request.IsAuthenticated)
                return RedirectToAction("Catalog");
            else
                return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            if (Request.IsAuthenticated)
            {
                SignOutUser();
                return Redirect("~/");
            }

            var model = new RegistrationModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Register(RegistrationModel model)
        {
            Shopper shopper = ShardManager.GetShopperByUserName(model.UserName);
            if (shopper != null)
            {
                ModelState.AddModelError("register", "User name already taken, please try again");
                return View(model);
            }

            shopper = new Shopper
            {
                UserName = model.UserName,
                CreateDate = DateTime.UtcNow
            };

            ShardManager.CreateShopper(shopper);

            SignInUser(shopper.ShopperID, shopper.UserName);

            return Redirect("~/");
        }

        private static OrderSummaryModel MapOrderSummaryFromReader(IDataReader reader)
        {
            return new OrderSummaryModel
            {
                UserName = reader.GetString(0),
                Total = reader.GetInt32(1),
                KitKat = reader.GetInt32(2),
                FifthAvenue = reader.GetInt32(3),
                Butterfinger = reader.GetInt32(4),
                Crunch = reader.GetInt32(5),
            };
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

        [HttpGet]
        public ActionResult SignIn()
        {
            if (Request.IsAuthenticated)
            {
                SignOutUser();
                return Redirect("~/");
            }

            var model = new RegistrationModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult SignIn(RegistrationModel model)
        {
            Shopper shopper = ShardManager.GetShopperByUserName(model.UserName);
            if (shopper == null)
            {
                ModelState.AddModelError("login", "Login failed, please try again");
                return View(model);
            }
            SignInUser(shopper.ShopperID, shopper.UserName);
            return Redirect("~/");
        }

        [HttpGet]
        public ActionResult SignOut()
        {
            SignOutUser();
            return Redirect("~/");
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

        [Authorize]
        [HttpGet]
        public ActionResult OrderSummary()
        {
            string sql = Util.GetEmbeddedResourceText("AzureScaleLeetTreats.Web.Controllers.MultiShardOrderQuery.sql");
            OrderSummaryModel[] orderSummaries = ShardManager.MultiShardQuery(sql, MapOrderSummaryFromReader).ToArray();
            return View(orderSummaries);
        }
    }
}