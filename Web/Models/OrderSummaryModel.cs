using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureScaleLeetTreats.Web.Models
{
    public class OrderSummaryModel
    {
        public string UserName { get; set; }
        public int Total { get; set; }
        public int KitKat { get; set; }
        public int FifthAvenue { get; set; }
        public int Butterfinger { get; set; }
        public int Crunch { get; set; }
    }
}