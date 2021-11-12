// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.TestFramework.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal class DesignTimeFallbackTestRunner : BaseTestRunner
    {
        public DesignTimeFallbackTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger ) { }

        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            Dictionary<string, object?> state )
        {
            await base.RunAsync( testInput, testResult, state );

            using var domain = new UnloadableCompileTimeDomain();

            var projectOptions = testResult.ProjectScopedServiceProvider.GetService<IProjectOptions>();

            // Replace the project options to enable design time fallback.
            var designTimeFallbackServiceProvider = testResult.ProjectScopedServiceProvider.WithService( new DesignTimeFallbackProjectOptions( projectOptions ) );
            using var compileTimePipeline = new CompileTimeAspectPipeline( designTimeFallbackServiceProvider, true, domain, Framework.Aspects.AspectExecutionScenario.CompileTime );

            var inputCompilation = testResult.InputCompilation!;

            // Create a compilation from the input compilation while removing nodes marked for removal.
            foreach ( var inputSyntaxTree in inputCompilation!.SyntaxTrees )
            {
                inputCompilation = inputCompilation.ReplaceSyntaxTree(
                    inputSyntaxTree,
                    inputSyntaxTree.WithRootAndOptions( RemovingRewriter.Instance.Visit( inputSyntaxTree.GetRoot() ), inputSyntaxTree.Options ) );
            }

            var compileTimeResult = await compileTimePipeline.ExecuteAsync( testResult.PipelineDiagnostics, inputCompilation, default, CancellationToken.None );

            if ( compileTimeResult == null )
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute failed" );

                return;
            }

            // Create a compilation from the input compilation with removed nodes plus auxiliary files.
            var resultingCompilation = inputCompilation;

            var newSyntaxTrees = new List<SyntaxTree>();

            foreach ( var file in compileTimeResult.AuxiliaryFiles.Where( f => f.Kind == AuxiliaryFileKind.DesignTimeFallback && Path.GetExtension(f.Path) == ".cs") )
            {
                var syntaxTree = CSharpSyntaxTree.ParseText( SourceText.From( file.Content, file.Content.Length ) );
                newSyntaxTrees.Add( syntaxTree );
                resultingCompilation = resultingCompilation.AddSyntaxTrees( syntaxTree );
            }

            if (newSyntaxTrees.Count == 0)
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute returned no design time fallback source files." );

                return;
            }

            testResult.InputCompilationDiagnostics.Report( resultingCompilation.GetDiagnostics() );
            testResult.HasOutputCode = true;

            // TODO: Multiple syntax trees.
            await testResult.SyntaxTrees.Single().SetRunTimeCodeAsync( newSyntaxTrees.Single().GetRoot() );

            var emitResult = resultingCompilation.Emit( Stream.Null );

            testResult.PipelineDiagnostics.Report( emitResult.Diagnostics );

            if ( !emitResult.Success )
            {
                testResult.SetFailed( "Final Compilation.Emit failed." );
            }
        }

        private class TestAuxiliaryFileProvider : IAuxiliaryFileProvider
        {
            private readonly ImmutableArray<AuxiliaryFile> _files;

            public TestAuxiliaryFileProvider(ImmutableArray<AuxiliaryFile> files)
            {
                this._files = files;
            }

            public ImmutableArray<AuxiliaryFile> GetAuxiliaryFiles()
            {
                return this._files;
            }
        }

        private class DesignTimeFallbackProjectOptions : IProjectOptions
        {
            private readonly IProjectOptions _underlying;

            public string ProjectId => this._underlying.ProjectId;

            public string? BuildTouchFile => this._underlying.BuildTouchFile;

            public string? AssemblyName => this._underlying.AssemblyName;

            public ImmutableArray<object> PlugIns => this._underlying.PlugIns;

            public bool IsFrameworkEnabled => this._underlying.IsFrameworkEnabled;

            public bool FormatOutput => this._underlying.FormatOutput;

            public bool FormatCompileTimeCode => this._underlying.FormatCompileTimeCode;

            public bool IsUserCodeTrusted => this._underlying.IsUserCodeTrusted;

            public string? ProjectPath => this._underlying.ProjectPath;

            public string? TargetFramework => this._underlying.TargetFramework;

            public string? Configuration => this._underlying.Configuration;

            public bool DesignTimeEnabled => false;

            public string? AuxiliaryFileDirectoryPath => null;

            public bool DebugCompilerProcess => this._underlying.DebugCompilerProcess;

            public bool DebugAnalyzerProcess => this._underlying.DebugAnalyzerProcess;

            public bool DebugIdeProcess => this._underlying.DebugIdeProcess;

            public DesignTimeFallbackProjectOptions(IProjectOptions underlying)
            {
                this._underlying = underlying;
            }

            public IProjectOptions Apply( IProjectOptions options )
            {
                throw new System.NotSupportedException();
            }

            public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value )
            {
                return this._underlying.TryGetProperty( name, out value );
            }
        }

        private class RemovingRewriter : CSharpSyntaxRewriter
        {
            public static readonly RemovingRewriter Instance = new RemovingRewriter();

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                if (HasRemoveTrivia( node ) )
                {
                    return null;
                }

                return base.VisitClassDeclaration( node );
            }

            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node )
            {
                if ( HasRemoveTrivia( node ) )
                {
                    return null;
                }

                return base.VisitStructDeclaration( node );
            }

            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                if ( HasRemoveTrivia( node ) )
                {
                    return null;
                }

                return base.VisitRecordDeclaration( node );
            }

            public override SyntaxNode? VisitEnumDeclaration( EnumDeclarationSyntax node )
            {
                if ( HasRemoveTrivia( node ) )
                {
                    return null;
                }

                return base.VisitEnumDeclaration( node );
            }

            public override SyntaxNode? VisitInterfaceDeclaration( InterfaceDeclarationSyntax node )
            {
                if ( HasRemoveTrivia( node ) )
                {
                    return null;
                }

                return base.VisitInterfaceDeclaration( node );
            }

            private static bool HasRemoveTrivia( SyntaxNode typeDeclaration )
            {
                if ( typeDeclaration.HasLeadingTrivia )
                {
                    var leadingTrivia = typeDeclaration.GetLeadingTrivia();
                    if ( leadingTrivia.ToString().ContainsOrdinal( "<remove>" ) )
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}