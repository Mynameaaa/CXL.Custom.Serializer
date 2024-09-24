using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWC._240924.Serializer.Models
{
    public class BasePageResult<T>
    {

        public int TotalCount { get; set; }

        public int PageCount { get; set; }

        public List<T> Items { get; set; }

    }

    public class PageResult<T> : BasePageResult<T>
    {

    }
}
