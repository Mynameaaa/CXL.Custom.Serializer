using WWC._240924.Serializer.Models;

namespace WWC._240924.Serializer
{
    public interface IModelMappingService
    {

        /// <summary>
        /// 分页获取对象配置信息
        /// </summary>
        /// <returns></returns>
        Task<PageResult<ModelMappingDTO>> GetModelMappingPageList(PageModel model);

        /// <summary>
        /// 根据编号获取 Model 映射配置
        /// </summary>
        /// <param name="modelID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        Task<ModelMappingDTO> GetModelMappingByID(int modelID);

        /// <summary>
        /// 添加 Model 配置信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task AddModelMappingWithDetails(ModelMappingDTO model);

        /// <summary>
        /// 修改 Model 映射配置
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ModelMappingDTO> UpdateModelMapping(ModelMappingDTO model);


        /// <summary>
        /// 删除 Model 映射配置
        /// </summary>
        /// <param name="detailIds"></param>
        /// <returns></returns>
        Task DeleteModelMappingDetails(List<int> detailIds);

    }
}
