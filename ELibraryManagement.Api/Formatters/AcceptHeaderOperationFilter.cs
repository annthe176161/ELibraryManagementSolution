using ELibraryManagement.Api.DTOs;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace ELibraryManagement.Api.Formatters
{
    public class AcceptHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // Add Accept header parameter for content negotiation
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept",
                In = ParameterLocation.Header,
                Description = "Response format (application/json, application/xml, text/csv)",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny>
                    {
                        new OpenApiString("application/json"),
                        new OpenApiString("application/xml"),
                        new OpenApiString("text/csv")
                    },
                    Default = new OpenApiString("application/json")
                }
            });

            // Add response examples for different formats
            if (operation.Responses.ContainsKey("200"))
            {
                var response = operation.Responses["200"];

                // Only add if not already exists
                if (!response.Content.ContainsKey("application/json"))
                {
                    response.Content.Add("application/json", new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(IEnumerable<BookDto>), context.SchemaRepository)
                    });
                }

                // XML example
                if (!response.Content.ContainsKey("application/xml"))
                {
                    response.Content.Add("application/xml", new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(IEnumerable<BookDto>), context.SchemaRepository),
                        Example = new OpenApiString(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ArrayOfBookDto xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BookDto>
    <Id>1</Id>
    <Title>Đắc Nhân Tâm</Title>
    <Author>Dale Carnegie</Author>
    <ISBN>9786047770560</ISBN>
    <Publisher>Nhà Xuất Bản Tổng Hợp TP.HCM</Publisher>
    <PublicationYear>2020</PublicationYear>
    <Description>Cuốn sách về nghệ thuật giao tiếp và thu phục lòng người</Description>
    <CoverImageUrl>/images/dac-nhan-tam.jpg</CoverImageUrl>
    <Quantity>10</Quantity>
    <AvailableQuantity>8</AvailableQuantity>
    <Price>89000</Price>
    <Language>Tiếng Việt</Language>
    <PageCount>320</PageCount>
    <AverageRating>4.5</AverageRating>
    <RatingCount>25</RatingCount>
    <Categories>
      <CategoryDto>
        <Id>7</Id>
        <Name>Kinh Doanh</Name>
        <Description>Sách kinh doanh và kinh tế</Description>
        <Color>#54A0FF</Color>
      </CategoryDto>
    </Categories>
  </BookDto>
</ArrayOfBookDto>")
                    });
                }

                // CSV example
                if (!response.Content.ContainsKey("text/csv"))
                {
                    response.Content.Add("text/csv", new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema { Type = "string" },
                        Example = new OpenApiString("Id,Title,Author,ISBN,Publisher,PublicationYear,Description,CoverImageUrl,Quantity,AvailableQuantity,Price,Language,PageCount,AverageRating,RatingCount,Categories\n1,Đắc Nhân Tâm,Dale Carnegie,9786047770560,Nhà Xuất Bản Tổng Hợp TP.HCM,2020,Cuốn sách về nghệ thuật giao tiếp...,/images/dac-nhan-tam.jpg,10,8,89000,Tiếng Việt,320,4.5,25,Kinh Doanh")
                    });
                }
            }
        }
    }
}
