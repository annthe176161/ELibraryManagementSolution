using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ELibraryManagement.Api.DTOs
{
    [DataContract(Name = "CreateBookDto")]
    [XmlRoot("CreateBookDto")]
    public class CreateBookDto
    {
        [DataMember]
        [XmlElement("Title")]
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string Title { get; set; } = string.Empty;

        [DataMember]
        [XmlElement("Author")]
        [Required(ErrorMessage = "Author is required")]
        [StringLength(100, ErrorMessage = "Author cannot be longer than 100 characters")]
        public string Author { get; set; } = string.Empty;

        [DataMember]
        [XmlElement("ISBN")]
        [StringLength(20, ErrorMessage = "ISBN cannot be longer than 20 characters")]
        public string? ISBN { get; set; }

        [DataMember]
        [XmlElement("Publisher")]
        [StringLength(100, ErrorMessage = "Publisher cannot be longer than 100 characters")]
        public string? Publisher { get; set; }

        [DataMember]
        [XmlElement("PublicationYear")]
        [Range(1000, 2100, ErrorMessage = "Publication year must be between 1000 and 2100")]
        public int PublicationYear { get; set; }

        [DataMember]
        [XmlElement("Description")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string? Description { get; set; }

        [DataMember]
        [XmlElement("CoverImageUrl")]
        [Url(ErrorMessage = "Cover image URL must be a valid URL")]
        public string? CoverImageUrl { get; set; }

        [DataMember]
        [XmlElement("Quantity")]
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number")]
        public int Quantity { get; set; }

        [DataMember]
        [XmlElement("Language")]
        [StringLength(50, ErrorMessage = "Language cannot be longer than 50 characters")]
        public string? Language { get; set; }

        [DataMember]
        [XmlElement("PageCount")]
        [Range(0, int.MaxValue, ErrorMessage = "Page count must be a non-negative number")]
        public int PageCount { get; set; }

        [DataMember]
        [XmlElement("CategoryIds")]
        public List<int>? CategoryIds { get; set; }
    }

    [DataContract(Name = "UpdateBookDto")]
    [XmlRoot("UpdateBookDto")]
    public class UpdateBookDto
    {
        [DataMember]
        [XmlElement("Id")]
        [Required(ErrorMessage = "Id is required")]
        public int Id { get; set; }

        [DataMember]
        [XmlElement("Title")]
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string Title { get; set; } = string.Empty;

        [DataMember]
        [XmlElement("Author")]
        [Required(ErrorMessage = "Author is required")]
        [StringLength(100, ErrorMessage = "Author cannot be longer than 100 characters")]
        public string Author { get; set; } = string.Empty;

        [DataMember]
        [XmlElement("ISBN")]
        [StringLength(20, ErrorMessage = "ISBN cannot be longer than 20 characters")]
        public string? ISBN { get; set; }

        [DataMember]
        [XmlElement("Publisher")]
        [StringLength(100, ErrorMessage = "Publisher cannot be longer than 100 characters")]
        public string? Publisher { get; set; }

        [DataMember]
        [XmlElement("PublicationYear")]
        [Range(1000, 2100, ErrorMessage = "Publication year must be between 1000 and 2100")]
        public int PublicationYear { get; set; }

        [DataMember]
        [XmlElement("Description")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string? Description { get; set; }

        [DataMember]
        [XmlElement("CoverImageUrl")]
        [Url(ErrorMessage = "Cover image URL must be a valid URL")]
        public string? CoverImageUrl { get; set; }

        [DataMember]
        [XmlElement("Quantity")]
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number")]
        public int Quantity { get; set; }

        [DataMember]
        [XmlElement("Language")]
        [StringLength(50, ErrorMessage = "Language cannot be longer than 50 characters")]
        public string? Language { get; set; }

        [DataMember]
        [XmlElement("PageCount")]
        [Range(0, int.MaxValue, ErrorMessage = "Page count must be a non-negative number")]
        public int PageCount { get; set; }

        [DataMember]
        [XmlElement("CategoryIds")]
        public List<int>? CategoryIds { get; set; }
    }
}