// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.AdditionalOutputs;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.TestFramework.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.DesignTime
{
    public class FallbackTests : TestBase
    {
        [Fact]
        public async Task Declarative()
        {
            const string code = @"
using Caravela.Framework.Aspects;

public class IntroductionAspectAttribute : TypeAspect
{
    [Introduce]
    public int Foo()
    {
        return 42;
    }
}
    
[IntroductionAspect]
public partial class TargetClass
{
    public void Bar()
    {
        _ = this.Foo();
    }
}

// <remove>
public partial class TargetClass
{
    public int Foo()
    {
        return 0;
    }
}
";

            await this.RunTest( code );
        }

        [Fact]
        public async Task Programmatic()
        {
            const string code = @"
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

public class IntroductionAspectAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advices.IntroduceMethod(builder.Target, nameof(Foo));
    }

    [Template]
    public int Foo()
    {
        return 42;
    }
}
    
[IntroductionAspect]
public partial class TargetClass
{
    public void Bar()
    {
        _ = this.Foo();
    }
}

// <remove>
public partial class TargetClass
{
    public int Foo()
    {
        return 0;
    }
}
";

            await this.RunTest( code );
        }

        [Fact]
        public async Task NonPartial()
        {
            const string code = @"
using Caravela.Framework.Aspects;

public class IntroductionAspectAttribute : TypeAspect
{
    [Introduce]
    public int Foo()
    {
        return 42;
    }
}
    
[IntroductionAspect]
public class TargetClass
{
    public void Bar()
    {
    }
}
";

            await this.RunTest( code );
        }

        public async Task RunTest( string code )
        {
            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();
            var inputCompilation = CreateCSharpCompilation( code );

            // Create a compilation from the input compilation while removing nodes marked for removal.
            foreach ( var inputSyntaxTree in inputCompilation!.SyntaxTrees )
            {
                inputCompilation = inputCompilation.ReplaceSyntaxTree(
                    inputSyntaxTree,
                    inputSyntaxTree.WithRootAndOptions( RemovingRewriter.Instance.Visit( inputSyntaxTree.GetRoot() ), inputSyntaxTree.Options ) );
            }

            var projectOptions = testContext.ServiceProvider.GetService<IProjectOptions>();

            // Replace the project options to enable design time fallback.
            var designTimeFallbackServiceProvider =
                testContext.ServiceProvider.WithService( new DesignTimeFallbackProjectOptions( testContext.ProjectOptions ) );

            using var compileTimePipeline = new CompileTimeAspectPipeline(
                designTimeFallbackServiceProvider,
                true,
                domain,
                ExecutionScenario.CompileTime );

            var diagnosticList = new DiagnosticList();

            var compileTimeResult = await compileTimePipeline.ExecuteAsync( diagnosticList, inputCompilation, default, CancellationToken.None );

            if ( compileTimeResult == null )
            {
                throw new AssertionFailedException( "CompileTimeAspectPipeline.TryExecute failed." );
            }

            // Create a compilation from the input compilation with removed nodes plus auxiliary files.
            var resultingCompilation = inputCompilation;

            var newSyntaxTrees = new List<SyntaxTree>();

            foreach ( var file in compileTimeResult.AdditionalCompilationOutputFiles.Where(
                f => f.Kind == AdditionalCompilationOutputFileKind.DesignTimeGeneratedCode ) )
            {
                using var outputStream = new MemoryStream();
                file.WriteToStream( outputStream );
                using var inputStream = new MemoryStream( outputStream.ToArray() );
                var syntaxTree = CSharpSyntaxTree.ParseText( SourceText.From( inputStream ) );
                newSyntaxTrees.Add( syntaxTree );
                resultingCompilation = resultingCompilation.AddSyntaxTrees( syntaxTree );
            }

            var compilationDiagnostics = resultingCompilation.GetDiagnostics();

            Assert.Empty( compilationDiagnostics );

            var emitResult = resultingCompilation.Emit( Stream.Null );

            Assert.Empty( emitResult.Diagnostics );
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

            public bool IsDesignTimeEnabled => false;

            public string? AdditionalCompilationOutputDirectory => null;

            public bool DebugCompilerProcess => this._underlying.DebugCompilerProcess;

            public bool DebugAnalyzerProcess => this._underlying.DebugAnalyzerProcess;

            public bool DebugIdeProcess => this._underlying.DebugIdeProcess;

            public DesignTimeFallbackProjectOptions( IProjectOptions underlying )
            {
                this._underlying = underlying;
            }

            public IProjectOptions Apply( IProjectOptions options )
            {
                throw new NotSupportedException();
            }

            public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value )
            {
                return this._underlying.TryGetProperty( name, out value );
            }
        }

        private class RemovingRewriter : CSharpSyntaxRewriter
        {
            public static readonly RemovingRewriter Instance = new();

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                if ( HasRemoveTrivia( node ) )
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