// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
    internal class ThisTypeUserReceiver : UserReceiver
    {
        private readonly INamedType _type;
        private readonly AspectReferenceSpecification _linkerAnnotation;

        public ThisTypeUserReceiver( INamedType type, AspectReferenceSpecification linkerAnnotation )
        {
            this._type = type;
            this._linkerAnnotation = linkerAnnotation;
        }

        public override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext ) => throw new NotSupportedException();

        public override RunTimeTemplateExpression ToRunTimeTemplateExpression( SyntaxGenerationContext syntaxGenerationContext )
            => throw new NotSupportedException();

        public override IType Type => this._type;

        public override RunTimeTemplateExpression CreateMemberAccessExpression( string member )
            => new(
                SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        TemplateExpansionContext.CurrentSyntaxGenerationContext.SyntaxGenerator.Type( this._type.GetSymbol() ),
                        SyntaxFactory.IdentifierName( SyntaxFactory.Identifier( member ) ) )
                    .WithAspectReferenceAnnotation( this._linkerAnnotation ),
                TemplateExpansionContext.CurrentSyntaxGenerationContext );
    }
}