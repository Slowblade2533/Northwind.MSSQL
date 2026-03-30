using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Northwind.MSSQL.Data;
using System.IO.Compression;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("NorthwindDb") ?? throw new InvalidOperationException("Connection string 'NorthwindDb' not found.");

builder.Services.AddDbContext<NorthwindContext>(
    options =>
    {
        options.UseSqlServer(connectionString);
    }
);

var app = builder.Build();


app.UseResponseCompression();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    //(ทางเลือก)ปิด HttpsRedirection ไว้กรณีรันบน Production Docker ที่มี Reverse Proxy จัดการ HTTPS ให้แล้ว
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
