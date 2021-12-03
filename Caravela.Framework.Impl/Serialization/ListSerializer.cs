// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class ListSerializer : ObjectSerializer
    {
        public override ExpressionSyntax Serialize( object obj, SyntaxSerializationContext serializationContext )
        {
            var serializedItems = new List<ExpressionSyntax>();

            foreach ( var item in (IEnumerable) obj )
            {
                ThrowIfStackTooDeep( item );
                serializedItems.Add( this.Service.Serialize( item, serializationContext ) );
            }

            return ObjectCreationExpression( serializationContext.GetTypeSyntax( obj.GetType() ) )
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.CollectionInitializerExpression,
                        SeparatedList( serializedItems ) ) )
                .NormalizeWhitespace();
        }

        public override Type InputType => typeof( IEnumerable<> );

        public override Type? OutputType => typeof( List<> );

        public override int Priority => 1;

        public ListSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray.Create( typeof( IReadOnlyList<> ), typeof( IReadOnlyCollection<> ) );
    }
}