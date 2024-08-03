// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

        var testContext = this.CreateTestContext( code );

        var attribute = testContext.Compilation.Types.OfName( "C" ).Single().Attributes.Single();

        var roundtrip = SerializeDeserialize( attribute.ToRef(), testContext ).GetTarget( testContext.Compilation );

        Assert.Equal( attribute.Type, roundtrip.Type );
        Assert.Equal( attribute.Constructor, roundtrip.Constructor );
        Assert.Equal( attribute.ConstructorArguments, roundtrip.ConstructorArguments );
        Assert.Equal( attribute.NamedArguments, roundtrip.NamedArguments );
    }
}