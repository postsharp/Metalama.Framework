// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
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

        private void AssertType( string type, string expectedTypeOf )
        {
            var code = $"using System.Collections.Generic; class T {{ {type} field; }} ";
            var compilation = CreateCompilationModel( code );
            var fieldType = compilation.DeclaredTypes.Single().Fields.Single().Type.GetSymbol();

            var typeOf = LanguageServiceFactory.CSharpSyntaxGenerator.TypeOfExpression( fieldType ).ToString();

            this._logger.WriteLine( "Actual: " + typeOf );
            Assert.Equal( expectedTypeOf, typeOf );
        }

        [Fact]
        public void TypeOfNullable()
        {
            this.AssertType( "int?", "typeof(global::System.Int32?)" );
            this.AssertType( "string?", "typeof(global::System.String)" );
            this.AssertType( "List<string?>", "typeof(global::System.Collections.Generic.List<global::System.String>)" );

            this.AssertType(
                "List<List<string?>>",
                "typeof(global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.String>>)" );

            this.AssertType( "List<string[]?>", "typeof(global::System.Collections.Generic.List<global::System.String[]>)" );
            this.AssertType( "List<int[]?>", "typeof(global::System.Collections.Generic.List<global::System.Int32[]>)" );
            this.AssertType( "List<int?[]>", "typeof(global::System.Collections.Generic.List<global::System.Int32?[]>)" );
        }
    }
}