// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.UnitTests
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
        public void TypeOfSyntax( string type, string expectedTypeOf, bool nullable )
        {
            var code = $"using System.Collections.Generic; class T {{ {type} field; }} ";
            var compilation = CreateCompilationModel( code );
            var fieldType = compilation.DeclaredTypes.Single().Fields.Single().Type.GetSymbol();

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
            var code = $"using System.Collections.Generic; class T {{ {type} field; }} ";
            var compilation = CreateCompilationModel( code );
            var fieldType = compilation.DeclaredTypes.Single().Fields.Single().Type.GetSymbol();

            var defaultSyntaxGenerator = OurSyntaxGenerator.GetInstance( nullable );

            var typeOf = defaultSyntaxGenerator.Type( fieldType ).ToString();

            this._logger.WriteLine( "Actual: " + typeOf );

            Assert.Equal( expectedTypeOf, typeOf );
        }
    }
}