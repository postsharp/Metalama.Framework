// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public class AttributeDiscoveryTests : TestBase
    {
        [Fact]
        public void Resolution()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
[assembly: MyAttribute(1)]
[module: MyAttribute(2)]

class MyAttribute : System.Attribute { public MyAttribute( int id ) {} }

[MyAttribute(3)]
class C< [MyAttribute(4)]T> 
{
    [MyAttribute(5)]
    [return: MyAttribute(6)]
   void M( [MyAttribute(7)] int p ) {}

   [MyAttribute(8)]
    int f, g;

    [MyAttribute(9)]
    [field: MyAttribute(10)]
    int P 
    {
        get;
        [param: MyAttribute(11)]set; 
    }

    [method: MyAttribute(12)] // Does not seem to work. Roslyn does not represent the attribute.
    string P2 => """";

    [MyAttribute(13)]
    [field: MyAttribute(14)] // Does not seem to work.  Roslyn does not represent the attribute.
    event System.EventHandler ee, ff;

}

";

            var compilation = testContext.CreateCompilationModel( code, name: "test" );
            var myAttribute = compilation.Types.OfName( "MyAttribute" ).Single();

            var targets = compilation.GetAllAttributesOfType( myAttribute )
                .OrderBy( a => a.ConstructorArguments[0].Value )
                .Select( a => a.ContainingDeclaration.ToDisplayString() + ":" + a.ConstructorArguments[0].Value )
                .ToArray();

            Assert.Equal(
                new[]
                {
                    "test:1",
                    "test:2",
                    "C<T>:3",
                    "T:4",
                    "C<T>.M(int):5",
                    "C<T>.M(int)@return:6",
                    "int:7",
                    "C<T>.f:8",
                    "C<T>.g:8",
                    "C<T>.P:9",
                    "C<T>.<P>k__BackingField:10",
                    "int:11",
                    "C<T>.ee:13",
                    "C<T>.ff:13"
                },
                targets );
        }
    }
}