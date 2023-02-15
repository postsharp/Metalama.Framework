// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Pseudo;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Testing.UnitTesting;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Utilities;

public sealed class SerializableDeclarationIdTests : UnitTestClass
{
    public SerializableDeclarationIdTests( ITestOutputHelper? testOutputHelper = null ) : base( testOutputHelper, false ) { }

    [Fact]
    public void TestAllDeclarations()
    {
        const string code = @"

delegate void D();

class C<T> 
{
  void M<T2>(int p) {}
  int this[int i] => 0;
  int _field;
  int Property { get; set; }
  event System.EventHandler Event;

  C() {}
  ~C() {}

  static C() {}

}

";

        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilation( code );

        foreach ( var declaration in compilation.GetContainedDeclarations() )
        {
            Roundtrip( declaration, compilation, this.TestOutput );
        }

        Roundtrip( compilation, compilation, this.TestOutput );
    }

    internal static void Roundtrip( IDeclaration declaration, ICompilation compilation, ITestOutputHelper testOutput )
    {
        if ( declaration is PseudoParameter && declaration.ContainingDeclaration is IMethod { MethodKind: MethodKind.EventRaise } )
        {
            // Not yet implemented.
            return;
        }

        // Test declaration roundtrip from reference
        var roundtripFromReference = declaration.ToRef().GetTarget( compilation );
        Assert.Same( declaration, roundtripFromReference );

        // Test declaration roundtrip from serialization.
        var declarationId = declaration.ToSerializableId();
        testOutput.WriteLine( declarationId.Id );
        var roundtripFromDeclarationId = declarationId.Resolve( compilation );
        Assert.Same( declaration, roundtripFromDeclarationId );

        if ( declaration is INamespace { IsGlobalNamespace: true } )
        {
            // Roslyn does not support this, see https://github.com/dotnet/roslyn/issues/66976,
            // so skip testing symbols.
            return;
        }

        // Test symbol roundtrip.
        var symbol = declaration.GetSymbol();

        if ( symbol != null )
        {
            var symbolDeclarationId = symbol.GetSerializableId();
            var symbolRoundtrip = symbolDeclarationId.ResolveToSymbolOrNull( compilation.GetRoslynCompilation() );

            Assert.Same( symbol, symbolRoundtrip );

            var symbolRoundtripFromRef = Ref.FromSymbol( symbol, compilation.GetCompilationModel().CompilationContext )
                .GetSymbol( compilation.GetRoslynCompilation() );

            Assert.Same( symbol, symbolRoundtripFromRef );
        }
    }
}