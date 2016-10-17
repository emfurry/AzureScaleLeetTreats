using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureScaleLeetTreats.Data.Model
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ImageUrl { get; set; }

        public virtual ICollection<Order> Order { get; set; }
    }
}
