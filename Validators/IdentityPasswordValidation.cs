using System.ComponentModel.DataAnnotations;

namespace Validators
{
    /// <summary>
    /// Validation attribute for identity passwords.
    /// </summary>
    public class IdentityPasswordValidation : ValidationAttribute
    {
        private readonly List<string> errorMessageList = new();

        public int RequiredLength { get; }
        public string RequiredLengthError { get; }
        public int RequiredUniqueChars { get; }
        public string RequiredUniqueCharsError { get; }
        public char[]? UniqueChars { get; }
        public string UniqueCharsError { get; }
        public bool RequireDigit { get; }
        public string RequireDigitError { get; }
        public bool RequireLowercase { get; }
        public string RequireLowercaseError { get; }
        public bool RequireNonAlphanumeric { get; }
        public string RequireNonAlphanumericError { get; }
        public bool RequireUppercase { get; }
        public string RequireUppercaseError { get; }

        /// <summary>
        /// Validation attribute for identity password. Defaults to the identity default settings.
        /// </summary>
        /// <param name="requiredLength"></param>
        /// <param name="requiredUniqueChars"></param>
        /// <param name="uniqueChars"></param>
        /// <param name="requireDigit"></param>
        /// <param name="requireLowercase"></param>
        /// <param name="requireNonAlphanumeric"></param>
        /// <param name="requireUppercase"></param>
        public IdentityPasswordValidation(
            int requiredLength = 6,
            string requiredLengthError = "A minimum of {0} characters is required",
            int requiredUniqueChars = 1,
            string requiredUniqueCharsError = "Atleast {0} unique (NonAlphanumeric) character is required",
            char[]? uniqueChars = null,
            string uniqueCharsError = "",
            bool requireDigit = true,
            string requireDigitError = "Password has to contain digits",
            bool requireNonAlphanumeric = true,
            string requireNonAlphanumericError = "",
            bool requireLowercase = true,
            string requireLowercaseError = "Password has to contain lowercase characters",
            bool requireUppercase = true,
            string requireUppercaseError = "Password has to contain uppercase characters")
        {
            RequiredLength = requiredLength;
            RequiredLengthError = requiredLengthError;
            RequiredUniqueChars = requiredUniqueChars;
            RequiredUniqueCharsError = requiredUniqueCharsError;


            if (requiredUniqueChars > 0)
            {
                UniqueChars = uniqueChars ?? new char[]
                {
                    '!', '"', '@', '#', '£', '¤', '$', '%', '€', '&', '/', '{', '(', '[', ']', ')', '}', '=', '*',
                    '?', '+', '-', '_', '.', ':', ',', ';', '<', '>', '|', '^', '¨', '~', '\'', '\\', '´', '`'
                };
            }
            UniqueCharsError = uniqueCharsError;

            RequireDigit = requireDigit;
            RequireDigitError = requireDigitError;
            RequireNonAlphanumeric = requireNonAlphanumeric;
            RequireNonAlphanumericError = requireNonAlphanumericError;
            RequireLowercase = requireLowercase;
            RequireLowercaseError = requireLowercaseError;
            RequireUppercase = requireUppercase;
            RequireUppercaseError = requireUppercaseError;
        }

        /// <summary>
        /// Creates the error message.
        /// </summary>
        /// <param name="errorResult"></param>
        /// <returns></returns>
        public string GetErrorMessage()
        {
            if (errorMessageList.Count == 0)
                return string.Empty;
            if (errorMessageList.Count == 1)
                return errorMessageList.First() + ".";
            return string.Join(". ", errorMessageList); ;
        }

        /// <summary>
        /// The validation metod. Used to validate and test validation.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public List<string> Validate(string value)
        {
            // Check length requirement.
            if (value.Length < RequiredLength)
                errorMessageList.Add(string.Format(RequiredLengthError, RequiredLength));

            // Check RequiredUniqueChar requirement.
            if (RequiredUniqueChars > 0 && (!ValidationUtilities.MinimumCharOccurrence(value, UniqueChars!, RequiredUniqueChars)))
                errorMessageList.Add(string.Format(RequiredUniqueCharsError, RequiredUniqueChars));

            // Check contains digit requirement.
            if (RequireDigit && (!ValidationUtilities.ContainsDigits(value)))
                errorMessageList.Add(RequireDigitError);

            // Check contains lowercase requirement.
            if (RequireLowercase && (!ValidationUtilities.ContainsLowercase(value)))
                errorMessageList.Add(RequireLowercaseError);

            // Check contains uppercase requirement.
            if (RequireUppercase && (!ValidationUtilities.ContainsUppercase(value)))
                errorMessageList.Add(RequireUppercaseError);

            return errorMessageList;
        }

        /// <summary>
        /// Returns the validation result.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            errorMessageList.Clear();
            _ = Validate((string)value!);
            if (errorMessageList.Any())
                return new ValidationResult(GetErrorMessage());
            return ValidationResult.Success;
        }
    }
}
