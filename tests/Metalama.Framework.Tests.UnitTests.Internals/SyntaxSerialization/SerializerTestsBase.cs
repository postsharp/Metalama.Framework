// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Testing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization
{
    public abstract class SerializerTestsBase : TestBase
    {
        private TestProjectOptions CreateProjectOptions() => new( additionalAssemblies: ImmutableArray.Create( this.GetType().Assembly ) );

        private protected SerializerTestContext CreateSerializationTestContext( string code ) => new( code, this.CreateProjectOptions() );

        private protected SerializerTestContext CreateSerializationTestContext( CompilationModel compilation )
            => new( compilation, this.CreateProjectOptions() );

        protected SerializerTestsBase( ITestOutputHelper? logger = null ) : base( logger ) { }

        private protected class SerializerTestContext : TestContext
        {
            public CompilationModel Compilation { get; }

            public SerializerTestContext( CompilationModel compilationModel, TestProjectOptions projectOptions ) : base( projectOptions )
            {
                this.Compilation = compilationModel;

                // We need a syntax factory for an arbitrary compilation, but at least with standard references.
                // Note that we cannot easily get a reference to Metalama.Compiler.Interfaces this way because we have a reference assembly.

                this.SerializationContext = new SyntaxSerializationContext(
                    this.Compilation,
                    SyntaxGenerationContext.Create( this.ServiceProvider, compilationModel.RoslynCompilation ) );

                this.SerializationService = new SyntaxSerializationService( this.ServiceProvider );
            }

            public SerializerTestContext( string code, TestProjectOptions projectOptions ) : base( projectOptions )
            {
                this.Compilation = this.CreateCompilationModel( code );

                // We need a syntax factory for an arbitrary compilation, but at least with standard references.
                // Note that we cannot easily get a reference to Metalama.Compiler.Interfaces this way because we have a reference assembly.

                this.SerializationContext = new SyntaxSerializationContext(
                    this.Compilation,
                    SyntaxGenerationContext.Create( this.ServiceProvider, this.Compilation.RoslynCompilation ) );

                this.SerializationService = new SyntaxSerializationService( this.ServiceProvider );
            }

            public SyntaxSerializationContext SerializationContext { get; }

            public SyntaxSerializationService SerializationService { get; }

            public ExpressionSyntax Serialize<T>( T o ) => this.SerializationService.Serialize( o, this.SerializationContext );
        }
    }
}