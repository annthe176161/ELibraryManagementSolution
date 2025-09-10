﻿using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Api.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int BookId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        // Timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Book Book { get; set; } = null!;
    }
}
