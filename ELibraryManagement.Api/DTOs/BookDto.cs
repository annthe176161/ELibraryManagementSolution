using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ELibraryManagement.Api.DTOs
{
    [DataContract(Name = "BookDto")]
    [XmlRoot("BookDto")]
    public class BookDto
    {
        [DataMember]
        [XmlElement("Id")]
        public int Id { get; set; }

        [DataMember]
        [XmlElement("Title")]
        [Required]
        public string Title { get; set; } = string.Empty;

        [DataMember]
        [XmlElement("Author")]
        [Required]
        public string Author { get; set; } = string.Empty;

        [DataMember]
        [XmlElement("ISBN")]
        public string? ISBN { get; set; }

        [DataMember]
        [XmlElement("Publisher")]
        public string? Publisher { get; set; }

        [DataMember]
        [XmlElement("PublicationYear")]
        public int PublicationYear { get; set; }

        [DataMember]
        [XmlElement("Description")]
        public string? Description { get; set; }

        [DataMember]
        [XmlElement("CoverImageUrl")]
        public string? CoverImageUrl { get; set; }

        [DataMember]
        [XmlElement("Quantity")]
        public int Quantity { get; set; }

        [DataMember]
        [XmlElement("AvailableQuantity")]
        public int AvailableQuantity { get; set; }

        [DataMember]
        [XmlElement("Price")]
        public decimal? Price { get; set; }

        [DataMember]
        [XmlElement("Language")]
        public string? Language { get; set; }

        [DataMember]
        [XmlElement("PageCount")]
        public int PageCount { get; set; }

        [DataMember]
        [XmlElement("AverageRating")]
        public float AverageRating { get; set; }

        [DataMember]
        [XmlElement("RatingCount")]
        public int RatingCount { get; set; }

        [DataMember]
        [XmlElement("Categories")]
        public List<CategoryDto>? Categories { get; set; }
    }

    [DataContract(Name = "CategoryDto")]
    [XmlRoot("CategoryDto")]
    public class CategoryDto
    {
        [DataMember]
        [XmlElement("Id")]
        public int Id { get; set; }

        [DataMember]
        [XmlElement("Name")]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        [XmlElement("Description")]
        public string? Description { get; set; }

        [DataMember]
        [XmlElement("Color")]
        public string? Color { get; set; }
    }
}
