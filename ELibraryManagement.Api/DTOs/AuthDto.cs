using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ELibraryManagement.Api.DTOs
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [ELibraryManagement.Api.Validators.StrictEmail(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [ELibraryManagement.Api.Validators.DisposableEmail(ErrorMessage = "Không được sử dụng email tạm thời. Vui lòng sử dụng email thật.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [JsonIgnore]
        public string? UserName { get; set; }

        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [MaxLength(10, ErrorMessage = "Mã sinh viên không được quá 10 ký tự.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Mã sinh viên chỉ được chứa chữ cái và số, không được có khoảng trắng hoặc ký tự đặc biệt.")]
        public string? StudentId { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có đúng 10 chữ số.")]
        public string? PhoneNumber { get; set; }

        [ELibraryManagement.Api.Validators.MinAge(18, ErrorMessage = "Người dùng phải lớn hơn hoặc bằng 18 tuổi.")]
        public DateTime? DateOfBirth { get; set; }
    }

    public class LoginRequestDto
    {
        [Required]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public DateTime? Expiration { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? StudentId { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        [ELibraryManagement.Api.Validators.MinAge(18, ErrorMessage = "Người dùng phải lớn hơn hoặc bằng 18 tuổi.")]
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }
        public int TotalBorrows { get; set; }
        public int ActiveBorrows { get; set; }
    }

    public class UpdateProfileRequestDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(10, ErrorMessage = "Mã sinh viên không được quá 10 ký tự.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Mã sinh viên chỉ được chứa chữ cái và số, không được có khoảng trắng hoặc ký tự đặc biệt.")]
        public string? StudentId { get; set; }

        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có đúng 10 chữ số.")]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(500)]
        [Url]
        public string? AvatarUrl { get; set; }
    }

    public class ChangePasswordRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [ELibraryManagement.Api.Validators.StrictEmail(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [ELibraryManagement.Api.Validators.DisposableEmail(ErrorMessage = "Không được sử dụng email tạm thời. Vui lòng sử dụng email thật.")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [ELibraryManagement.Api.Validators.StrictEmail(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [ELibraryManagement.Api.Validators.DisposableEmail(ErrorMessage = "Không được sử dụng email tạm thời. Vui lòng sử dụng email thật.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
