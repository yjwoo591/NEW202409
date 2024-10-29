
using System;
using System.IO;
using System.Text;
using PC1databaseCreator.Core.Storage.Base.Interfaces;

namespace PC1databaseCreator.Tests.TestHelpers
{
    public static class MockData
    {
        /// <summary>
        /// 테스트용 임시 파일 생성
        /// </summary>
        public static string CreateTempFile(string content, string extension = ".txt")
        {
            var filePath = Path.Combine(
                Path.GetTempPath(),
                $"test_{Guid.NewGuid()}{extension}");

            File.WriteAllText(filePath, content);
            return filePath;
        }

        /// <summary>
        /// 테스트용 이진 파일 생성
        /// </summary>
        public static string CreateTempBinaryFile(int sizeInBytes)
        {
            var filePath = Path.Combine(
                Path.GetTempPath(),
                $"test_{Guid.NewGuid()}.bin");

            var data = new byte[sizeInBytes];
            new Random().NextBytes(data);
            File.WriteAllBytes(filePath, data);
            return filePath;
        }

        /// <summary>
        /// 테스트용 임시 디렉토리 생성
        /// </summary>
        public static string CreateTempDirectory()
        {
            var dirPath = Path.Combine(
                Path.GetTempPath(),
                $"test_dir_{Guid.NewGuid()}");

            Directory.CreateDirectory(dirPath);
            return dirPath;
        }

        /// <summary>
        /// 테스트용 대용량 텍스트 파일 생성
        /// </summary>
        public static string CreateLargeTextFile(int sizeInMb)
        {
            var filePath = Path.Combine(
                Path.GetTempPath(),
                $"test_large_{Guid.NewGuid()}.txt");

            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            var random = new Random();
            var buffer = new char[1024];  // 1KB 버퍼

            for (int mb = 0; mb < sizeInMb; mb++)
            {
                for (int kb = 0; kb < 1024; kb++)
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = (char)('A' + random.Next(26));
                    }
                    writer.Write(buffer);
                }
            }

            return filePath;
        }
    }
}