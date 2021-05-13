// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Serialization
{
    internal class SByteSerializer : ObjectSerializer<sbyte>
    {
        public override ExpressionSyntax Serialize( sbyte obj, ISyntaxFactory syntaxFactory )
        {
            return SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( obj ) );
        }

        public SByteSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}