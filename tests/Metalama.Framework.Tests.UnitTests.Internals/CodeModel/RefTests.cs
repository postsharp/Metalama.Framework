// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel;

public class RefTests : TestBase
{
    [Fact]
    public void CompilationRef()
    {
        var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( "/* nothing */" );

        var compilationRef = compilation.ToRef();
        var resolved = compilationRef.GetTarget( compilation );

        Assert.Same( compilation, resolved );
    }

    [Fact]
    public void CompilationSymbolId()
    {
        var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( "/* nothing */" );
        var symbolId = SymbolId.Create( compilation.Symbol );
        var resolvedSymbol = symbolId.Resolve( compilation.RoslynCompilation ).AssertNotNull();
        var resolvedDeclaration = compilation.Factory.GetCompilationElement( resolvedSymbol );

        Assert.Same( compilation, resolvedDeclaration );
    }

    [Fact]
    public void ReferencedAssemblySymbol()
    {
        var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( "/* nothing */" );

        var assemblyRefSymbol = compilation.Factory.GetTypeByReflectionType( typeof(string) ).GetSymbol();
        var assemblyRefRef = SymbolId.Create( assemblyRefSymbol );
        _ = assemblyRefRef.Resolve( compilation.RoslynCompilation );
    }
}