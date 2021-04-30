// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Serialization
{
    internal class StringSerializer : ObjectSerializer<string>
    {
        public override ExpressionSyntax Serialize( string obj, ISyntaxFactory syntaxFactory )
        {
            return SyntaxFactory.LiteralExpression( SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal( obj ) );
        }

        public StringSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}