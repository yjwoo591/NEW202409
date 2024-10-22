```csharp
using System;
using System.IO;
using PC1MAINAITradingSystem.Interfaces;

namespace PC1MAINAITradingSystem.Core.ERDProcessor
{
    public class ERDReader : IERDReader
    {
        private readonly ILogger _logger;

        public ERDReader(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string ReadERDFile(string filePath)
        {
            try
            {
                _logger.Log($"Reading ERD file: {filePath}");

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"ERD file not found: {filePath}");
                }

                using (var streamReader = new StreamReader(filePath))
                {
                    var content = streamReader.ReadToEnd();
                    _logger.Log($"Successfully read ERD file: {filePath}");
                    return content;
                }
            }
            catch (Exception ex) when (ex is not FileNotFoundException)
            {
                _logger.LogError($"Error reading ERD file: {ex.Message}");
                throw new ERDReadException($"Failed to read ERD file: {filePath}", ex);
            }
        }

        public bool SaveERDFile(string filePath, string content)
        {
            try
            {
                _logger.Log($"Saving ERD file: {filePath}");

                using (var streamWriter = new StreamWriter(filePath, false))
                {
                    streamWriter.Write(content);
                }

                _logger.Log($"Successfully saved ERD file: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving ERD file: {ex.Message}");
                throw new ERDWriteException($"Failed to save ERD file: {filePath}", ex);
            }
        }

        public bool BackupERDFile(string originalFilePath)
        {
            try
            {
                _logger.Log($"Creating backup of ERD file: {originalFilePath}");

                string backupPath = GenerateBackupPath(originalFilePath);
                File.Copy(originalFilePath, backupPath, true);

                _logger.Log($"Successfully created ERD backup: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error backing up ERD file: {ex.Message}");
                throw new ERDBackupException($"Failed to backup ERD file: {originalFilePath}", ex);
            }
        }

        private string GenerateBackupPath(string originalFilePath)
        {
            string directory = Path.GetDirectoryName(originalFilePath);
            string fileName = Path.GetFileNameWithoutExtension(originalFilePath);
            string extension = Path.GetExtension(originalFilePath);
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            return Path.Combine(directory, $"{fileName}_{timestamp}_backup{extension}");
        }
    }
}
```