// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Serialization
{
    internal class ShortSerializer : ObjectSerializer<short>
    {
        public override ExpressionSyntax Serialize( short obj, SyntaxSerializationContext serializationContext )
        {
            return SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( obj ) );
        }

        public ShortSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}