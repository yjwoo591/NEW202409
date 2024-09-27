using System.Threading.Tasks;
using ForexAITradingSystem.Models;

namespace ForexAITradingSystem.Managers
{
    public class ApiManager
    {
        private ApiConfig _currentConfig;

        public ApiConfig CurrentConfig => _currentConfig;

        public void UpdateConfig(ApiConfig newConfig)
        {
            _currentConfig = newConfig;
            // 여기에 설정을 저장하는 로직을 추가할 수 있습니다.
        }

        public async Task<MarketData> GetMarketDataAsync(string symbol)
        {
            // 실제 API 호출을 시뮬레이션
            await Task.Delay(100);
            return new MarketData { Symbol = symbol, CurrentPrice = 100.0m };
        }
    }
}