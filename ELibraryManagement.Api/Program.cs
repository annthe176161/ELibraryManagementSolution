
using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Formatters;
using ELibraryManagement.Api.Services.Implementations;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using System.Text;

namespace ELibraryManagement.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register services
            builder.Services.AddScoped<IBookService, BookService>();

            // Add OData
            var modelBuilder = new ODataConventionModelBuilder();
            var books = modelBuilder.EntitySet<BookDto>("Books");
            books.EntityType.HasMany(b => b.Categories);

            // Configure MVC with formatters
            builder.Services.AddControllers(options =>
            {
                // Add XML formatter with custom settings
                var xmlFormatter = new Microsoft.AspNetCore.Mvc.Formatters.XmlDataContractSerializerOutputFormatter();
                xmlFormatter.WriterSettings.Indent = true;
                xmlFormatter.WriterSettings.OmitXmlDeclaration = false;
                options.OutputFormatters.Add(xmlFormatter);

                options.InputFormatters.Add(new Microsoft.AspNetCore.Mvc.Formatters.XmlDataContractSerializerInputFormatter(options));

                // Add custom CSV formatter
                options.OutputFormatters.Add(new CsvOutputFormatter());
            }).AddOData(
                options => options
                    .Select()
                    .Filter()
                    .OrderBy()
                    .Expand()
                    .Count()
                    .SetMaxTop(100)
                    .AddRouteComponents("odata", modelBuilder.GetEdmModel())
            );
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "ELibrary Management API",
                    Version = "v1",
                    Description = "API for managing electronic library with Content Negotiation support"
                });

                // Configure Swagger to show all supported media types
                options.OperationFilter<AcceptHeaderOperationFilter>();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
