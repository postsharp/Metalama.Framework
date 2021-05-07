// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CultureInfoSerializer : ObjectSerializer<CultureInfo>
    {
        public override ExpressionSyntax Serialize( CultureInfo obj, ISyntaxFactory syntaxFactory )
        {
            return ObjectCreationExpression( syntaxFactory.GetTypeSyntax( typeof( CultureInfo ) ) )
                .AddArgumentListArguments(
                    Argument(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal( obj.Name ) ) ),
                    Argument( LiteralExpression( obj.UseUserOverride ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression ) ) )
                .NormalizeWhitespace();
        }

        public CultureInfoSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}