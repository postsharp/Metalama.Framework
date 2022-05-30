// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization
{
    public abstract class SerializerTestsBase : TestBase
    {
        private protected SerializerTestContext CreateSerializationTestContext( string code ) => new( this, code );

        private protected SerializerTestContext CreateSerializationTestContext( CompilationModel compilation ) => new( this, compilation );

        private protected class SerializerTestContext : TestContext
        {
            public CompilationModel Compilation { get; }

            public SerializerTestContext( TestBase parent, CompilationModel compilationModel ) : base( parent, null, null )
            {
                this.Compilation = compilationModel;

                // We need a syntax factory for an arbitrary compilation, but at least with standard references.
                // Note that we cannot easily get a reference to Metalama.Compiler.Interfaces this way because we have a reference assembly.

                this.SerializationContext = new SyntaxSerializationContext(
                    this.Compilation,
                    SyntaxGenerationContext.Create( this.ServiceProvider, compilationModel.RoslynCompilation ) );

                this.SerializationService = new SyntaxSerializationService( this.ServiceProvider );
            }

            public SerializerTestContext( TestBase parent, string code ) : base( parent, null, null )
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