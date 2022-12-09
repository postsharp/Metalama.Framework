// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Pseudo;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Testing.UnitTesting;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Utilities;

public class SerializableDeclarationIdTests : UnitTestClass
{
    public SerializableDeclarationIdTests( ITestOutputHelper? testOutputHelper = null ) : base( testOutputHelper ) { }

    [Fact]
    public void TestAllDeclarations()
    {
        var code = @"

class C<T> 
{
  void M<T2>(int p) {}
  int this[int i] => 0;
  int _field;
  event System.EventHandler Event;

  C() {}
  ~C() {}

}

";

        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilation( code );

        foreach ( var declaration in compilation.GetContainedDeclarations() )
        {
            Roundloop( declaration );
        }

        void Roundloop( IDeclaration declaration )
        {
            if ( declaration is PseudoParameter && declaration.ContainingDeclaration is IMethod { MethodKind: MethodKind.EventRaise } )
            {
                // Not yet implemented.
                return;
            }

            var declarationId = declaration.ToSerializableId();

            this.TestOutput.WriteLine( declarationId.Id );

            // Test declaration roundloop.
            var roundloop = declarationId.Resolve( compilation );
            Assert.Same( declaration, roundloop );

            // Test symbol roundloop.
            var symbol = declaration.GetSymbol();

            if ( symbol != null )
            {
                var symbolDeclarationId = symbol.GetSerializableId();

                var symbolRoundloop = symbolDeclarationId.ResolveToSymbol( compilation.GetRoslynCompilation() );

                Assert.Same( symbol, symbolRoundloop );
            }
        }
    }
}