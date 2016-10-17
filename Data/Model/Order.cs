using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureScaleLeetTreats.Data.Model
{
    public class Order
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ShopperID { get; set; }
        [Key, Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderID { get; set; }
        public int ProductID { get; set; }

        public virtual Shopper Shopper { get; set; }
        public virtual Product Product { get; set; }
    }
}
