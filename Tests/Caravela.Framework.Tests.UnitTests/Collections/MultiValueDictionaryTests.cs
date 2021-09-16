// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Collections;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Collections
{
    public class MultiValueDictionaryTests
    {
        [Fact]
        public void Basics()
        {
            var builder = ImmutableMultiValueDictionary<int, string>.CreateBuilder();
            builder.Add( 1, "1.1" );
            builder.Add( 1, "1.2" );
            builder.Add( 1, "1.3" );
            builder.Add( 2, "2.1" );
            builder.Add( 2, "2.2" );
            builder.Add( 2, "2.3" );
            var dictionary = builder.ToImmutable();

            Assert.Equal( new[] { 1, 2 }, dictionary.Keys );
            Assert.Equal( new[] { "1.1", "1.2", "1.3" }, dictionary[1] );
            Assert.Equal( new[] { "2.1", "2.2", "2.3" }, dictionary[2] );
        }

        [Fact]
        public void ExtensionMethod1()
        {
            List<(int, string)> list = new() { (1, "a"), (2, "b"), (1, "c") };
            var dictionary = list.ToMultiValueDictionary( i => i.Item1 );
            Assert.Equal( new[] { 1, 2 }, dictionary.Keys );
            Assert.Equal( new[] { (1, "a"), (1, "c") }, dictionary[1] );
        }

        [Fact]
        public void ExtensionMethod2()
        {
            List<(int, string)> list = new() { (1, "a"), (2, "b"), (1, "c") };
            var dictionary = list.ToMultiValueDictionary( i => i.Item1, i => i.Item2 );
            Assert.Equal( new[] { 1, 2 }, dictionary.Keys );
            Assert.Equal( new[] { "a", "c" }, dictionary[1] );
        }
    }
}