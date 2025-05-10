using FruitDiseaseDetection.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using FruitDiseaseDetection.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// CORS ayarları
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
// HttpClient'i PredictionService için ekleyelim
builder.Services.AddHttpClient<PredictionService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

builder.WebHost.UseUrls("http://localhost:80");

builder.Services.AddDbContext<FruitDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
app.UseCors(); 

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.MapScalarApiReference();
    app.MapOpenApi();
// }

using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FruitDbContext>();
    db.Database.Migrate();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
