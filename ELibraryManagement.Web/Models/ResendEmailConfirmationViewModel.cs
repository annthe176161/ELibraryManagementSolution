using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Web.Models
{
    public class ResendEmailConfirmationViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}