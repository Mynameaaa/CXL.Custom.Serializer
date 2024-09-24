using System;
using System.Collections.Generic;
using System.Linq;

namespace WWC._240924.Serializer
{
    public class PropertyInit
    {
        public static void InitializeFieldTypeStrings()
        {
            foreach (var objectTypeInfo in ObjectTypeInfo.ObjectTypeInfos)
            {
                var objectType = Type.GetType(objectTypeInfo.FullName); // 获取类型信息
                if (objectType == null)
                {
                    continue;
                }

                foreach (var mapping in objectTypeInfo.Mappings)
                {
                    SetFieldTypeStringRecursive(objectType, mapping);
                }
            }
        }

        private static void SetFieldTypeStringRecursive(Type objectType, ObjectJsonMapping mapping)
        {
            // 初始化 FieldTypeString 防止 NullReferenceException
            mapping.FieldTypeString ??= "Unknown";

            // 查找属性
            var propertyInfo = objectType.GetProperty(mapping.PropertyName);

            // 确保 propertyInfo 不为 null
            if (propertyInfo != null)
            {
                // 处理数组、集合、对象、枚举及基本类型
                if (propertyInfo.PropertyType.IsArray || IsCollection(propertyInfo.PropertyType))
                {
                    mapping.FieldTypeString = "Array of " + GetElementTypeName(propertyInfo.PropertyType);
                }
                else if (propertyInfo.PropertyType.IsEnum)
                {
                    mapping.FieldTypeString = "Enum of " + propertyInfo.PropertyType.Name;
                }
                else if (propertyInfo.PropertyType == typeof(DateTime))
                {
                    mapping.FieldTypeString = "DateTime";
                }
                else if (propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType != typeof(string))
                {
                    mapping.FieldTypeString = "Object of " + propertyInfo.PropertyType.Name;

                    // 获取嵌套属性的映射
                    var nestedType = propertyInfo.PropertyType;
                    var nestedMappings = ObjectTypeInfo.ObjectTypeInfos.FirstOrDefault(oti => oti.FullName == nestedType.FullName)?.Mappings;

                    if (nestedMappings != null)
                    {
                        foreach (var nestedMapping in nestedMappings)
                        {
                            SetFieldTypeStringRecursive(nestedType, nestedMapping); // 递归调用
                        }
                    }
                }
                else
                {
                    mapping.FieldTypeString = propertyInfo.PropertyType.Name;
                }
            }
            else
            {
                // 如果没有找到对应的属性，设置为未知类型
                mapping.FieldTypeString = "Unknown";
            }
        }

        // 获取数组或集合的元素类型名称
        private static string GetElementTypeName(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType()?.Name ?? "Unknown";
            }

            if (type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                return type.GetGenericArguments()[0].Name;
            }

            return "Unknown";
        }

        // 判断类型是否为集合类型
        private static bool IsCollection(Type type)
        {
            return type.IsArray || (type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()));
        }
    }
}
