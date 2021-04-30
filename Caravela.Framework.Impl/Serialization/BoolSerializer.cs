// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class BoolSerializer : TypedObjectSerializer<bool>
    {
        public override ExpressionSyntax Serialize( bool o, ISyntaxFactory syntaxFactory )
        {
            return LiteralExpression( o ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression );
        }

        public BoolSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}