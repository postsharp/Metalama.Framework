// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// An abstract class for all template-base tests.
    /// </summary>
    public abstract class BaseTestRunner
    {
        private readonly Assembly[] _additionalAssemblies;

        public IServiceProvider ServiceProvider { get; }

        public BaseTestRunner( IServiceProvider serviceProvider, string? projectDirectory, IEnumerable<Assembly>? additionalAssemblies = null )
        {
            this._additionalAssemblies = (additionalAssemblies ?? Enumerable.Empty<Assembly>())
                .Append( typeof(BaseTestRunner).Assembly )
                .ToArray();

            this.ServiceProvider = serviceProvider;
            this.ProjectDirectory = projectDirectory;
        }

        /// <summary>
        /// Gets the project directory, or <c>null</c> if it is unknown.
        /// </summary>
        public string? ProjectDirectory { get; }

        protected virtual TestResult CreateTestResult() => new();

        /// <summary>
        /// Runs a test.
        /// </summary>
        /// <param name="testInput"></param>
        /// <returns></returns>
        public virtual TestResult RunTest( TestInput testInput )
        {
            // Source.
            var project = this.CreateProject().WithParseOptions( CSharpParseOptions.Default.WithPreprocessorSymbols( "TESTRUNNER" ) );
            var testDocument = project.AddDocument( "Test.cs", SourceText.From( testInput.SourceCode, Encoding.UTF8 ), filePath: "Test.cs" );
            var syntaxTree = testDocument.GetSyntaxTreeAsync().Result!;

            var initialCompilation = CSharpCompilation.Create(
                "test",
                new[] { syntaxTree },
                project.MetadataReferences,
                (CSharpCompilationOptions?) project.CompilationOptions );

            var testResult = this.CreateTestResult();
            testResult.Project = project;
            testResult.Input = testInput;
            testResult.TemplateDocument = testDocument;
            testResult.InitialCompilation = initialCompilation;

            if ( this.ReportInvalidInputCompilation )
            {
                var diagnostics = initialCompilation.GetDiagnostics();
                var errors = diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).ToArray();

                if ( errors.Any() )
                {
                    testResult.Report( errors );
                    testResult.SetFailed( "The initial compilation failed." );

                    return testResult;
                }
            }

            return testResult;
        }

        public static string? NormalizeString( string? s )
        {
            return s == null ? null : CSharpSyntaxTree.ParseText( s ).GetRoot().NormalizeWhitespace().ToString().Replace( "\r", "" );
        }

        public virtual void ExecuteAssertions( TestInput testInput, TestResult testResult, ITestOutputHelper logger )
        {
            if ( this.ProjectDirectory == null )
            {
                throw new InvalidOperationException( "This method cannot be called when the test path is unknown." );
            }
            
            // Compare the "Target" region of the transformed code to the expected output.
            // If the region is not found then compare the complete transformed code.
            var sourceAbsolutePath = testInput.FullPath;

            var expectedTransformedPath = Path.Combine(
                Path.GetDirectoryName( sourceAbsolutePath )!,
                Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + FileExtensions.TransformedCode );

            Assert.NotNull( testResult.TransformedTargetSourceText );

            var actualTransformedSourceText = NormalizeString( testResult.TransformedTargetSourceText!.ToString() );

            // If the expectation file does not exist, create it with some placeholder content.
            if ( !File.Exists( expectedTransformedPath ) )
            {
                File.WriteAllText(
                    expectedTransformedPath,
                    "// TODO: Replace this file with the correct transformed code. See the test output for the actual transformed code." );
            }

            // Read expectations from the file.
            var expectedNonNormalizedSourceText = File.ReadAllText( expectedTransformedPath );
            var expectedTransformedSourceText = NormalizeString( expectedNonNormalizedSourceText );

            // Update the file in obj/transformed if it is different.
            var actualTransformedPath = Path.Combine(
                this.ProjectDirectory,
                "obj",
                "transformed",
                Path.GetDirectoryName( testInput.RelativePath ) ?? "",
                Path.GetFileNameWithoutExtension( testInput.RelativePath ) + FileExtensions.TransformedCode );

            Directory.CreateDirectory( Path.GetDirectoryName( actualTransformedPath ) );

            var storedTransformedSourceText =
                File.Exists( actualTransformedPath ) ? NormalizeString( File.ReadAllText( actualTransformedPath ) ) : null;

            if ( expectedTransformedSourceText == actualTransformedSourceText
                 && storedTransformedSourceText != expectedNonNormalizedSourceText )
            {
                // Update the obj/transformed file to the non-normalized expected text, so that future call to update_transformed.txt
                // does not overwrite any whitespace change.
                File.WriteAllText( actualTransformedPath, expectedNonNormalizedSourceText );
            }
            else if ( storedTransformedSourceText == null || storedTransformedSourceText != actualTransformedSourceText )
            {
                File.WriteAllText( actualTransformedPath, actualTransformedSourceText );
            }

            logger.WriteLine( "=== TRANSFORMED CODE ===" );
            logger.WriteLine( actualTransformedSourceText );
            logger.WriteLine( "========================" );

            // Write all diagnostics to the logger.
            foreach ( var diagnostic in testResult.Diagnostics )
            {
                logger.WriteLine( diagnostic.ToString() );
            }

            Assert.Equal( expectedTransformedSourceText, actualTransformedSourceText );
        }

        protected virtual bool ReportInvalidInputCompilation => true;

        /// <summary>
        /// Creates a new project that is used to compile the test source.
        /// </summary>
        /// <returns>A new project instance.</returns>
        public virtual Project CreateProject()
        {
            var compilation = TestCompilationFactory.CreateEmptyCSharpCompilation( null, this._additionalAssemblies );

            var guid = Guid.NewGuid();
            var workspace1 = new AdhocWorkspace();
            var solution = workspace1.CurrentSolution;

            var project = solution.AddProject( guid.ToString(), guid.ToString(), LanguageNames.CSharp )
                .WithCompilationOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ) )
                .AddMetadataReferences( compilation.References );

            // Don't add the assembly containing the code to test because it would result in duplicate symbols.

            return project;
        }
    }
}