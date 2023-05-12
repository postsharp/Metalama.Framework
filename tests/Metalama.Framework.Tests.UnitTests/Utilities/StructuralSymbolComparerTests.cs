// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Utilities
{
    public sealed class StructuralSymbolComparerTests : UnitTestClass
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
    public T Quz<T>() => default(T);
    public void Quz2<T>() {}
}

class B
{
    public void Foo(int x) {}
    public void Foo(ref int x) {}
    public void Foo(int[] x) {}
    public void Foo(List<int> x) {}
    public void Foo(List<long> x) {}
    public unsafe void Foo(int* x) {}
    public unsafe void Foo(long* x) {}
    public void Bar<T>() {}
    public void Bar<T,U>() {}
    public void Quz(ref int x) {}
}

class Z<T,U> : A
    where T : A
    where U : A
{
    public void Test()
    {
        A a = new A();

        a.Foo();
        a.Bar();
        a.Bar(5);
        a.Quz();
        a.Quz<int>();
        a.Quz<long>();
        a.Quz<T>();
        a.Quz<U>();
        a.Quz2<T>();

        var b = a.Quz<Z<U,T>>();
        b.Test();
    }
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

            AssertSymbolsEqual( compilation1, compilation2, StructuralSymbolComparer.Default );
        }

        // TODO: More tests.

        private static void AssertSymbolsEqual( CompilationModel compilation1, CompilationModel compilation2, StructuralSymbolComparer comparer )
        {
            var symbols1 = GetAllSymbols( compilation1 );
            var symbols2 = GetAllSymbols( compilation2 );

            Assert.Equal( symbols1.Count, symbols2.Count );

            // Do comparison of all symbols on the same position.
            for ( var i = 0; i < symbols1.Count; i++ )
            {
                Assert.True( comparer.Equals( symbols1[i], symbols2[i] ) );
                Assert.Equal( comparer.GetHashCode( symbols1[i] ), comparer.GetHashCode( symbols2[i] ) );
            }

            // Group symbol using SymbolEqualityComparer (we presume it's correct).
            var groups1 = symbols1.GroupBy( x => x, SymbolEqualityComparer.Default ).Select( g => g.ToArray() ).ToArray();
            var groups2 = symbols2.GroupBy( x => x, SymbolEqualityComparer.Default ).Select( g => g.ToArray() ).ToArray();

            Assert.Equal( groups1.Length, groups2.Length );

            // Test equality across compilation within one group.
            for ( var gi = 0; gi < groups1.Length; gi++ )
            {
                Assert.Equal( groups1[gi].Length, groups2[gi].Length );

                var expectedHash = comparer.GetHashCode( groups1[gi][0] );

                for ( var i = 0; i < groups1[gi].Length; i++ )
                {
                    for ( var k = 0; k < groups1[gi].Length; k++ )
                    {
                        Assert.True( comparer.Equals( groups1[gi][i], groups2[gi][k] ) );
                        Assert.True( comparer.Equals( groups2[gi][i], groups1[gi][k] ) );
                    }

                    Assert.Equal( expectedHash, comparer.GetHashCode( groups1[gi][i] ) );
                    Assert.Equal( expectedHash, comparer.GetHashCode( groups2[gi][i] ) );
                }
            }

            // Test inequality across compilation across groups.
            for ( var gi1 = 0; gi1 < groups1.Length; gi1++ )
            {
                for ( var gi2 = 0; gi2 < groups1.Length; gi2++ )
                {
                    if ( gi1 == gi2 )
                    {
                        continue;
                    }

                    for ( var i = 0; i < groups1[gi1].Length; i++ )
                    {
                        for ( var k = 0; k < groups1[gi2].Length; k++ )
                        {
                            Assert.False( comparer.Equals( groups1[gi1][i], groups2[gi2][k] ) );
                            Assert.False( comparer.Equals( groups2[gi1][i], groups1[gi2][k] ) );
                        }
                    }
                }
            }

            static IReadOnlyList<ISymbol> GetAllSymbols( CompilationModel compilation )
            {
                var symbols = new List<ISymbol>();

                foreach ( var syntaxTree in compilation.RoslynCompilation.SyntaxTrees )
                {
                    var semanticModel = compilation.RoslynCompilation.GetSemanticModel( syntaxTree );
                    var symbolFinder = new SymbolFinder( semanticModel, symbols );

                    symbolFinder.Visit( syntaxTree.GetRoot() );
                }

                return symbols;
            }
        }

        private sealed class SymbolFinder : CSharpSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private readonly List<ISymbol> _symbols;

            public SymbolFinder( SemanticModel semanticModel, List<ISymbol> symbols )
            {
                this._semanticModel = semanticModel;
                this._symbols = symbols;
            }

            public override void Visit( SyntaxNode? node )
            {
                if ( node != null )
                {
                    var declaredSymbol = this._semanticModel.GetDeclaredSymbol( node );

                    if ( declaredSymbol != null )
                    {
                        this._symbols.Add( declaredSymbol );
                    }

                    var referencedSymbol = this._semanticModel.GetSymbolInfo( node ).Symbol;

                    if ( referencedSymbol != null )
                    {
                        this._symbols.Add( referencedSymbol );
                    }
                }

                base.Visit( node );
            }
        }
    }
}