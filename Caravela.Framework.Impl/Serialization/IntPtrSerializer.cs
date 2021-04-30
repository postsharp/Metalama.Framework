// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Serialization
{
    internal class IntPtrSerializer : TypedObjectSerializer<IntPtr>
    {
        public override ExpressionSyntax Serialize( IntPtr o, ISyntaxFactory syntaxFactory )
        {
            return SyntaxFactory.ObjectCreationExpression( syntaxFactory.GetTypeSyntax( typeof(IntPtr) ) )
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal( o.ToInt64() ) ) ) );
        }

        public IntPtrSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}