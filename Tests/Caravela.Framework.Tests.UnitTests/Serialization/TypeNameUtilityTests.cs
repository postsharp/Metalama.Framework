// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Xunit;

// ReSharper disable BadListLineBreaks

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    // ReSharper disable UnusedTypeParameter
    // ReSharper disable ClassNeverInstantiated.Global

    public class TypeNameUtilityTests : SerializerTestsBase
    {
        [Fact]
        public void TestTypeNameUtility()
        {
            var typesToTest = new Dictionary<string, Type>
            {
                { "global::System.String", typeof(string) },
                { "global::System.String[]", typeof(string[]) },
                { "global::System.Object[]", typeof(object[]) },
                { "global::System.Boolean[]", typeof(bool[]) },
                { "global::System.Object", typeof(object) },
                { "global::System.Int32", typeof(int) },
                { "global::System.Double", typeof(double) },
                { "global::System.Single", typeof(float) },
                { "global::System.Boolean", typeof(bool) },
                { "global::System.Char", typeof(char) },
                { "global::System.Decimal", typeof(decimal) },
                { "global::System.Decimal?[]", typeof(decimal?[]) },
                { "global::System.Decimal?[][]", typeof(decimal?[][]) },
                { "global::System.Int64", typeof(long) },
                { "global::System.Guid", typeof(Guid) },
                { "global::System.Int32?", typeof(int?) },
                { "global::System.Double?", typeof(double?) },
                { "global::System.Single?", typeof(float?) },
                { "global::System.Boolean?", typeof(bool?) },
                { "global::System.Char?", typeof(char?) },
                { "global::System.Decimal?", typeof(decimal?) },
                { "global::System.Int64?", typeof(long?) },
                { "global::System.Guid?", typeof(Guid?) },
                { "global::System.Collections.Generic.List<global::System.String>", typeof(List<string>) },
                { "global::System.Collections.Generic.Dictionary<global::System.String, global::System.Guid>", typeof(Dictionary<string, Guid>) },
                { "global::System.Collections.Generic.Dictionary<global::System.String, global::System.Guid>[]", typeof(Dictionary<string, Guid>[]) },
                { "global::System.Collections.Generic.Dictionary<global::System.String, global::System.Guid?>", typeof(Dictionary<string, Guid?>) },
                {
                    "global::System.Collections.Generic.Dictionary<global::System.String, global::System.Collections.Generic.Dictionary<global::System.String, global::System.Nullable<global::System.Guid>>>",
                    typeof(Dictionary<string, Dictionary<string, Guid?>>)
                },
                {
                    "global::System.Collections.Generic.Dictionary<global::System.String, global::System.Collections.Generic.Dictionary<global::System.String, global::System.Nullable<global::System.Guid>>>[]",
                    typeof(Dictionary<string, Dictionary<string, Guid?>>[])
                },
                {
                    "global::System.Collections.Generic.Dictionary<global::System.String, global::System.Collections.Generic.Dictionary<global::System.String, global::System.Nullable<global::System.Guid>>>[][]",
                    typeof(Dictionary<string, Dictionary<string, Guid?>>[][])
                },
                { "global::System.Int32[]", typeof(int[]) },
                { "global::System.Int32[][]", typeof(int[][]) },
                { "global::System.Int32[][][]", typeof(int[][][]) },
                { "global::System.Int32[][][][]", typeof(int[][][][]) },
                { "global::System.Int32[][][][][]", typeof(int[][][][][]) },
                { "global::Caravela.Framework.Tests.UnitTests.Serialization.TestClass", typeof(TestClass) },
                { "global::System.Collections.Generic.List<global::Caravela.Framework.Tests.UnitTests.Serialization.TestClass>", typeof(List<TestClass>) },
                {
                    "global::System.Collections.Generic.Dictionary<global::Caravela.Framework.Tests.UnitTests.Serialization.TestClass, global::Caravela.Framework.Tests.UnitTests.Serialization.TestClass>",
                    typeof(Dictionary<TestClass, TestClass>)
                },
                {
                    "global::System.Collections.Generic.Dictionary<global::System.String, global::Caravela.Framework.Tests.UnitTests.Serialization.TestClass>",
                    typeof(Dictionary<string, TestClass>)
                },
                {
                    "global::System.Collections.Generic.List<global::System.Collections.Generic.Dictionary<global::System.String, global::Caravela.Framework.Tests.UnitTests.Serialization.TestClass>>",
                    typeof(List<Dictionary<string, TestClass>>)
                },
                {
                    "global::System.Collections.Generic.List<global::System.Collections.Generic.Dictionary<global::System.String, global::Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<global::System.String>>>",
                    typeof(List<Dictionary<string, GenericTestClass<string>>>)
                },
                {
                    "global::Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<global::System.String>.SecondSubType<global::System.Decimal>",
                    typeof(GenericTestClass<string>.SecondSubType<decimal>)
                },
                {
                    "global::Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<global::System.String>.SecondSubType",
                    typeof(GenericTestClass<string>.SecondSubType)
                },
                {
                    "global::Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<global::System.String, global::System.Int32>.SecondSubType",
                    typeof(GenericTestClass<string, int>.SecondSubType)
                },
                {
                    "global::Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<global::System.String, global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>.SecondSubType<global::System.String>",
                    typeof(GenericTestClass<string, Dictionary<string, int>>.SecondSubType<string>)
                },
                {
                    "global::Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<global::System.String, global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>.SecondSubType<global::Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<global::System.String, global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>>",
                    typeof(GenericTestClass<string, Dictionary<string, int>>.SecondSubType<GenericTestClass<string, Dictionary<string, int>>>)
                }
            };

            foreach ( var t in typesToTest )
            {
                var actualName = this.SyntaxFactory.GetTypeSyntax( t.Value ).ToString();
                Assert.Equal( t.Key, actualName );
            }
        }
    }

#pragma warning disable SA1402 // File may only contain a single type

    public class GenericTestClass<T1, T2>
    {
        public class SecondSubType { }

        public class SecondSubType<T3> { }
    }
}