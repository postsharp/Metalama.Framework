// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Impl.Serialization
{
    internal class BoolSerializer : ObjectSerializer<bool>
    {
        public override ExpressionSyntax Serialize( bool obj, SyntaxSerializationContext serializationContext )
        {
            return LiteralExpression( obj ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression );
        }

        public BoolSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}