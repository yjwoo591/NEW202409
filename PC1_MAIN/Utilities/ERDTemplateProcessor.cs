using ForexAITradingPC1Main.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ForexAITradingPC1Main.Utils
{
    public class ERDTemplateProcessor
    {
        private readonly string _templatePath;

        public ERDTemplateProcessor(string templatePath)
        {
            _templatePath = templatePath;
        }

        public async Task<string> ReadTemplateAsync()
        {
            try
            {
                return await File.ReadAllTextAsync(_templatePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading ERD template: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<bool> UpdateTemplateAsync(string content)
        {
            try
            {
                await File.WriteAllTextAsync(_templatePath, content);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating ERD template: {ex.Message}");
                return false;
            }
        }

        public string ProcessTemplate(string template, Dictionary<string, string> replacements)
        {
            foreach (var replacement in replacements)
            {
                template = template.Replace($"{{{replacement.Key}}}", replacement.Value);
            }
            return template;
        }

        public async Task<string> GenerateERDAsync(Dictionary<string, string> entityDefinitions)
        {
            var template = await ReadTemplateAsync();
            var erd = "erDiagram\n";

            foreach (var entity in entityDefinitions)
            {
                erd += $"    {entity.Key} {{\n{entity.Value}\n    }}\n";
            }

            return ProcessTemplate(template, new Dictionary<string, string> { { "ENTITIES", erd } });
        }
    }
}

/*
ERDTemplate.mmd 파일은 Mermaid 형식의 ERD(Entity-Relationship Diagram) 폴더를 포함하고 있습니다. 이 파일을 처리하고 활용하기 위해 다음과 동일한 방법을 제안합니다:

파일 읽기: 먼저, 이 파일의 내용을 입력하는 방법을 요청해 주세요.

유틸리티/ERDTemplateProcessor.cs코드를 열려면 클릭하세요
이 ERDTemplateProcessor 클래스는 다음과 같은 기능을 제공합니다:

ReadTemplateAsync(): ERD 폴더를 적절하게 읽습니다.
UpdateTemplateAsync(): ERD 폴더를 적절하게 업데이트합니다.
ProcessTemplate(): 폴더의 플레이스홀더를 실제 값으로 대체합니다.
generateERDAsync(): 분리 정의를 사용하여 ERD를 생성합니다.

이 클래스를 사용하여 ERD 폴더를 관리하고 동적으로 ERD를 생성할 수 있습니다. 예를 들어, 데이터베이스가 변경될 때마다 이 클래스를 사용하여 ERD를 자동으로 업데이트할 수 있습니다.
사용 예시:
씨샤프복사var processor = new ERDTemplateProcessor("path/to/ERDTemplate.mmd");

var entityDefinitions = new Dictionary<string, string>
{
    { "Customer", "int id\nstring name\nstring email" },
    { "Account", "int id\nint customerId\ndecimal balance" }
};

var erd = await processor.GenerateERDAsync(entityDefinitions);
Console.WriteLine(erd);
이 방식으로 ERDTemplate.mmd 파일을 활용할 수 있습니다. 필요에 따라 ERD를 적극적으로 생성하거나 추가할 수 있으므로 이를 통해 데이터베이스 구조를 표시하여 표현하고 관리할 수 있습니다.
추가 기능이나 수정이 필요하시면 말씀해 주세요.복사다시 해 보다클로드는 아직 생성된 코드를 실행할 수 있는 능력이 없습니다.

*/