using Microsoft.EntityFrameworkCore;
using WWC._240924.Serializer;
using WWC._240924.Serializer.API;

var builder = WebApplication.CreateBuilder(args);

PropertyInit.InitializeFieldTypeStrings();

builder.Services.AddCustomControllers();

builder.Services.AddDbContext<JCenterDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("JCenter")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Services

builder.Services.AddTransient<IModelMappingService, ModelMappingService>();

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
