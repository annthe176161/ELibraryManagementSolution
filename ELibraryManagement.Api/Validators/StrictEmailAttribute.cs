using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace ELibraryManagement.Api.Validators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class StrictEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var email = value as string;
            if (string.IsNullOrWhiteSpace(email)) return ValidationResult.Success;

            // Basic parse using MailAddress
            try
            {
                var addr = new MailAddress(email);
                // MailAddress allows some weird values; perform additional domain checks
                var host = addr.Host; // part after @
                if (string.IsNullOrWhiteSpace(host))
                {
                    return new ValidationResult(ErrorMessage ?? "Địa chỉ email không hợp lệ.");
                }

                // domain must contain at least one dot and no empty labels
                if (!host.Contains('.') || Array.Exists(host.Split('.'), string.IsNullOrEmpty))
                {
                    return new ValidationResult(ErrorMessage ?? "Địa chỉ email không hợp lệ.");
                }

                // domain cannot start or end with a dot
                if (host.StartsWith('.') || host.EndsWith('.'))
                {
                    return new ValidationResult(ErrorMessage ?? "Địa chỉ email không hợp lệ.");
                }

                return ValidationResult.Success;
            }
            catch
            {
                return new ValidationResult(ErrorMessage ?? "Địa chỉ email không hợp lệ.");
            }
        }
    }
}
