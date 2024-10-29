using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace PC1databaseCreator.Tests.TestHelpers
{
    public class TestUtils
    {
        private readonly ITestOutputHelper _output;

        public TestUtils(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// 로그 출력이 가능한 ILogger 생성
        /// </summary>
        public ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory
                .Create(builder => builder.AddXUnit(_output))
                .CreateLogger<T>();
        }

        /// <summary>
        /// 파일 작업 시도 및 재시도
        /// </summary>
        public async Task<bool> TryWithRetryAsync(Func<Task> action, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await action();
                    return true;
                }
                catch (IOException) when (i < maxRetries - 1)
                {
                    await Task.Delay(100 * (i + 1));  // 점진적 대기
                    continue;
                }
            }
            return false;
        }

        /// <summary>
        /// 파일 내용 비교
        /// </summary>
        public async Task<bool> CompareFilesAsync(string path1, string path2)
        {
            if (new FileInfo(path1).Length != new FileInfo(path2).Length)
                return false;

            const int bufferSize = 4096;
            using var fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read);
            using var fs2 = new FileStream(path2, FileMode.Open, FileAccess.Read);
            var buffer1 = new byte[bufferSize];
            var buffer2 = new byte[bufferSize];

            while (true)
            {
                var count1 = await fs1.ReadAsync(buffer1, 0, bufferSize);
                var count2 = await fs2.ReadAsync(buffer2, 0, bufferSize);

                if (count1 != count2)
                    return false;

                if (count1 == 0)
                    return true;

                for (int i = 0; i < count1; i++)
                {
                    if (buffer1[i] != buffer2[i])
                        return false;
                }
            }
        }
    }
}
