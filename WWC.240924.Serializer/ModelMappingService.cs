using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WWC._240924.Serializer.Entities;
using WWC._240924.Serializer.Models;

namespace WWC._240924.Serializer
{
    public class ModelMappingService : IModelMappingService
    {
        private readonly ILogger<ModelMappingService> _logger;
        private readonly JCenterDbContext _context;

        public ModelMappingService(ILogger<ModelMappingService> logger, JCenterDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// 分页获取 Model 映射信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<PageResult<ModelMappingDTO>> GetModelMappingPageList(PageModel model)
        {
            var result = new PageResult<ModelMappingDTO>();

            var mappings = await _context.Set<ModelMappingEntity>()
                .Where(p => p.State == ModelMappingState.Added)
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .Include(p => p.MappingDetails)
                .ToListAsync();

            result.Items = mappings.Select(m => ConvertToDTO(m, m.MappingDetails)).ToList();
            result.TotalCount = await _context.ModelMappingEntitys.CountAsync();
            result.PageCount = result.TotalCount / model.PageSize;

            return result;
        }

        /// <summary>
        /// 根据编号获取 Model 映射配置
        /// </summary>
        /// <param name="modelID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ModelMappingDTO> GetModelMappingByID(int modelID)
        {
            var modelMapping = await _context.ModelMappingEntitys
                .Where(wh => wh.ID == modelID && wh.State == ModelMappingState.Added)
                .Include(i => i.MappingDetails)
                .FirstOrDefaultAsync();

            if (modelMapping == null)
            {
                throw new Exception("不存在的模型映射配置");
            }

            var result = ConvertToDTO(modelMapping, modelMapping.MappingDetails);

            return result;
        }

        /// <summary>
        /// 修改 Model 映射配置
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ModelMappingDTO> UpdateModelMapping(ModelMappingDTO model)
        {
            MapFieldTypeString(Type.GetType(model.ModelFullName), model);
            model.MappingDetails = TreeToList(model.MappingDetails);

            var existingModel = await _context.ModelMappingEntitys
                .Include(m => m.MappingDetails)
                .FirstOrDefaultAsync(m => m.ID == model.ID);

            if (existingModel == null)
            {
                throw new Exception("不存在的模型映射配置");
            }

            existingModel.ModelFullName = model.ModelFullName;
            existingModel.ModelTypeName = model.ModelTypeName;

            var newDetails = model.MappingDetails;
            var existingDetails = existingModel.MappingDetails;

            foreach (var existingDetail in existingDetails.ToList())
            {
                if (!newDetails.Any(d => d.LevelNumber == existingDetail.LevelNumber))
                {
                    _context.ModelMappingDetailEntitys.Remove(existingDetail);
                }
            }

            foreach (var newDetail in newDetails)
            {
                var existingDetail = existingDetails.FirstOrDefault(d => d.LevelNumber == newDetail.LevelNumber);
                if (existingDetail != null)
                {
                    existingDetail.JsonFieldName = newDetail.JsonFieldName;
                    existingDetail.FieldTypeString = newDetail.FieldTypeString;
                    existingDetail.PropertyName = newDetail.PropertyName;
                    existingDetail.Type = newDetail.Type;
                }
                else
                {
                    var detailEntity = new ModelMappingDetailEntity
                    {
                        MainID = existingModel.ID,
                        JsonFieldName = newDetail.JsonFieldName,
                        FieldTypeString = newDetail.FieldTypeString,
                        PropertyName = newDetail.PropertyName,
                        Type = newDetail.Type,
                        ParentId = newDetail.ParentId
                    };
                    _context.ModelMappingDetailEntitys.Add(detailEntity);
                }
            }
            await _context.SaveChangesAsync();

            var updatedModel = new ModelMappingDTO
            {
                ID = existingModel.ID,
                ModelFullName = existingModel.ModelFullName,
                ModelTypeName = existingModel.ModelTypeName,
                MappingDetails = model.MappingDetails
            };

            return updatedModel;
        }

        /// <summary>
        /// 添加 Model 配置信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task AddModelMappingWithDetails(ModelMappingDTO model)
        {
            var modelEntity = new ModelMappingEntity
            {
                ModelFullName = model.ModelFullName,
                ModelTypeName = model.ModelTypeName,
                State = ModelMappingState.Added,
                MappingDetails = new List<ModelMappingDetailEntity>()
            };

            foreach (var detailEntity in model.MappingDetails)
            {
                detailEntity.LevelNumber = Yitter.IdGenerator.YitIdHelper.NextId();
                if (detailEntity.Children != null && detailEntity.Children.Any())
                {
                    detailEntity.Children = BuildChildrenStructure(detailEntity.LevelNumber, detailEntity.Children);
                }
            }

            model.MappingDetails = TreeToList(model.MappingDetails);

            foreach (var detail in model.MappingDetails)
            {
                var detailEntity = new ModelMappingDetailEntity
                {
                    JsonFieldName = detail.JsonFieldName,
                    FieldTypeString = detail.FieldTypeString,
                    PropertyName = detail.PropertyName,
                    Type = detail.Type,
                    ParentId = detail.ParentId,
                    LevelNumber = detail.LevelNumber,
                };

                modelEntity.MappingDetails.Add(detailEntity);
            }

            _context.ModelMappingEntitys.Add(modelEntity);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 删除 Model 映射配置
        /// </summary>
        /// <param name="detailIds"></param>
        /// <returns></returns>
        public async Task DeleteModelMappingDetails(List<int> detailIds)
        {
            foreach (var detailId in detailIds)
            {
                var existingDetail = await _context.ModelMappingEntitys
                    .FirstOrDefaultAsync(d => d.ID == detailId);

                if (existingDetail != null)
                {
                    existingDetail.State = ModelMappingState.Delete;
                    _context.ModelMappingEntitys.Update(existingDetail);
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 构建层级关系
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="childrens"></param>
        /// <returns></returns>
        private List<ModelMappingDetailDTO> BuildChildrenStructure(long? parentID, List<ModelMappingDetailDTO> childrens)
        {
            List<ModelMappingDetailDTO> result = new List<ModelMappingDetailDTO>();

            foreach (var node in childrens)
            {
                node.ParentId = parentID;
                node.LevelNumber = Yitter.IdGenerator.YitIdHelper.NextId();
                result.Add(node);
                if (node.Children != null && node.Children.Any())
                {
                    node.Children = BuildChildrenStructure(node.LevelNumber, node.Children);
                }
            }

            return result;
        }

        /// <summary>
        /// 转为 DTO 数据
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="allDetails"></param>
        /// <returns></returns>
        private ModelMappingDTO ConvertToDTO(ModelMappingEntity entity, List<ModelMappingDetailEntity> allDetails)
        {
            var dto = new ModelMappingDTO
            {
                ID = entity.ID,
                ModelFullName = entity.ModelFullName,
                ModelTypeName = entity.ModelTypeName,
                MappingDetails = BuildDetailHierarchy(null, entity.ID, allDetails),
            };

            return dto;
        }

        /// <summary>
        /// 构建树形 
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="mainId"></param>
        /// <param name="allDetails"></param>
        /// <returns></returns>
        private List<ModelMappingDetailDTO> BuildDetailHierarchy(long? parentId, int mainId, List<ModelMappingDetailEntity> allDetails)
        {
            var details = allDetails
                .Where(d => d.ParentId == parentId && d.MainID == mainId)
                .ToList();

            var result = new List<ModelMappingDetailDTO>();
            foreach (var detail in details)
            {
                var dto = new ModelMappingDetailDTO
                {
                    MainID = detail.MainID,
                    ParentId = detail.ParentId,
                    JsonFieldName = detail.JsonFieldName,
                    FieldTypeString = detail.FieldTypeString,
                    PropertyName = detail.PropertyName,
                    Type = detail.Type,
                    LevelNumber = detail.LevelNumber,
                };

                dto.Children = BuildDetailHierarchy(detail.LevelNumber, detail.MainID, allDetails);

                result.Add(dto);
            }

            return result;
        }

        //public void MapFieldTypeString(object obj, ModelMappingDTO mappingEntity)
        //{
        //    if (mappingEntity == null || obj == null) return;

        //    // 获取 obj 类型的 Type 信息
        //    Type type = obj.GetType();

        //    // 遍历 mappingEntity 中的 MappingDetails
        //    foreach (var detail in mappingEntity.MappingDetails)
        //    {
        //        // 找到当前 JsonFieldName 对应的 PropertyInfo
        //        var propertyInfo = type.GetProperty(detail.PropertyName);
        //        if (propertyInfo != null)
        //        {
        //            // 获取完全限定类型名称
        //            detail.FieldTypeString = propertyInfo.PropertyType.FullName;

        //            // 处理嵌套的复杂类型，递归调用
        //            if (detail.Children != null && detail.Children.Count > 0)
        //            {
        //                // 获取当前属性的值，并递归传入子级进行类型获取
        //                var propertyValue = propertyInfo.GetValue(obj);
        //                if (propertyValue != null)
        //                {
        //                    foreach (var childDetail in detail.Children)
        //                    {
        //                        MapFieldTypeString(propertyValue, new ModelMappingDTO { MappingDetails = detail.Children });
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        private static void MapFieldTypeString(Type parentType, ModelMappingDTO mappingEntity)
        {
            if (mappingEntity == null || parentType == null) return;

            foreach (var detail in mappingEntity.MappingDetails)
            {
                var propertyInfo = parentType.GetProperty(detail.PropertyName);
                if (propertyInfo != null)
                {
                    Type propertyType = propertyInfo.PropertyType;
                    Type lastType = propertyType;

                    if (propertyType.IsArray)
                    {
                        lastType = propertyType.GetElementType();
                        detail.FieldTypeString = lastType.FullName;
                    }
                    else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propertyType) && propertyType.IsGenericType)
                    {
                        lastType = propertyType.GetGenericArguments()[0];
                        detail.FieldTypeString = propertyType.GetGenericTypeDefinition().FullName;
                    }
                    else
                    {
                        detail.FieldTypeString = propertyType.FullName;
                    }

                    if (detail.Children != null && detail.Children.Count > 0)
                    {
                        MapFieldTypeString(lastType, new ModelMappingDTO { MappingDetails = detail.Children });
                    }
                }
            }
        }

        /// <summary>
        /// 树形结构转成集合
        /// </summary>
        /// <param name="allDetails">所有的节点集合</param>
        /// <returns>平展的节点集合</returns>
        private List<ModelMappingDetailDTO> TreeToList(List<ModelMappingDetailDTO> allDetails)
        {
            List<ModelMappingDetailDTO> flatList = new List<ModelMappingDetailDTO>();

            // 找到树的根节点，假设根节点 ParentId 为 null 或 0
            var rootNodes = allDetails.Where(detail => detail.ParentId == null || detail.ParentId == 0).ToList();

            // 递归将树的每个节点加入到列表中
            foreach (var rootNode in rootNodes)
            {
                AddNodeAndChildrenToList(flatList, rootNode);
            }

            return flatList;
        }

        /// <summary>
        /// 将子节点数据获取出来
        /// </summary>
        /// <param name="flatList"></param>
        /// <param name="rootNode"></param>
        /// <returns></returns>
        private List<ModelMappingDetailDTO> AddNodeAndChildrenToList(List<ModelMappingDetailDTO> flatList, ModelMappingDetailDTO rootNode)
        {
            flatList.Add(rootNode);
            if (rootNode.Children != null && rootNode.Children.Any())
            {
                foreach (var node in rootNode.Children)
                {
                    AddNodeAndChildrenToList(flatList, node);
                }
            }

            return flatList;
        }

        /// <summary>
        /// 递归方法，将节点及其子节点添加到列表中
        /// </summary>
        /// <param name="node">当前节点</param>
        /// <param name="allDetails">所有的节点集合</param>
        /// <param name="flatList">平展的节点集合</param>
        private void AddNodeAndChildrenToList(ModelMappingDetailDTO node, List<ModelMappingDetailDTO> allDetails, List<ModelMappingDetailDTO> flatList)
        {
            // 将当前节点转换为 DTO 并添加到列表中
            flatList.Add(new ModelMappingDetailDTO
            {
                ParentId = node.ParentId,
                PropertyName = node.PropertyName,
                JsonFieldName = node.JsonFieldName,
                FieldTypeString = node.FieldTypeString,
                Children = node.Children,
                // 添加其他需要的属性
            });

            // 查找所有子节点
            var childNodes = allDetails.Where(detail => detail.ParentId == node.LevelNumber).ToList();

            // 递归处理子节点
            foreach (var childNode in childNodes)
            {
                AddNodeAndChildrenToList(childNode, allDetails, flatList);
            }
        }

    }
}
