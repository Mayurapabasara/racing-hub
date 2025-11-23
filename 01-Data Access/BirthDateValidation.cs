using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;

namespace RacingHubCarRental.Validations
{
    /// <summary>
    /// Validates if a given birth date matches an allowed age range.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class BirthDateValidationAttribute : ValidationAttribute, IClientValidatable
    {
        public int MinAge { get; }
        public int MaxAge { get; }

        public BirthDateValidationAttribute(int minAge, int maxAge)
            : base("{0} is invalid.")
        {
            MinAge = minAge;
            MaxAge = maxAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // If no value provided, no validation needed (use [Required] separately if needed)
            if (value == null)
                return ValidationResult.Success;

            if (value is not DateTime birthDate)
                throw new Exception("BirthDateValidation: value must be a DateTime.");

            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            // Adjust age if the birthday hasn't occurred yet this year
            if (birthDate.Date > today.AddYears(-age)) 
                age--;

            if (age < MinAge || age > MaxAge)
            {
                string msg = FormatErrorMessage(validationContext.DisplayName);
                return new ValidationResult(msg);
            }

            return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ValidationT

