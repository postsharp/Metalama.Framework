// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Caravela.AspectWorkbench.Model
{
    internal static class TestSerializer
    {
        private static string GetExpectedTransformedCodeFilePath( string testFilePath ) => Path.ChangeExtension( testFilePath, FileExtensions.TransformedCode );

        private static string GetExpectedProgramOutputFilePath( string testFilePath ) => Path.ChangeExtension( testFilePath, FileExtensions.ProgramOutput );

        public static async Task<TemplateTest> LoadFromFileAsync( string filePath )
        {
            var testSource = await File.ReadAllTextAsync( filePath );

            var expectedTransformedCodeFilePath = GetExpectedTransformedCodeFilePath( filePath );
            string? expectedTransformedCode = null;

            if ( File.Exists( expectedTransformedCodeFilePath ) )
            {
                expectedTransformedCode = await File.ReadAllTextAsync( expectedTransformedCodeFilePath );
            }

            var expectedProgramOutputFilePath = GetExpectedProgramOutputFilePath( filePath );
            string? expectedProgramOutput = null;

            if ( File.Exists( expectedProgramOutputFilePath ) )
            {
                expectedProgramOutput = await File.ReadAllTextAsync( expectedProgramOutputFilePath );
            }

            return new TemplateTest
            {
                Input = TestInput.FromSource( testSource, filePath ),
                ExpectedTransformedCode = expectedTransformedCode,
                ExpectedProgramOutput = expectedProgramOutput
            };
        }

        public static async Task SaveToFileAsync( TemplateTest test, string filePath )
        {
            if ( test.Input == null )
            {
                throw new InvalidOperationException( "Test input not set." );
            }

            await File.WriteAllTextAsync( filePath, test.Input.SourceCode );

            var expectedTransformedCodeFilePath = GetExpectedTransformedCodeFilePath( filePath );
            await File.WriteAllTextAsync( expectedTransformedCodeFilePath, test.ExpectedTransformedCode );

            if ( !string.IsNullOrWhiteSpace( test.ExpectedTransformedCode ) )
            {
                var expectedProgramOutputFilePath = GetExpectedProgramOutputFilePath( filePath );
                await File.WriteAllTextAsync( expectedProgramOutputFilePath, test.ExpectedProgramOutput );
            }
        }
    }
}