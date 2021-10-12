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
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections.Generic;
class C
{
    IEnumerable<int> Enumerable1() { yield return 1; }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );
        }

        [Fact]
        public void GenericEnumeratorYield()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections.Generic;
class C
{
    IEnumerator<int> Enumerable1() { yield return 1; }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );
        }

        [Fact]
        public void AsyncEnumerableYield()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections.Generic;
class C
{
    async IAsyncEnumerable<int> Enumerable1() { yield return 1; }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IAsyncEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );
        }

        [Fact]
        public void AsyncEnumeratorYield()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections.Generic;
class C
{
    async IAsyncEnumerator<int> Enumerable1() { yield return 1; }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IAsyncEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );
        }

        [Fact]
        public void NonGenericEnumeratorYield()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections;
class C
{
    IEnumerator Enumerable1() { yield return 1; }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.UntypedIEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(object) ), iteratorInfo.ItemType );
        }

        [Fact]
        public void NonGenericEnumerableYield()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections;
class C
{
    IEnumerable Enumerable1() { yield return 1; }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.True( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.UntypedIEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(object) ), iteratorInfo.ItemType );
        }

        [Fact]
        public void GenericEnumerableNonYield()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections.Generic;
class C
{
    IEnumerable<int> Enumerable1() => null;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );
        }

        [Fact]
        public void GenericEnumeratorNonYield()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections.Generic;
class C
{
    IEnumerator<int> Enumerable1() => null;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );
        }

        [Fact]
        public void AsyncEnumerableNonYield()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections.Generic;
class C
{
     IAsyncEnumerable<int> Enumerable1() => null;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IAsyncEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );
        }

        [Fact]
        public void AsyncEnumeratorNonYield()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections.Generic;
class C
{
     IAsyncEnumerator<int> Enumerable1() => null;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.IAsyncEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(int) ), iteratorInfo.ItemType );
        }

        [Fact]
        public void NonGenericEnumeratorNonYield()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections;
class C
{
    IEnumerator Enumerable1() => null;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.UntypedIEnumerator, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(object) ), iteratorInfo.ItemType );
        }

        [Fact]
        public void NonGenericEnumerableNonYield()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System.Collections;
class C
{
    IEnumerable Enumerable1() => null;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var iteratorInfo = compilation.Types.Single().Methods.Single().GetIteratorInfo();

            Assert.False( iteratorInfo.IsIterator );
            Assert.Equal( EnumerableKind.UntypedIEnumerable, iteratorInfo.EnumerableKind );
            Assert.Equal( compilation.Factory.GetTypeByReflectionType( typeof(object) ), iteratorInfo.ItemType );
        }
    }
}