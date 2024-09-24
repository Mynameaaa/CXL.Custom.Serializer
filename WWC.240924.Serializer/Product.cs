using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWC._240924.Serializer
{
    public class Product
    {
        public string ProductName { get; set; }

        public DateTime TimeNow { get; set; }

        public ProductDetails Details { get; set; }

        public ProductType ProductType { get; set; }

        public string[] Array { get; set; }

        public Tag[] Tags { get; set; }
    }

    public class ProductDetails
    {
        public string Description { get; set; }
        public double Price { get; set; }

        public Weight Weight { get; set; }
    }

    public class Weight
    {
        public int ProductSize { get; set; }

        public int ProductWeight { get; set; }
    }

    public class Tag
    {
        public string TagName { get; set; }

        public double Price { get; set; }
    }

    public enum ProductType
    {
        A = 1,

        B = 2,

        C = 3,

        D = 4,
    }

}
