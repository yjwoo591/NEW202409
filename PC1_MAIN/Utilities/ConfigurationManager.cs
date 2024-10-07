using System;
using System.IO;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ForexAITradingPC1Main.Utilities
{
    public static class ConfigurationManager
    {
        private static IConfiguration _configuration;
        private static string _configFilePath;

        public static void Initialize(string configFilePath)
        {
            _configFilePath = configFilePath;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFilePath, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        public static string GetValue(string key)
        {
            return _configuration[key];
        }

        public static T GetSection<T>(string sectionName) where T : class, new()
        {
            return _configuration.GetSection(sectionName).Get<T>();
        }

        public static void SetValue(string key, string value)
        {
            var json = File.ReadAllText(_configFilePath);
            var jObject = JObject.Parse(json);
            jObject[key] = value;
            string output = JsonConvert.SerializeObject(jObject, Formatting.Indented);
            File.WriteAllText(_configFilePath, output);

            // Reload the configuration
            Initialize(_configFilePath);
        }

        public static void SetSection<T>(string sectionName, T sectionData) where T : class
        {
            var json = File.ReadAllText(_configFilePath);
            var jObject = JObject.Parse(json);
            jObject[sectionName] = JToken.FromObject(sectionData);
            string output = JsonConvert.SerializeObject(jObject, Formatting.Indented);
            File.WriteAllText(_configFilePath, output);

            // Reload the configuration
            Initialize(_configFilePath);
        }

        public static bool TryGetValue(string key, out string value)
        {
            value = _configuration[key];
            return !string.IsNullOrEmpty(value);
        }

        public static bool TryGetSection<T>(string sectionName, out T section) where T : class, new()
        {
            section = _configuration.GetSection(sectionName).Get<T>();
            return section != null;
        }

        public static void ReloadConfiguration()
        {
            Initialize(_configFilePath);
        }

        public static string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name);
        }

        public static bool IsProduction()
        {
            return _configuration["Environment"] == "Production";
        }

        public static bool IsDevelopment()
        {
            return _configuration["Environment"] == "Development";
        }
    }
}
/*
 * 
 * 
 * 이 ConfigurationManager.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

JSON 설정 파일을 로드하고 관리하는 기능
환경을 포함한 설정 관리
설정 값 읽기 및 쓰기 기능
섹션별 설정 관리
설정 리로드 기능
연결 문자열 관리자
환경(개발/운영) 확인 기능

주요 방법:

초기화: 파일을 설정합니다.
GetValue: 특정 키에 대한 설정 값을 가져옵니다.
GetSection: 특정 섹션의 설정을 가져옵니다.
SetValue: 특정 키에 대한 설정 값을 설정합니다.
SetSection: 특정 섹션의 설정을 설정합니다.
TryGetValue: 특정 키에 대한 설정 값을 안전하게 가져옵니다.
TryGetSection: 특정 섹션의 설정을 지원하도록 가져옵니다.
ReloadConfiguration: 설정을 다시 로드합니다.
GetConnectionString: 특정 이름의 연결 문자열을 개체입니다.
IsProduction, IsDevelopment: 현재 환경이 운영인지 개발인지 확인합니다.

이 클래스는 다음과 같은 방식으로 설정을 관리합니다:

Microsoft.Extensions.Configuration을 사용하여 JSON 설정 파일을 로드합니다.
Newtonsoft.Json을 사용하여 JSON 파일을 처리하고 수정합니다.
파일 I/O 작업을 통해 파일을 직접 수정합니다.
설정이 변경될 때마다 설정을 리로드하여 최신 상태를 유지합니다.

이 유틸리티 클래스를 사용하여 모든 설정을 중앙에서 관리할 수 있습니다. 데이터베이스 연결 문자열, API 키, 환경 설정 등 다양한 설정을 관리하는 데 유용합니다.
추가 기능이나 수정이 필요하시면 말씀해 주세요.

*/