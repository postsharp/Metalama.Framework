// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public class SyntaxGeneratorTests : TestBase
    {
        private readonly ITestOutputHelper _logger;

        public SyntaxGeneratorTests( ITestOutputHelper logger )
        {
            this._logger = logger;
        }

        [Theory]

        // With nullable context.
        [InlineData( "int?", "typeof(global::System.Int32?)", true )]
        [InlineData( "string?", "typeof(global::System.String)", true )]
        [InlineData( "List<string?>", "typeof(global::System.Collections.Generic.List<global::System.String>)", true )]
        [InlineData( "List<string>?", "typeof(global::System.Collections.Generic.List<global::System.String>)", true )]
        [InlineData(
            "List<List<string?>>",
            "typeof(global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.String>>)",
            true )]
        [InlineData( "List<string[]?>", "typeof(global::System.Collections.Generic.List<global::System.String[]>)", true )]
        [InlineData( "List<string?[]?>?", "typeof(global::System.Collections.Generic.List<global::System.String[]>)", true )]
        [InlineData( "List<int[]?>", "typeof(global::System.Collections.Generic.List<global::System.Int32[]>)", true )]
        [InlineData( "List<int?[]>", "typeof(global::System.Collections.Generic.List<global::System.Int32?[]>)", true )]

        // Without nullable context.
        [InlineData( "int?", "typeof(global::System.Int32?)", false )]
        [InlineData( "string?", "typeof(global::System.String)", false )]
        [InlineData( "List<string?>", "typeof(global::System.Collections.Generic.List<global::System.String>)", false )]
        [InlineData( "List<string>?", "typeof(global::System.Collections.Generic.List<global::System.String>)", false )]
        [InlineData(
            "List<List<string?>>",
            "typeof(global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.String>>)",
            false )]
        [InlineData( "List<string[]?>", "typeof(global::System.Collections.Generic.List<global::System.String[]>)", false )]
        [InlineData( "List<string?[]?>?", "typeof(global::System.Collections.Generic.List<global::System.String[]>)", false )]
        [InlineData( "List<int[]?>", "typeof(global::System.Collections.Generic.List<global::System.Int32[]>)", false )]
        [InlineData( "List<int?[]>", "typeof(global::System.Collections.Generic.List<global::System.Int32?[]>)", false )]
        [InlineData(
            "global::System.Collections.Generic.List<global::System.String[]?>",
            "typeof(global::System.Collections.Generic.List<global::System.String[]>)",
            false )]
        [InlineData(
            "global::System.Collections.Generic.List<global::System.String?[]?>?",
            "typeof(global::System.Collections.Generic.List<global::System.String[]>)",
            false )]
        [InlineData(
            "global::System.Collections.Generic.List<global::System.Int32[]?>",
            "typeof(global::System.Collections.Generic.List<global::System.Int32[]>)",
            false )]
        [InlineData(
            "global::System.Collections.Generic.List<global::System.Int32?[]>",
            "typeof(global::System.Collections.Generic.List<global::System.Int32?[]>)",
            false )]
        public void TypeOfSyntax( string type, string expectedTypeOf, bool nullable )
        {
            using var testContext = this.CreateTestContext();

            var code = $"using System.Collections.Generic; class T {{ {type} field; }} ";
            var compilation = testContext.CreateCompilationModel( code );
            var fieldType = compilation.Types.Single().Fields.Single().Type.GetSymbol();

            var defaultSyntaxGenerator = OurSyntaxGenerator.GetInstance( nullable );

            var typeOf = defaultSyntaxGenerator.TypeOfExpression( fieldType ).ToString();

            this._logger.WriteLine( "Actual: " + typeOf );

            Assert.Equal( expectedTypeOf, typeOf );
        }

        [Theory]

        // With nullable context.
        [InlineData( "int?", "global::System.Int32?", true )]
        [InlineData( "string?", "global::System.String?", true )]
        [InlineData( "List<string?>", "global::System.Collections.Generic.List<global::System.String?>", true )]
        [InlineData( "List<string>?", "global::System.Collections.Generic.List<global::System.String>?", true )]
        [InlineData(
            "List<List<string?>>",
            "global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.String?>>",
            true )]
        [InlineData( "List<string[]?>", "global::System.Collections.Generic.List<global::System.String[]?>", true )]
        [InlineData( "List<string?[]?>?", "global::System.Collections.Generic.List<global::System.String?[]?>?", true )]
        [InlineData( "List<int[]?>", "global::System.Collections.Generic.List<global::System.Int32[]?>", true )]
        [InlineData( "List<int?[]>", "global::System.Collections.Generic.List<global::System.Int32?[]>", true )]

        // Without nullable context.
        [InlineData( "int?", "global::System.Int32?", false )]
        [InlineData( "string?", "global::System.String", false )]
        [InlineData( "List<string?>", "global::System.Collections.Generic.List<global::System.String>", false )]
        [InlineData( "List<string>?", "global::System.Collections.Generic.List<global::System.String>", false )]
        [InlineData(
            "List<List<string?>>",
            "global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.String>>",
            false )]
        [InlineData( "List<string[]?>", "global::System.Collections.Generic.List<global::System.String[]>", false )]
        [InlineData( "List<string?[]?>?", "global::System.Collections.Generic.List<global::System.String[]>", false )]
        [InlineData( "List<int[]?>", "global::System.Collections.Generic.List<global::System.Int32[]>", false )]
        [InlineData( "List<int?[]>", "global::System.Collections.Generic.List<global::System.Int32?[]>", false )]
        public void TypeSyntax( string type, string expectedTypeOf, bool nullable )
        {
            using var testContext = this.CreateTestContext();

            var code = $"using System.Collections.Generic; class T {{ {type} field; }} ";
            var compilation = testContext.CreateCompilationModel( code );
            var fieldType = compilation.Types.Single().Fields.Single().Type.GetSymbol();

            var defaultSyntaxGenerator = OurSyntaxGenerator.GetInstance( nullable );

            var typeOf = defaultSyntaxGenerator.Type( fieldType ).ToString();

            this._logger.WriteLine( "Actual: " + typeOf );

            Assert.Equal( expectedTypeOf, typeOf );
        }

        [Theory]
        [InlineData( "0", "0" )]
        [InlineData( "null", "default(global::System.Object)" )]
        [InlineData( "typeof(string)", "typeof(global::System.String)" )]
        [InlineData( "DayOfWeek.Monday", "global::System.DayOfWeek.Monday" )]
        [InlineData( "new[] { 0 }", "new global::System.Int32[]{0}" )]
        [InlineData( "new[] { (string?) null }", "new global::System.String[]{null}" )]
        [InlineData( "new[] { typeof(string) }", "new global::System.Type[]{typeof(global::System.String)}" )]
        [InlineData( "new[] { DayOfWeek.Monday }", "new global::System.DayOfWeek[]{global::System.DayOfWeek.Monday}" )]
        public void AttributeValue( string inputSyntax, string expectedOutputSyntax )
        {
            using var testContext = this.CreateTestContext();

            var code =
                $"using System; class MyAttribute : Attribute {{ public MyAttribute( object value ) {{}} }} [MyAttribute( {inputSyntax} )] class C {{}} ";

            var compilation = testContext.CreateCompilationModel( code );
            var syntaxGenerationContext = SyntaxGenerationContext.Create( testContext.ServiceProvider, compilation.RoslynCompilation );
            var syntaxGenerator = new SyntaxGeneratorWithContext( OurSyntaxGenerator.Default, syntaxGenerationContext );
            var type = compilation.Types.OfName( "C" ).Single();
            var attribute = type.Attributes.Single();
            var codeModelOutput = syntaxGenerator.Attribute( attribute ).ArgumentList!.Arguments[0].NormalizeWhitespace().ToFullString();
            Assert.Equal( expectedOutputSyntax, codeModelOutput );
        }
    }
}