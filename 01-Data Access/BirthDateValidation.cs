using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;

namespace RacingHubCarRental.Validations
{
    /// <summary>
    /// Validates if a given birth date matches an allowed age range.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BirthDateValidationAttribute : ValidationAttribute, IClientValidatable
    {
        public int MinAge { get; }
        public int MaxAge { get; }

        public BirthDateValidationAttribute(int minAge, int maxAge)
            : base("The {0} is not within the valid age range.")
        {
            MinAge = minAge;
            MaxAge = maxAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success; // Use [Required] for required validation

            if (value is not DateTime birthDate)
                throw new Exception("BirthDateValidation: value must be a DateTime.");

            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            // Adjust if birthday has not occurred this year
            if (birthDate.Date > today.AddYears(-age))
                age--;

            if (age < MinAge || age > MaxAge)
            {
                string msg = FormatErrorMessage(validationContext.DisplayName);
                return new ValidationResult(msg);
            }

            return ValidationResult.Success;
        }

        // --------------------------
        // Client-Side Validation Rule
        // --------------------------
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ValidationType = "birthdatevalidation", // → lowercase, no spaces
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName())
            };

            // Pass MinAge & MaxAge to the client (JavaScript)
            rule.ValidationParameters.Add("minage", MinAge);
            rule.ValidationParameters.Add("maxage", MaxAge);

            yield return rule;
        }
    }
}

