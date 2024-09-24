using Microsoft.EntityFrameworkCore;
using WWC._240924.Serializer.Entities;

namespace WWC._240924.Serializer
{
    public class JCenterDbContext : DbContext
    {
        public JCenterDbContext()
        {

        }

        public JCenterDbContext(DbContextOptions<JCenterDbContext> options) : base(options)
        {

        }

        public virtual DbSet<ModelMappingDetailEntity> ModelMappingDetailEntitys { get; set; }

        public virtual DbSet<ModelMappingEntity> ModelMappingEntitys { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer("")
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置 ModelMappingEntity 与 ModelMappingDetailEntity 的关系
            modelBuilder.Entity<ModelMappingEntity>()
                .HasKey(m => m.ID); // 主键

            // 配置 ModelMappingDetailEntity 的主键
            modelBuilder.Entity<ModelMappingDetailEntity>()
                .HasKey(m => m.LevelNumber); // 设置 LevelNumber 为主键

            // 一对多关系，ModelMappingEntity 与 ModelMappingDetailEntity
            modelBuilder.Entity<ModelMappingDetailEntity>()
                .HasOne(d => d.Mapping)
                .WithMany(m => m.MappingDetails)
                .HasForeignKey(d => d.MainID) //设置外键 MainID
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<ModelMappingEntity>()
            //    .HasMany(m => m.MappingDetails)
            //    .WithOne(m => m.Mapping)
            //    .HasForeignKey(m => m.MainID)
            //    .OnDelete(DeleteBehavior.Cascade);

            // 父级关系，ParentId
            modelBuilder.Entity<ModelMappingDetailEntity>()
                .HasOne<ModelMappingDetailEntity>()
                .WithMany()
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // 禁止级联删除
        }

    }
}
