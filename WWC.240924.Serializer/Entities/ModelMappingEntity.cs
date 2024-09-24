using System.ComponentModel.DataAnnotations;
using WWC._240924.Serializer.Models;

namespace WWC._240924.Serializer.Entities
{
    public class ModelMappingEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// 类型完全限定名称
        /// </summary>
        public string ModelFullName { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        public string ModelTypeName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public ModelMappingState State { get; set; }

        /// <summary>
        /// 映射详细信息
        /// </summary>
        public List<ModelMappingDetailEntity> MappingDetails { get; set; }

    }
}
