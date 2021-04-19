// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Serialization;
using System;
using System.Collections.Generic;
using Xunit;

// ReSharper disable BadListLineBreaks

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    // ReSharper disable UnusedTypeParameter
    // ReSharper disable ClassNeverInstantiated.Global

    public class TypeNameUtilityTests
    {
        [Fact]
        public void TestTypeNameUtility()
        {
            var typesToTest = new Dictionary<string, Type>
            {
                { "System.String", typeof(string) },
                { "System.String[]", typeof(string[]) },
                { "System.Object[]", typeof(object[]) },
                { "System.Boolean[]", typeof(bool[]) },
                { "System.Object", typeof(object) },
                { "System.Int32", typeof(int) },
                { "System.Double", typeof(double) },
                { "System.Single", typeof(float) },
                { "System.Boolean", typeof(bool) },
                { "System.Char", typeof(char) },
                { "System.Decimal", typeof(decimal) },
                { "System.Nullable<System.Decimal>[]", typeof(decimal?[]) },
                { "System.Nullable<System.Decimal>[][]", typeof(decimal?[][]) },
                { "System.Int64", typeof(long) },
                { "System.Guid", typeof(Guid) },
                { "System.Nullable<System.Int32>", typeof(int?) },
                { "System.Nullable<System.Double>", typeof(double?) },
                { "System.Nullable<System.Single>", typeof(float?) },
                { "System.Nullable<System.Boolean>", typeof(bool?) },
                { "System.Nullable<System.Char>", typeof(char?) },
                { "System.Nullable<System.Decimal>", typeof(decimal?) },
                { "System.Nullable<System.Int64>", typeof(long?) },
                { "System.Nullable<System.Guid>", typeof(Guid?) },
                { "System.Collections.Generic.List<System.String>", typeof(List<string>) },
                { "System.Collections.Generic.Dictionary<System.String, System.Guid>", typeof(Dictionary<string, Guid>) },
                { "System.Collections.Generic.Dictionary<System.String, System.Guid>[]", typeof(Dictionary<string, Guid>[]) },
                { "System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>", typeof(Dictionary<string, Guid?>) },
                {
                    "System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>>",
                    typeof(Dictionary<string, Dictionary<string, Guid?>>)
                },
                {
                    "System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>>[]",
                    typeof(Dictionary<string, Dictionary<string, Guid?>>[])
                },
                {
                    "System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>>[][]",
                    typeof(Dictionary<string, Dictionary<string, Guid?>>[][])
                },
                { "System.Int32[]", typeof(int[]) },
                { "System.Int32[][]", typeof(int[][]) },
                { "System.Int32[][][]", typeof(int[][][]) },
                { "System.Int32[][][][]", typeof(int[][][][]) },
                { "System.Int32[][][][][]", typeof(int[][][][][]) },
                { "Caravela.Framework.Tests.UnitTests.Serialization.TestClass", typeof(TestClass) },
                { "System.Collections.Generic.List<Caravela.Framework.Tests.UnitTests.Serialization.TestClass>", typeof(List<TestClass>) },
                {
                    "System.Collections.Generic.Dictionary<Caravela.Framework.Tests.UnitTests.Serialization.TestClass, Caravela.Framework.Tests.UnitTests.Serialization.TestClass>",
                    typeof(Dictionary<TestClass, TestClass>)
                },
                {
                    "System.Collections.Generic.Dictionary<System.String, Caravela.Framework.Tests.UnitTests.Serialization.TestClass>",
                    typeof(Dictionary<string, TestClass>)
                },
                {
                    "System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String, Caravela.Framework.Tests.UnitTests.Serialization.TestClass>>",
                    typeof(List<Dictionary<string, TestClass>>)
                },
                {
                    "System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String, Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<System.String>>>",
                    typeof(List<Dictionary<string, GenericTestClass<string>>>)
                },
                {
                    "Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<System.String>.SecondSubType<System.Decimal>",
                    typeof(GenericTestClass<string>.SecondSubType<decimal>)
                },
                {
                    "Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<System.String>.SecondSubType",
                    typeof(GenericTestClass<string>.SecondSubType)
                },
                {
                    "Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<System.String, System.Int32>.SecondSubType",
                    typeof(GenericTestClass<string, int>.SecondSubType)
                },
                {
                    "Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<System.String, System.Collections.Generic.Dictionary<System.String, System.Int32>>.SecondSubType<System.String>",
                    typeof(GenericTestClass<string, Dictionary<string, int>>.SecondSubType<string>)
                },
                {
                    "Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<System.String, System.Collections.Generic.Dictionary<System.String, System.Int32>>.SecondSubType<Caravela.Framework.Tests.UnitTests.Serialization.GenericTestClass<System.String, System.Collections.Generic.Dictionary<System.String, System.Int32>>>",
                    typeof(GenericTestClass<string, Dictionary<string, int>>.SecondSubType<GenericTestClass<string, Dictionary<string, int>>>)
                }
            };

            foreach ( var t in typesToTest )
            {
                var actualName = TypeNameUtility.ToCSharpQualifiedName( t.Value );
                Assert.Equal( t.Key, actualName );
            }
        }
    }

#pragma warning disable SA1402 // File may only contain a single type

    public class TestClass { }

    public class GenericTestClass<T>
    {
        public class SecondSubType { }

        public class SecondSubType<T2> { }
    }

    public class GenericTestClass<T1, T2>
    {
        public class SecondSubType { }

        public class SecondSubType<T3> { }
    }
}