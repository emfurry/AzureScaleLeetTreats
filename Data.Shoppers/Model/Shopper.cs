using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureScaleLeetTreats.Data.Shoppers.Model
{
    public class Shopper
    {
        [Key]
        public int ShopperID { get; set; }
        [Required]
        [Index(IsUnique = true)]
        [StringLength(450, MinimumLength = 4)]
        public string UserName { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
