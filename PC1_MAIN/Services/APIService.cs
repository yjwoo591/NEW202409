using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ForexAITradingPC1Main.Models;
using Microsoft.Extensions.Configuration;

namespace ForexAITradingPC1Main.Services
{
    public class APIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public APIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _baseUrl = _configuration["ExternalAPI:BaseUrl"];
            _apiKey = _configuration["ExternalAPI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<HogaData> GetLatestHogaDataAsync(int seriesId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/hoga/{seriesId}/latest");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<HogaData>(content);
        }

        public async Task<DealData> GetLatestDealDataAsync(int seriesId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/deal/{seriesId}/latest");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DealData>(content);
        }

        public async Task<bool> PlaceOrderAsync(Order order)
        {
            var json = JsonConvert.SerializeObject(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/order", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/order/{orderId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Account> GetAccountInfoAsync(string accountNumber)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/account/{accountNumber}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Account>(content);
        }

        public async Task<Series> GetSeriesInfoAsync(int seriesId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/series/{seriesId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Series>(content);
        }

        public async Task<bool> UpdateRiskManagementSettingsAsync(RiskManagement settings)
        {
            var json = JsonConvert.SerializeObject(settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/risk-management", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<string> GetMarketStatusAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/market/status");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<bool> SendTradingSignalAsync(TradingSignal signal)
        {
            var json = JsonConvert.SerializeObject(signal);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/trading-signal", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<string> GetSystemHealthAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/system/health");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    public class Order
    {
        public int SeriesId { get; set; }
        public string AccountNumber { get; set; }
        public string OrderType { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class TradingSignal
    {
        public int SeriesId { get; set; }
        public string SignalType { get; set; }
        public decimal TargetPrice { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
/*
이 APIService.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

외부 API 통신을 통해 HttpClient를 사용합니다.
설정 파일에서 API 기본 URL과 API 키를 읽어오는 기능이 있습니다.
다양한 API 포인트에 대한 요청 메서드 구현
JSON 직렬화 및 역직렬화를 사용합니다. Newtonsoft.Json 라이브러리를 사용합니다.

주요 방법:

GetLatestHogaDataAsync: 최신 호가 데이터를 제출합니다.
GetLatestDealDataAsync: 최신 거래 데이터를 제출합니다.
PlaceOrderAsync: 주문을 발주합니다.
CancelOrderAsync: 주문을 취소합니다.
GetAccountInfoAsync: 당연한 정보를 가지고 있습니다.
GetSeriesInfoAsync: 시리즈 정보를 가져옵니다.
UpdateRiskManagementSettingsAsync: 보안 관리 설정을 업데이트합니다.
GetMarketStatusAsync: 시장 상태를 공모합니다.
SendTradingSignalAsync: 거래 신호를 전송합니다.
GetSystemHealthAsync: 시스템 상태를 확인합니다.

이 서비스는 다음과 동일한 방식으로 API 통신을 수행합니다.

HttpClient를 사용하여 HTTP 요청을 보냅니다.
API 인증을 위해 헤더에 API 키를 추가합니다.
JSON 형식으로 데이터를 전송합니다.
응답 상태 코드를 확인하여 요청의 성공 여부를 설명합니다.

또한 Order와 TradingSignal 클래스를 정의하여 주문 및 거래 신호 데이터를 관리합니다.
이 서비스를 사용하여 외부 시스템과 통신하고, 측정 데이터를 가져오거나 선물을 드릴 수 있습니다. 시스템의 다른 부분에서 이 서비스를 수락할 수 있도록 설계해 주시기 바랍니다.
추가 기능이나 수정이 필요하시면 말씀해 주세요.

*/