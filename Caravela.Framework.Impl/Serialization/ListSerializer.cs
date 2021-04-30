// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class ListSerializer : ObjectSerializer
    {
        public override ExpressionSyntax Serialize( object obj, ISyntaxFactory syntaxFactory )
        {
            var serializedItems = new List<ExpressionSyntax>();

            foreach ( var item in (IEnumerable) obj )
            {
                ThrowIfStackTooDeep( item );
                serializedItems.Add( this.Service.Serialize( item, syntaxFactory ) );
            }

            return ObjectCreationExpression( syntaxFactory.GetTypeSyntax( obj.GetType() ) )
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.CollectionInitializerExpression,
                        SeparatedList( serializedItems ) ) )
                .NormalizeWhitespace();
        }

        public ListSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}