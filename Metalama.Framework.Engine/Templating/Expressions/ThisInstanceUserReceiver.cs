﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.SyntaxSerialization;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// An implementation of <see cref="UserExpression"/> that represents <c>this</c> and allows to access its instance members dynamically.
    /// </summary>
    internal sealed class ThisInstanceUserReceiver : UserReceiver
    {
        private readonly INamedType _type;

        public ThisInstanceUserReceiver( INamedType type, in AspectReferenceSpecification aspectReferenceSpecification ) : base( aspectReferenceSpecification )
        {
            this._type = type;
        }

        protected override ExpressionSyntax ToSyntax( SyntaxSerializationContext syntaxSerializationContext ) => ThisExpression();

        public override bool IsAssignable => this._type.TypeKind == TypeKind.Struct;

        public override IType Type => this._type;

        public override TypedExpressionSyntaxImpl CreateMemberAccessExpression( string member )
            => new(
                MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( Identifier( member ) ) )
                    .WithAspectReferenceAnnotation( this.AspectReferenceSpecification ),
                this._type,
                TemplateExpansionContext.CurrentSyntaxGenerationContext,
                canBeNull: false );

        protected override UserReceiver WithAspectReferenceSpecification( AspectReferenceSpecification spec )
            => new ThisInstanceUserReceiver( this._type, spec );

        protected override bool CanBeNull => false;
    }
}