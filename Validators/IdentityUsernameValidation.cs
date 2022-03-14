using System.ComponentModel.DataAnnotations;

namespace Validators
{
    /// <summary>
    /// Validate attribute for identity username.
    /// </summary>
    public class IdentityUsernameValidation : ValidationAttribute
    {
        private readonly List<string> errorMessageList = new();

        public char[] AllowedChars { get; }
        public string AllowedCharsError { get; }
        public int RequiredLength { get; }
        public string RequiredLengthError { get; }

        /// <summary>
        /// Validate attribute for identity username. Defaults to identity defaults.
        /// </summary>
        /// <param name="allowedChars"></param>
        /// <param name="allowedCharsError"></param>
        /// <param name="requiredLength"></param>
        /// <param name="requiredLengthError"></param>
        public IdentityUsernameValidation(
            char[]? allowedChars = null,
            string allowedCharsError = "Unallowed character used. Allowed characters is a-z, A-Z, 0-9 and -._@+!?",
            int requiredLength = 3,
            string requiredLengthError = "Username not long enough")
        {
            AllowedChars = allowedChars ?? "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+!?".ToCharArray();
            AllowedCharsError = allowedCharsError;
            RequiredLength = requiredLength;
            RequiredLengthError = requiredLengthError;
        }

        /// <summary>
        /// Creates the error message.
        /// </summary>
        /// <returns>Error details string.</returns>
        public string GetErrorMessage()
        {
            if (errorMessageList.Count == 0)
                return string.Empty;
            if (errorMessageList.Count == 1)
                return errorMessageList.First();
            return string.Join(". ", errorMessageList);
        }

        /// <summary>
        /// The validation method. Used to validate and test validation.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>List of error messages.</returns>
        public List<string> Validate(string value)
        {
            if (value.Length < RequiredLength)
                errorMessageList.Add(RequiredLengthError);

            if (!ValidationUtilities.StringContainsOnlyValidChars(value, AllowedChars))
                errorMessageList.Add(AllowedCharsError);

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
