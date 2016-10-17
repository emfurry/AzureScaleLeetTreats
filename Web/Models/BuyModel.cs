using AzureScaleLeetTreats.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureScaleLeetTreats.Web.Models
{
    public class BuyModel
    {
        public string ImageBaseUrl { get; set; }
        public Product Product { get; set; }
    }
}