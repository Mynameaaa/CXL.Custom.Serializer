using WWC._240924.Serializer.Entities;

namespace WWC._240924.Serializer.Models
{
    public class ModelMappingDetailDTO
    {
        /// <summary>
        /// 层级编号，用于构建层级
        /// </summary>
        public long LevelNumber { get; set; }

        /// <summary>
        /// 主表编号
        /// </summary>
        public int MainID { get; set; }

        /// <summary>
        /// 父级编号
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// JsonKey 名称
        /// </summary>
        public string JsonFieldName { get; set; }

        /// <summary>
        /// 属性类型
        /// </summary>
        public string FieldTypeString { get; set; }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 属性类型
        /// </summary>
        public PropertyType Type { get; set; }

        /// <summary>
        /// 主表
        /// </summary>
        public ModelMappingEntity Mapping { get; set; }

        /// <summary>
        /// 子集属性
        /// </summary>
        public List<ModelMappingDetailDTO> Children { get; set; }

    }
}
