﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Testing.UnitTesting;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel;

public class GenericContextTests : UnitTestClass
{
    [Fact]
    public void NonGenericTypeNestedInGenericType()
    {
        using var testContext = this.CreateTestContext();

        // This basically tests that [Memo] works.

        const string code = """
                            class C<T>
                            {
                              class D;
                              C<int> f;
                            }
                            """;

        var compilation = testContext.CreateCompilationModel( code );

        var classC = compilation.Types.OfName( "C" ).Single();
        var classD = classC.Types.Single();
        var genericTypeInstance = (NamedType) classC.Fields.Single().Type;

        // Map through IType.
        var mappedClassD = genericTypeInstance.GenericContext.Map( classD );
        Assert.Equal( "C<int>.D", mappedClassD.ToString() );

        // Map through ITypeSymbol.
        var mappedClassDSymbol = genericTypeInstance.GenericContext.Map( classD.GetSymbol() );
        Assert.Equal( "C<int>.D", mappedClassDSymbol.ToString() );
    }

    [Fact]
    public void GenericTypeNestedInGenericType()
    {
        using var testContext = this.CreateTestContext();

        // This basically tests that [Memo] works.

        const string code = """
                            class C<T>
                            {
                              class D<S>;
                              C<int>.D<string> f;
                            }
                            """;

        var compilation = testContext.CreateCompilationModel( code );

        var classC = compilation.Types.OfName( "C" ).Single();
        var classD = classC.Types.Single();
        var genericTypeInstance = (NamedType) classC.Fields.Single().Type;

        // Map through IType.
        var mappedClassD = genericTypeInstance.GenericContext.Map( classD );
        Assert.Equal( "C<int>.D<string>", mappedClassD.ToString() );

        // Map through ITypeSymbol.
        var mappedClassDSymbol = genericTypeInstance.GenericContext.Map( classD.GetSymbol() );
        Assert.Equal( "C<int>.D<string>", mappedClassDSymbol.ToString() );
    }
}