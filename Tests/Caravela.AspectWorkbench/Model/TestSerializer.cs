using System.IO;
using System.Threading.Tasks;
using Caravela.TestFramework;

namespace Caravela.AspectWorkbench.Model
{
    internal class TestSerializer
    {
        private static string GetExpectedOutputFilePath( string testFilePath ) => Path.ChangeExtension( testFilePath, ".transformed.txt" );

        public async Task<TemplateTest> LoadFromFileAsync( string filePath )
        {
            var testName = Path.GetFileNameWithoutExtension( filePath );
            var testSource = await File.ReadAllTextAsync( filePath );

            var expectedOutputFilePath = GetExpectedOutputFilePath( filePath );
            string? expectedOutput = null;

            if ( File.Exists( expectedOutputFilePath ) )
            {
                expectedOutput = File.ReadAllText( expectedOutputFilePath );
            }

            return new TemplateTest
            {
                Input = new TestInput( testName, null, testSource, null ),
                ExpectedOutput = expectedOutput
            };
        }

        public async Task SaveToFileAsync( TemplateTest test, string filePath )
        {
            await File.WriteAllTextAsync( filePath, test.Input.TestSource.ToString() );

            var expectedOutputFilePath = GetExpectedOutputFilePath( filePath );
            await File.WriteAllTextAsync( expectedOutputFilePath, test.ExpectedOutput );
        }
    }
}
