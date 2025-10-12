using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ELibraryManagement.Api.Validators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DisposableEmailAttribute : ValidationAttribute
    {
        // A small blacklist of common disposable email domains. Extend as needed.
        private static readonly string[] DisposableDomains = new[]
        {
            "mailinator.com",
            "10minutemail.com",
            "yopmail.com",
            "temp-mail.org",
            "guerrillamail.com",
            "trashmail.com",
            "maildrop.cc",
            "disposablemail.com",
            "tempmail.com"
        };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var email = value as string;
            if (string.IsNullOrWhiteSpace(email)) return ValidationResult.Success;

            var atIndex = email.LastIndexOf('@');
            if (atIndex < 0 || atIndex == email.Length - 1)
            {
                // Let [EmailAddress] handle format validation if present
                return ValidationResult.Success;
            }

            var domain = email.Substring(atIndex + 1).ToLowerInvariant();

            // check exact matches and subdomains
            if (DisposableDomains.Any(d => domain == d || domain.EndsWith("." + d)))
            {
                var message = ErrorMessage ?? "Không được sử dụng email tạm thời. Vui lòng sử dụng email thật.";
                return new ValidationResult(message);
            }

            return ValidationResult.Success;
        }
    }
}
