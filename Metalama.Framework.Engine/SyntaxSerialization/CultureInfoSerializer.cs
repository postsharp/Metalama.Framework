// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal sealed class CultureInfoSerializer : ObjectSerializer<CultureInfo>
    {
        public override ExpressionSyntax Serialize( CultureInfo obj, SyntaxSerializationContext serializationContext )
        {
            return ObjectCreationExpression(
                serializationContext.GetTypeSyntax( typeof(CultureInfo) ),
                ArgumentList(
                    SeparatedList(
                        new[]
                        {
                            Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( obj.Name ) ) ),
                            Argument( LiteralExpression( obj.UseUserOverride ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression ) )
                        } ) ),
                null );
        }

        public CultureInfoSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}