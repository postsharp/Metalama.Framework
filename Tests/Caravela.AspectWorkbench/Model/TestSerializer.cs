// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Caravela.AspectWorkbench.Model
{
    internal class TestSerializer
    {
        private static string GetExpectedOutputFilePath( string testFilePath ) => Path.ChangeExtension( testFilePath, ".transformed.txt" );

        public static async Task<TemplateTest> LoadFromFileAsync( string filePath )
        {
            var testName = Path.GetFileNameWithoutExtension( filePath );
            var testSource = await File.ReadAllTextAsync( filePath );

            var expectedOutputFilePath = GetExpectedOutputFilePath( filePath );
            string? expectedOutput = null;

            if ( File.Exists( expectedOutputFilePath ) )
            {
                expectedOutput = File.ReadAllText( expectedOutputFilePath );
            }

            return new TemplateTest { Input = new TestInput( testName, testSource ), ExpectedOutput = expectedOutput };
        }

        public static async Task SaveToFileAsync( TemplateTest test, string filePath )
        {
            if ( test.Input == null )
            {
                throw new InvalidOperationException( "Test input not set." );
            }

            await File.WriteAllTextAsync( filePath, test.Input.TestSource );

            var expectedOutputFilePath = GetExpectedOutputFilePath( filePath );
            await File.WriteAllTextAsync( expectedOutputFilePath, test.ExpectedOutput );
        }
    }
}