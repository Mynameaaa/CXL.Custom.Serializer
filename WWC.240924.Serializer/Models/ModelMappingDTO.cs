namespace WWC._240924.Serializer.Models
{
    public class ModelMappingDTO
    {

        /// <summary>
        /// 主键
        /// </summary>
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
        /// 映射详细信息
        /// </summary>
        public List<ModelMappingDetailDTO> MappingDetails { get; set; }

    }
}
