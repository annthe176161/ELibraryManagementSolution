using System;
using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Api.Validators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MinAgeAttribute : ValidationAttribute
    {
        private readonly int _minAge;

        public MinAgeAttribute(int minAge)
        {
            _minAge = minAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // If null, let [Required] handle it when necessary
                return ValidationResult.Success;
            }

            if (value is DateTime dob)
            {
                var today = DateTime.UtcNow.Date;
                var age = today.Year - dob.Date.Year;
                if (dob.Date > today.AddYears(-age)) age--;

                if (age < _minAge)
                {
                    var message = ErrorMessage ?? $"Người dùng phải lớn hơn hoặc bằng {_minAge} tuổi.";
                    return new ValidationResult(message);
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Giá trị ngày tháng không hợp lệ.");
        }
    }
}
