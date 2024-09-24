using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWC._240924.Serializer
{
    public class ObjectTypeInfo
    {
        public int ID { get; set; }

        public string TypeName { get; set; }

        public string FullName { get; set; }

        public List<ObjectJsonMapping> Mappings { get; set; }


        public static List<ObjectTypeInfo> ObjectTypeInfos = new List<ObjectTypeInfo>()
        {
            new ObjectTypeInfo()
            {
                FullName  = typeof(Product).FullName ?? string.Empty,
                TypeName  = typeof(Product).Name,
                ID = 1,
                Mappings = new List<ObjectJsonMapping>()
                {
                    new ObjectJsonMapping()
                    {
                        ID = 1,
                        PropertyName = "ProductName",
                        JsonFieldName = "ProductTitle",
                        Type = PropertyType.Ordinary,
                        ParentId = null,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 10,
                        PropertyName = "Array",
                        JsonFieldName = "Array",
                        Type = PropertyType.Array,
                        ParentId = null,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 2,
                        PropertyName = "Details",
                        JsonFieldName = "Details",
                        Type = PropertyType.Object,
                        ParentId = null,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 11,
                        PropertyName = "Weight",
                        JsonFieldName = "Weight",
                        Type = PropertyType.Object,
                        ParentId = 2,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 12,
                        PropertyName = "ProductSize",
                        JsonFieldName = "Size",
                        Type = PropertyType.Ordinary,
                        ParentId = 11,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 13,
                        PropertyName = "ProductWeight",
                        JsonFieldName = "Weight",
                        Type = PropertyType.Ordinary,
                        ParentId = 11,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 3,
                        PropertyName = "Description",
                        JsonFieldName = "Description",
                        Type = PropertyType.Ordinary,
                        ParentId = 2,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 4,
                        PropertyName = "Price",
                        JsonFieldName = "Price",
                        Type = PropertyType.Ordinary,
                        ParentId = 2,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 5,
                        PropertyName = "Tags",
                        JsonFieldName = "Tags",
                        Type = PropertyType.Array,
                        ParentId = null,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 6,
                        PropertyName = "TagName",
                        JsonFieldName = "TagName",
                        Type = PropertyType.Ordinary,
                        ParentId = 5,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 9,
                        PropertyName = "Price",
                        JsonFieldName = "Price",
                        Type = PropertyType.Ordinary,
                        ParentId = 5,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 7,
                        PropertyName = "TimeNow",
                        JsonFieldName = "DateTime",
                        Type = PropertyType.Ordinary,
                        ParentId = null,
                        MainID = 1,
                    },
                    new ObjectJsonMapping()
                    {
                        ID = 8,
                        PropertyName = "ProductType",
                        JsonFieldName = "ProductType",
                        Type = PropertyType.Ordinary,
                        ParentId = null,
                        MainID = 1,
                    },
                }
            }
        };
    }

    public class ObjectJsonMapping
    {
        public int ID { get; set; }

        public int MainID { get; set; }

        public int? ParentId { get; set; }

        public string JsonFieldName { get; set; }

        public string FieldTypeString { get; set; }

        public string PropertyName { get; set; }

        public PropertyType Type { get; set; }

    }

    public enum PropertyType
    {
        Ordinary,

        Object,

        Array
    }
}
