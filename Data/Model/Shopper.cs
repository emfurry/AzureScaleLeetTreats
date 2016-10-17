using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureScaleLeetTreats.Data.Model
{
    public class Shopper
    {
        [Key]
        public int ShopperID { get; set; }
        [Required]
        public string NickName { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual ICollection<Order> Order { get; set; }
    }
}
