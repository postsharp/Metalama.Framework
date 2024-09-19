﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CompileTime.Serialization;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.UnitTesting;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.LamaSerialization;

public class AttributeSerializationTests : SerializationTestsBase
{
    [Fact]
    public void RoundtripTest()
    {
        const string code = """
                            [TheAttribute("constructorArgumentValue",
                                           "constructorArrayItem1",
                                           "constructorArrayItem2",
                                           typeof(TheAttribute),
                                           System.ConsoleColor.Black,
                                           NamedArgument = "namedArgumentValue",
                                           NamedArrayArgument = [
                                               "namedArrayArgumentItem1",
                                               "namedArrayArgumentItem2",
                                               typeof(TheAttribute),
                                               System.ConsoleColor.Red ] )]
                            public class C;
                            public class TheAttribute : System.Attribute
                            {
                                public TheAttribute(string constructorArgument, params object[] constructorArrayArgument) { }
                            
                                public string NamedArgument
                                {
                                    get;
                                    set;
                                }
                                public object[] NamedArrayArgument
                                {
                                    get;
                                    set;
                                }
                            }

                            """;

        using var testContext = this.CreateTestContext( code );

        var attribute = testContext.Compilation.Types.OfName( "C" ).Single().Attributes.Single();

        var roundtrip = SerializeDeserialize( attribute.ToRef(), testContext ).GetTarget( testContext.Compilation );

        Assert.Equal( attribute.Type, roundtrip.Type );
        Assert.Equal( attribute.Constructor, roundtrip.Constructor );
        Assert.Equal( attribute.ConstructorArguments, roundtrip.ConstructorArguments );
        Assert.Equal( attribute.NamedArguments, roundtrip.NamedArguments );

        // Non-ref serialization must fail.
        Assert.Throws<CompileTimeSerializationException>( () => SerializeDeserialize( attribute, testContext ) );
    }

    [Fact]
    public void AttributeDataFromDifferentCompilationModelsAreSame()
    {
        // This is to test that two models of the same compilation have identical attribute serialization keys.

        var code = """
                   public class TheAttribute : System.Attribute;

                   [TheAttribute]
                   public class C;

                   """;

        using var testContext = this.CreateTestContext( code );

        var compilationModel1 = testContext.Compilation;
        var attribute1 = compilationModel1.Types.OfName( "C" ).Single().Attributes.Single();

        Assert.True( ((AttributeRef) attribute1.ToRef()).TryGetAttributeSerializationDataKey( out var attributeKey1 ) );

        var compilationModel2 = testContext.CreateCompilationModel( testContext.Compilation.RoslynCompilation );
        var attribute2 = compilationModel1.Types.OfName( "C" ).Single().Attributes.Single();

        Assert.True( ((AttributeRef) attribute2.ToRef()).TryGetAttributeSerializationDataKey( out var attributeKey2 ) );

        // Test that two serialization keys of the same attribute in two models resolve are identical. 
        Assert.Same( attributeKey1, attributeKey2 );

        // Test that two references to the same attribute resolve to the same IAttribute.
        var array = new[] { attribute1.ToRef(), attribute1.ToRef() };
        var roundtripArray = TestSerialization( testContext, array, testEquality: false );

        Assert.Same( roundtripArray[0].GetTarget( compilationModel1 ), roundtripArray[1].GetTarget( compilationModel1 ) );
    }
}