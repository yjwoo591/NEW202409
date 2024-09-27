using System.IO;
using Newtonsoft.Json;

namespace ForexAITradingSystem.Utilities
{
    public static class ConfigManager
    {
        private const string ConfigFileName = "ForexAITradingSystemConfig.json";

        public static void SaveConfig(Config config)
        {
            string json = JsonConvert.SerializeObject(config);
            string encrypted = Encryptor.Encrypt(json);
            File.WriteAllText(ConfigFileName, encrypted);
        }

        public static Config LoadConfig()
        {
            if (File.Exists(ConfigFileName))
            {
                string encrypted = File.ReadAllText(ConfigFileName);
                string json = Encryptor.Decrypt(encrypted);
                return JsonConvert.DeserializeObject<Config>(json);
            }
            return new Config();
        }
    }

    public class Config
    {
        public string DbServerIp { get; set; }
        public string DbUserId { get; set; }
        public string DbPassword { get; set; }
        // 기타 설정 정보...
    }
}