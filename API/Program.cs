using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references (Livro -> Generos -> Livros -> Generos)
        // Without this no results show in the frontend
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen();

// Configure Entity Framework DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookTalk API");
    c.RoutePrefix = "swagger"; // Swagger UI disponível em /swagger
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
