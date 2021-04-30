// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Serialization
{
    internal class UIntPtrSerializer : TypedObjectSerializer<UIntPtr>
    {
        public override ExpressionSyntax Serialize( UIntPtr o, ISyntaxFactory syntaxFactory )
        {
            return SyntaxFactory.ObjectCreationExpression( syntaxFactory.GetTypeSyntax( typeof(UIntPtr) ) )
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal( o.ToUInt64() ) ) ) );
        }

        public UIntPtrSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}