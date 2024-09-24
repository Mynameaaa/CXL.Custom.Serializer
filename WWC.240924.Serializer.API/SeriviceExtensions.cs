using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Serialization;

namespace WWC._240924.Serializer.API
{
    public static class SeriviceExtensions
    {
        public static IServiceCollection AddCustomControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true; // 全局禁用模型状态验证
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        ProcessDictionaryKeys = true,
                        OverrideSpecifiedNames = false
                    }
                };

                options.SerializerSettings.Converters.Add(new CustomJsonConverter(new JCenterDbContext()));
            });

            return services;
        }

    }
}
