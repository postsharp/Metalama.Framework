// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CharSerializer : TypedObjectSerializer<char>
    {
        public override ExpressionSyntax Serialize( char o, ISyntaxFactory syntaxFactory )
        {
            return LiteralExpression( SyntaxKind.CharacterLiteralExpression, Literal( o ) );
        }

        public CharSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}