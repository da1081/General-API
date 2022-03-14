namespace Models.ResponseModels
{
    public class TokenResponseModel
    {
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset Expiration { get; set; } = DateTimeOffset.MinValue;
    }
}
