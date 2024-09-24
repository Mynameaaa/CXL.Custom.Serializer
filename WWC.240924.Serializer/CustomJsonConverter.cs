using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;
using WWC._240924.Serializer.Entities;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace WWC._240924.Serializer
{
    public class CustomJsonConverter : JsonConverter
    {
        private readonly JCenterDbContext _dbContext;
        private List<ModelMappingEntity> modelMappingEntities = new List<ModelMappingEntity>();

        public CustomJsonConverter(JCenterDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override bool CanConvert(Type objectType)
        {
            if (modelMappingEntities.Count <= 0)
            {
                modelMappingEntities = _dbContext.ModelMappingEntitys.Include(i => i.MappingDetails).ToList();
            }
            return modelMappingEntities.Any(m => m.ModelFullName == objectType.FullName || m.ModelTypeName == objectType.Name);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject json = JObject.Load(reader);

            var result = Activator.CreateInstance(objectType);

            var mappingInfo = modelMappingEntities.FirstOrDefault(m => m.ModelFullName == objectType.FullName || m.ModelTypeName == objectType.Name);

            if (mappingInfo != null)
            {
                MapObject(result, json, null, mappingInfo.MappingDetails);
            }

            return result;
        }

        #region Json 转成 Object

        //private void MapObject(object target, JObject json, int? parentId, List<ModelMappingDetailEntity> mappings)
        //{
        //    var fields = mappings.Where(f => f.ParentId == parentId).ToList();

        //    foreach (var field in fields)
        //    {
        //        var jsonFieldValue = json.Properties()
        //            .FirstOrDefault(p => string.Equals(p.Name, field.JsonFieldName, StringComparison.OrdinalIgnoreCase))?.Value;

        //        if (jsonFieldValue == null)
        //        {
        //            continue;
        //        }

        //        var prop = target.GetType().GetProperty(field.PropertyName);
        //        if (prop == null)
        //        {
        //            continue;
        //        }

        //        // 根据 FieldTypeString 进行类型判断
        //        switch (field.FieldTypeString?.ToLower() ?? "")
        //        {
        //            case "string":
        //                prop.SetValue(target, jsonFieldValue.ToString());
        //                break;

        //            case "int":
        //            case "int32":
        //                prop.SetValue(target, Convert.ToInt32(jsonFieldValue));
        //                break;

        //            case "long":
        //            case "int64":
        //                prop.SetValue(target, Convert.ToInt64(jsonFieldValue));
        //                break;
        //            case "datetime":
        //                if (DateTime.TryParse(jsonFieldValue.ToString(), out DateTime dateTimeValue))
        //                {
        //                    prop.SetValue(target, dateTimeValue);
        //                }
        //                break;

        //            case "double":
        //                prop.SetValue(target, Convert.ToDouble(jsonFieldValue));
        //                break;

        //            // 如果需要处理其他内置类型，可以继续添加
        //            default:
        //                if (field.Type == PropertyType.Ordinary)
        //                {
        //                    prop.SetValue(target, Convert.ChangeType(jsonFieldValue, prop.PropertyType));
        //                }
        //                else if (field.Type == PropertyType.Object && jsonFieldValue is JObject)
        //                {
        //                    var childObject = Activator.CreateInstance(prop.PropertyType);
        //                    MapObject(childObject, (JObject)jsonFieldValue, field.ID, mappings);
        //                    prop.SetValue(target, childObject);
        //                }
        //                else if (field.Type == PropertyType.Array && jsonFieldValue is JArray)
        //                {
        //                    var propertyType = prop.PropertyType;

        //                    // 判断是否为数组或泛型集合
        //                    if (propertyType.IsArray)
        //                    {
        //                        var elementType = propertyType.GetElementType();
        //                        if (elementType == null) continue;

        //                        var array = Array.CreateInstance(elementType, ((JArray)jsonFieldValue).Count);
        //                        for (int i = 0; i < ((JArray)jsonFieldValue).Count; i++)
        //                        {
        //                            var itemValue = ((JArray)jsonFieldValue)[i];
        //                            if (itemValue is JObject)
        //                            {
        //                                var childObject = Activator.CreateInstance(elementType);
        //                                MapObject(childObject, (JObject)itemValue, field.ID, mappings);
        //                                array.SetValue(childObject, i);
        //                            }
        //                            else
        //                            {
        //                                array.SetValue(Convert.ChangeType(itemValue, elementType), i);
        //                            }
        //                        }
        //                        prop.SetValue(target, array);
        //                    }
        //                    else if (propertyType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(propertyType.GetGenericTypeDefinition()))
        //                    {
        //                        var elementType = propertyType.GetGenericArguments().First();
        //                        var listType = typeof(List<>).MakeGenericType(elementType);
        //                        var listInstance = (IList)Activator.CreateInstance(listType);

        //                        foreach (var item in (JArray)jsonFieldValue)
        //                        {
        //                            if (item is JObject)
        //                            {
        //                                var childObject = Activator.CreateInstance(elementType);
        //                                MapObject(childObject, (JObject)item, field.ID, mappings);
        //                                listInstance.Add(childObject);
        //                            }
        //                            else
        //                            {
        //                                listInstance.Add(Convert.ChangeType(item, elementType));
        //                            }
        //                        }
        //                        prop.SetValue(target, listInstance);
        //                    }
        //                }
        //                break;
        //        }
        //    }
        //}

        private void MapObject(object target, JObject json, long? parentId, List<ModelMappingDetailEntity> mappings)
        {
            var fields = mappings.Where(f => f.ParentId == parentId).ToList();

            foreach (var field in fields)
            {
                var jsonFieldValue = json.Properties()
                    .FirstOrDefault(p => string.Equals(p.Name, field.JsonFieldName, StringComparison.OrdinalIgnoreCase))?.Value;

                if (jsonFieldValue == null)
                {
                    continue;
                }

                var prop = target.GetType().GetProperty(field.PropertyName);
                if (prop == null)
                {
                    continue;
                }

                var fieldTypeString = field.FieldTypeString?.ToLower() ?? "";
                var propertyType = prop.PropertyType;

                if (Type.GetType(fieldTypeString) != null)
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(jsonFieldValue.ToObject(propertyType), propertyType);
                        prop.SetValue(target, convertedValue);
                    }
                    catch
                    {
                        // 无法转换时跳过
                        continue;
                    }
                }
                else
                {
                    // 非内置类型的自定义处理
                    switch (fieldTypeString)
                    {
                        case "string":
                            prop.SetValue(target, jsonFieldValue.ToString());
                            break;

                        case "int":
                        case "int32":
                            prop.SetValue(target, Convert.ToInt32(jsonFieldValue));
                            break;

                        case "long":
                        case "int64":
                            prop.SetValue(target, Convert.ToInt64(jsonFieldValue));
                            break;

                        case "decimal":
                            prop.SetValue(target, Convert.ToDecimal(jsonFieldValue));
                            break;

                        case "datetime":
                            if (DateTime.TryParse(jsonFieldValue.ToString(), out DateTime dateTimeValue))
                            {
                                prop.SetValue(target, dateTimeValue);
                            }
                            break;

                        case "double":
                            prop.SetValue(target, Convert.ToDouble(jsonFieldValue));
                            break;
                        default:
                            // 处理复杂类型，如对象、数组
                            if (field.Type == PropertyType.Ordinary)
                            {
                                prop.SetValue(target, Convert.ChangeType(jsonFieldValue, propertyType));
                            }
                            else if (field.Type == PropertyType.Object && jsonFieldValue is JObject)
                            {
                                var childObject = Activator.CreateInstance(propertyType);
                                MapObject(childObject, (JObject)jsonFieldValue, field.LevelNumber, mappings);
                                prop.SetValue(target, childObject);
                            }
                            else if (field.Type == PropertyType.Array && jsonFieldValue is JArray)
                            {
                                HandleArrayField(prop, target, jsonFieldValue, field.LevelNumber, mappings);
                            }
                            break;
                    }
                }
            }
        }

        private void HandleArrayField(PropertyInfo prop, object target, JToken jsonFieldValue, long? fieldId, List<ModelMappingDetailEntity> mappings)
        {
            var propertyType = prop.PropertyType;

            // 判断是否为数组或泛型集合
            if (propertyType.IsArray)
            {
                var elementType = propertyType.GetElementType();
                if (elementType == null) return;

                var array = Array.CreateInstance(elementType, ((JArray)jsonFieldValue).Count);
                for (int i = 0; i < ((JArray)jsonFieldValue).Count; i++)
                {
                    var itemValue = ((JArray)jsonFieldValue)[i];
                    var childObject = CreateOrConvertToElementType(itemValue, elementType, fieldId, mappings);
                    array.SetValue(childObject, i);
                }
                prop.SetValue(target, array);
            }
            else if (propertyType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(propertyType.GetGenericTypeDefinition()))
            {
                var elementType = propertyType.GetGenericArguments().First();
                var listType = typeof(List<>).MakeGenericType(elementType);
                var listInstance = (IList)Activator.CreateInstance(listType);

                foreach (var item in (JArray)jsonFieldValue)
                {
                    var childObject = CreateOrConvertToElementType(item, elementType, fieldId, mappings);
                    listInstance.Add(childObject);
                }
                prop.SetValue(target, listInstance);
            }
        }

        private object CreateOrConvertToElementType(JToken itemValue, Type elementType, long? fieldId, List<ModelMappingDetailEntity> mappings)
        {
            if (itemValue is JObject)
            {
                var childObject = Activator.CreateInstance(elementType);
                MapObject(childObject, (JObject)itemValue, fieldId, mappings);
                return childObject;
            }
            else
            {
                return Convert.ChangeType(itemValue, elementType);
            }
        }


        #endregion

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var jsonObject = new JObject();
            var mappings = modelMappingEntities.FirstOrDefault(f => f.ModelFullName == value.GetType().FullName || f.ModelTypeName == value.GetType().Name)?.MappingDetails;

            if (mappings == null)
            {
                return;
            }

            MapObjectToJson(value, jsonObject, mappings, serializer);

            jsonObject.WriteTo(writer);
        }

        #region Object 转成 Json

        //private void MapObjectToJson(object target, JObject jsonObject, List<ModelMappingDetailEntity> mappings, Newtonsoft.Json.JsonSerializer serializer)
        //{
        //    var properties = target.GetType().GetProperties();

        //    foreach (var prop in properties)
        //    {
        //        object propValue = null;
        //        try
        //        {
        //            propValue = prop.GetValue(target);
        //        }
        //        catch
        //        {
        //            continue; // 跳过无法获取的属性
        //        }

        //        var mappingInfo = mappings.FirstOrDefault(m => m.PropertyName == prop.Name);
        //        var jsonFieldName = mappingInfo?.JsonFieldName ?? prop.Name;

        //        // 处理嵌套对象和数组
        //        if (propValue != null)
        //        {
        //            if (mappingInfo != null && mappingInfo.Type == PropertyType.Object)
        //            {
        //                var childObject = new JObject();
        //                MapObjectToJson(propValue, childObject, mappings, serializer);
        //                jsonObject[jsonFieldName] = childObject;
        //            }
        //            else if (mappingInfo != null && mappingInfo.Type == PropertyType.Array)
        //            {
        //                var jsonArray = new JArray();
        //                foreach (var item in (IEnumerable)propValue)
        //                {
        //                    if (item != null)
        //                    {
        //                        var childObject = new JObject();
        //                        MapObjectToJson(item, childObject, mappings, serializer);
        //                        jsonArray.Add(childObject);
        //                    }
        //                }
        //                jsonObject[jsonFieldName] = jsonArray;
        //            }
        //            //else if (mappingInfo != null && mappingInfo.Type == FieldType.Generic)
        //            //{
        //            //    // 处理泛型类型（例如 List<T>）
        //            //    var jsonArray = new JArray();
        //            //    foreach (var item in (IEnumerable)propValue)
        //            //    {
        //            //        if (item != null)
        //            //        {
        //            //            var childObject = new JObject();
        //            //            MapObjectToJson(item, childObject, mappings, serializer);
        //            //            jsonArray.Add(childObject);
        //            //        }
        //            //    }
        //            //    jsonObject[jsonFieldName] = jsonArray;
        //            //}
        //            else
        //            {
        //                // 处理基本类型和其他类型
        //                jsonObject[jsonFieldName] = JToken.FromObject(propValue, serializer);
        //            }
        //        }
        //        else
        //        {
        //            // 处理 null 值
        //            jsonObject[jsonFieldName] = null;
        //        }
        //    }
        //}

        private void MapObjectToJson(object target, JObject jsonObject, List<ModelMappingDetailEntity> mappings, Newtonsoft.Json.JsonSerializer serializer)
        {
            var properties = target.GetType().GetProperties();

            foreach (var prop in properties)
            {
                object propValue = null;
                try
                {
                    propValue = prop.GetValue(target);
                }
                catch
                {
                    continue; // 跳过无法获取的属性
                }

                var mappingInfo = mappings.FirstOrDefault(m => m.PropertyName == prop.Name);
                var jsonFieldName = mappingInfo?.JsonFieldName ?? prop.Name;

                // 处理嵌套对象和数组
                if (propValue != null)
                {
                    if (mappingInfo != null && mappingInfo.Type == PropertyType.Object)
                    {
                        var childObject = new JObject();
                        MapObjectToJson(propValue, childObject, mappings, serializer);
                        jsonObject[jsonFieldName] = childObject;
                    }
                    else if (mappingInfo != null && mappingInfo.Type == PropertyType.Array)
                    {
                        var jsonArray = new JArray();
                        foreach (var item in (IEnumerable)propValue)
                        {
                            if (item != null)
                            {
                                var childObject = new JObject();
                                MapObjectToJson(item, childObject, mappings, serializer);
                                jsonArray.Add(childObject);
                            }
                        }
                        jsonObject[jsonFieldName] = jsonArray;
                    }
                    else
                    {
                        // 处理基本类型和其他类型
                        jsonObject[jsonFieldName] = JToken.FromObject(propValue, serializer);
                    }
                }
                else
                {
                    jsonObject[jsonFieldName] = null;
                }
            }
        }


        #endregion

    }
}
