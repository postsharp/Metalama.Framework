// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Comparers;
using Microsoft.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class StructuralSymbolComparerTests : TestBase
    {
        private static void Equal( StructuralSymbolComparer comparer, ISymbol x, ISymbol y )
        {
            Assert.True( comparer.Equals( x, y ) );
            Assert.True( comparer.Equals( y, x ) );
            Assert.Equal( comparer.GetHashCode( x ), comparer.GetHashCode( y ) );
        }

        private static void NotEqual( StructuralSymbolComparer comparer, ISymbol x, ISymbol y )
        {
            Assert.False( comparer.Equals( x, y ) );
            Assert.False( comparer.Equals( y, x ) );
        }

        [Fact]
        public void Names()
        {
            const string code = @"
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
    public void Bar<T>() {}
    public void Quz(ref int x) {}
}

namespace C
{
    class B {}
}

namespace D {}
";

            var comparer = new StructuralSymbolComparer( StructuralSymbolComparerOptions.Name );

            var compilation = CreateCSharpCompilation( code );
            var globalNamespaceMembers = compilation.Assembly.Modules.Single().GlobalNamespace.GetMembers().ToArray();
            var typeA = globalNamespaceMembers[0];
            var typeB = globalNamespaceMembers[1];
            var namespaceC = globalNamespaceMembers[2];
            var typeCb = namespaceC.GetMembers().Single();
            var namespaceD = globalNamespaceMembers[3];
            var typeAFoo = typeA.GetMembers()[0];
            var typeABar = typeA.GetMembers()[1];
            var typeABarInt = typeA.GetMembers()[2];
            var typeAQuz = typeA.GetMembers()[3];
            var typeAQuzT = typeA.GetMembers()[4];
            var typeBFooInt = typeB.GetMembers()[0];
            var typeBBarT = typeB.GetMembers()[1];
            var typeBQuzInt = typeB.GetMembers()[2];

            // Namespaces
            NotEqual( comparer, namespaceC, namespaceD );

            // Types
            NotEqual( comparer, typeA, typeB );
            Equal( comparer, typeB, typeCb );

            // Methods
            NotEqual( comparer, typeAFoo, typeABar );
            NotEqual( comparer, typeABar, typeAQuzT );
            NotEqual( comparer, typeAQuz, typeBFooInt );
            NotEqual( comparer, typeBBarT, typeAQuzT );

            Equal( comparer, typeAFoo, typeBFooInt );

            Equal( comparer, typeABar, typeABarInt );
            Equal( comparer, typeABar, typeBBarT );
            Equal( comparer, typeABarInt, typeBBarT );

            Equal( comparer, typeAQuz, typeAQuzT );
            Equal( comparer, typeAQuz, typeBQuzInt );
            Equal( comparer, typeAQuzT, typeBQuzInt );
        }

        // TODO: More tests.
    }
}