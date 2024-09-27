namespace ForexAITradingSystem.Models
{
    public class Order
    {
        public string Symbol { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public bool IsBuyOrder { get; set; }
        // 필요한 다른 속성들...
    }

    public enum OrderType
    {
        Buy,
        Sell
    }
}