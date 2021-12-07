// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Impl.Serialization
{
    internal class UShortSerializer : ObjectSerializer<ushort>
    {
        public override ExpressionSyntax Serialize( ushort obj, SyntaxSerializationContext serializationContext )
        {
            return SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( obj ) );
        }

        public UShortSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}