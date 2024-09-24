using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWC._240924.Serializer.Entities
{
    public class ModelMappingDetailEntity
    {
        /// <summary>
        /// 层级编号，用于构建层级
        /// </summary>
        [Key]
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
        [NotMapped]
        public List<ModelMappingDetailEntity> Children { get; set; }

    }
}
