// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// An implementation of <see cref="UserExpression"/> that represents a <see cref="INamedType"/> and allows to access
    /// its static members dynamically.
    /// </summary>
    internal sealed class ThisTypeUserReceiver : UserReceiver
    {
        private readonly INamedType _type;

        public ThisTypeUserReceiver( INamedType type, in AspectReferenceSpecification linkerAnnotation ) : base( linkerAnnotation )
        {
            this._type = type;
        }

        public override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext ) => throw new NotSupportedException();

        public override IType Type => this._type;

        public override TypedExpressionSyntaxImpl CreateMemberAccessExpression( string member )
            => new(
                SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        TemplateExpansionContext.CurrentSyntaxGenerationContext.SyntaxGenerator.Type( this._type.GetSymbol() ),
                        SyntaxFactory.IdentifierName( SyntaxFactory.Identifier( member ) ) )
                    .WithAspectReferenceAnnotation( this.AspectReferenceSpecification ),
                TemplateExpansionContext.CurrentSyntaxGenerationContext );

        public override bool CanBeNull => false;
    }
}