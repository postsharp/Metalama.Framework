// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Comparers;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class StructuralSymbolComparerTests : TestBase
    {
        [Fact]
        public void Names()
        {
            const string code = @"
using System.Collections.Generic;

class A
{
    public void Foo() {}
    public void Bar() {}
    public void Bar(int x) {}
    public void Quz() {}
    public void Quz<T>() {}
}

class B
{
    public void Foo(int x) {}
    public void Foo(ref int x) {}
    public void Foo(int[] x) {}
    public void Foo(List<int> x) {}
    public void Foo(List<long> x) {}
    public unsafe void Foo(int* x) {}
    public void Bar<T>() {}
    public void Quz(ref int x) {}
}

namespace C
{
    class B {}
}

namespace D {}
";

            using var testContext = this.CreateTestContext();

            var compilation1 = testContext.CreateCompilationModel( code );
            var compilation2 = testContext.CreateCompilationModel( code );

            var declarations1 = compilation1.GetContainedDeclarations().ToList();
            var declarations2 = compilation2.GetContainedDeclarations().ToList();

            for ( var i1 = 0; i1 < declarations1.Count; i1++ )
            {
                for ( var i2 = 0; i1 < i2; i2++ )
                {
                    var symbol1 = declarations1[i1].GetSymbol();
                    var symbol2 = declarations2[i2].GetSymbol();

                    if ( symbol1 != null && symbol2 != null )
                    {
                        Assert.Equal( i1 == i2, StructuralSymbolComparer.Default.Equals( symbol1, symbol2 ) );
                    }
                }
            }
        }

        // TODO: More tests.
    }
}