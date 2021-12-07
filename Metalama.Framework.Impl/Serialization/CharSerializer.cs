// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CharSerializer : ObjectSerializer<char>
    {
        public override ExpressionSyntax Serialize( char obj, SyntaxSerializationContext serializationContext )
        {
            return LiteralExpression( SyntaxKind.CharacterLiteralExpression, Literal( obj ) );
        }

        public CharSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}