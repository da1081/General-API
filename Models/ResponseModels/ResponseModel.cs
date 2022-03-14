namespace Models.ResponseModels
{
    public enum Status
    {
        UnknownError,
        Success,
        UsernameNotAvailableError,
        EmailNotAvailableError,
        InvalidDataSubmitedError,
        LockedOutError,
        UserNotFoundError,
        AlreadyConfirmedError,
        ConfirmationNotFoundError,
        PasswordResetError,
        InvalidIdError,
        InvalidRequest,
        PhoneConfirmationFailed,
        EmailNotConfirmed,
    }

    public class ResponseModel
    {
        public Status Status { get; set; } = Status.UnknownError;
        public string StatusStr { get => Status.ToString(); }
        public string Message { get; set; } = string.Empty;
    }
}
