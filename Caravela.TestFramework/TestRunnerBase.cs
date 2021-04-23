// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Caravela.TestFramework
{
    /// <summary>
    /// An abstract class for all template-base tests.
    /// </summary>
    public abstract class TestRunnerBase
    {
        public TestRunnerBase( string? projectDirectory )
        {
            this.ProjectDirectory = projectDirectory;
        }

        /// <summary>
        /// Gets the project directory, or <c>null</c> if it is unknown.
        /// </summary>
        public string? ProjectDirectory { get; }

        /// <summary>
        /// Runs a test.
        /// </summary>
        /// <param name="testInput"></param>
        /// <returns></returns>
        public virtual async Task<TestResult> RunTestAsync( TestInput testInput )
        {
            // Source.
            var project = this.CreateProject().WithParseOptions( CSharpParseOptions.Default.WithPreprocessorSymbols( "TESTRUNNER" ) );
            var testDocument = project.AddDocument( "Test.cs", SourceText.From( testInput.TestSource, Encoding.UTF8 ) );

            var initialCompilation = CSharpCompilation.Create(
                "test",
                new[] { (await testDocument.GetSyntaxTreeAsync())! },
                project.MetadataReferences,
                (CSharpCompilationOptions?) project.CompilationOptions );

            var result = new TestResult( project, testInput.TestName, testDocument, initialCompilation );

            if ( this.ReportInvalidInputCompilation )
            {
                var diagnostics = initialCompilation.GetDiagnostics();

                if ( diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
                {
                    result.SetFailed( "The initial compilation failed." );

                    return result;
                }
            }

            return result;
        }

        protected virtual bool ReportInvalidInputCompilation => true;

        /// <summary>
        /// Creates a new project that is used to compile the test source.
        /// </summary>
        /// <returns>A new project instance.</returns>
        public virtual Project CreateProject()
        {
            var referenceAssemblies = new ReferenceAssemblyLocator().SystemAssemblyPaths;

            var guid = Guid.NewGuid();
            var workspace1 = new AdhocWorkspace();
            var solution = workspace1.CurrentSolution;

            var project = solution.AddProject( guid.ToString(), guid.ToString(), LanguageNames.CSharp )
                .WithCompilationOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ) )
                .AddMetadataReferences( referenceAssemblies.Select( f => MetadataReference.CreateFromFile( f ) ) )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof(CompileTimeAttribute).Assembly.Location ) )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof(TemplateSyntaxFactory).Assembly.Location ) )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof(MetadataLoadContext).Assembly.Location ) )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof(TestOutputAttribute).Assembly.Location ) );

            // Don't add the assembly containing the code to test because it would result in duplicate symbols.

            return project;
        }
    }
}