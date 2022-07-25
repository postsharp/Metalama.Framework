// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// An implementation of <see cref="UserExpression"/> that represents <c>this</c> and allows to access its instance members dynamically.
    /// </summary>
    internal class ThisInstanceUserReceiver : UserReceiver
    {
        private readonly INamedType _type;
        private readonly AspectReferenceSpecification _linkerAnnotation;

        public ThisInstanceUserReceiver( INamedType type, AspectReferenceSpecification linkerAnnotation )
        {
            this._type = type;
            this._linkerAnnotation = linkerAnnotation;
        }

        protected override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext ) => ThisExpression();

        public override bool IsAssignable => this._type.TypeKind == TypeKind.Struct;

        public override IType Type => this._type;

        public override TypedExpressionSyntax CreateMemberAccessExpression( string member )
            => new(
                MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( Identifier( member ) ) )
                    .WithAspectReferenceAnnotation( this._linkerAnnotation ),
                this._type,
                TemplateExpansionContext.CurrentSyntaxGenerationContext );

        // TODO: Add linker annotations.
    }
}