// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CodeModel
{
    public class IteratorInfoTests : TestBase
    {
           
        [Fact]
        public void GenericEnumerableYield()
        {
            var code = @"
using System.Collections.Generic;
class C
{
    IEnumerable<int> Enumerable1() { yield return 1; }
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );

        }
        
                   
        [Fact]
        public void GenericEnumeratorYield()
        {
            var code = @"
using System.Collections.Generic;
class C
{
    IEnumerator<int> Enumerable1() { yield return 1; }
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );

        }
        
        [Fact]
        public void AsyncEnumerableYield()
        {
            var code = @"
using System.Collections.Generic;
class C
{
    async IAsyncEnumerable<int> Enumerable1() { yield return 1; }
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IAsyncEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );

        }
        
        [Fact]
        public void AsyncEnumeratorYield()
        {
            var code = @"
using System.Collections.Generic;
class C
{
    async IAsyncEnumerator<int> Enumerable1() { yield return 1; }
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IAsyncEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );

        }

        
                   
        
                           
        [Fact]
        public void NonGenericEnumeratorYield()
        {
            var code = @"
using System.Collections;
class C
{
    IEnumerator Enumerable1() { yield return 1; }
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.UntypedIEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(object) ), iteratorInfo.ItemType );

        }
        
        [Fact]
        public void NonGenericEnumerableYield()
        {
            var code = @"
using System.Collections;
class C
{
    IEnumerable Enumerable1() { yield return 1; }
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.UntypedIEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(object) ), iteratorInfo.ItemType );

        }
        
             
        [Fact]
        public void GenericEnumerableNonYield()
        {
            var code = @"
using System.Collections.Generic;
class C
{
    IEnumerable<int> Enumerable1() => null;
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );

        }
        
                   
        [Fact]
        public void GenericEnumeratorNonYield()
        {
            var code = @"
using System.Collections.Generic;
class C
{
    IEnumerator<int> Enumerable1() => null;
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );

        }
        
        [Fact]
        public void AsyncEnumerableNonYield()
        {
            var code = @"
using System.Collections.Generic;
class C
{
     IAsyncEnumerable<int> Enumerable1() => null;
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IAsyncEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );

        }
        
        [Fact]
        public void AsyncEnumeratorNonYield()
        {
            var code = @"
using System.Collections.Generic;
class C
{
     IAsyncEnumerator<int> Enumerable1() => null;
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IAsyncEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );

        }
                   
        
                           
        [Fact]
        public void NonGenericEnumeratorNonYield()
        {
            var code = @"
using System.Collections;
class C
{
    IEnumerator Enumerable1() => null;
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.UntypedIEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(object) ), iteratorInfo.ItemType );

        }
        
        [Fact]
        public void NonGenericEnumerableNonYield()
        {
            var code = @"
using System.Collections;
class C
{
    IEnumerable Enumerable1() => null;
}
";

            var compilation = CreateCompilationModel( code );
            var iteratorInfo = compilation.DeclaredTypes.Single().Methods.Single().GetIteratorInfo();
            
            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.UntypedIEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(object) ), iteratorInfo.ItemType );

        }
        
    }
}