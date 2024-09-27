namespace ForexAITradingSystem.Models
{
    public class ApiConfig
    {
        public required string ApiKey { get; init; }
        public required string ApiSecret { get; init; }
        public required string Endpoint { get; init; }
    }
}