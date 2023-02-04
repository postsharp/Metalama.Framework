// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal partial class RunTimeInvocationApi
    {
        private readonly SyntaxGenerationContext _generationContext;

        public RunTimeInvocationApi( SyntaxGenerationContext syntaxGenerationContext )
        {
            this._generationContext = syntaxGenerationContext;
        }

        protected record struct ReceiverTypedExpressionSyntax(
            TypedExpressionSyntaxImpl TypedExpressionSyntax,
            bool RequiresConditionalAccess,
            AspectReferenceSpecification AspectReferenceSpecification )
        {
            public ExpressionSyntax Syntax => this.TypedExpressionSyntax.Syntax;

            public ReceiverExpressionSyntax WithSyntax( ExpressionSyntax syntax )
                => new( syntax, this.RequiresConditionalAccess, this.AspectReferenceSpecification );

            public ReceiverExpressionSyntax ToReceiverExpressionSyntax()
                => new( this.Syntax, this.RequiresConditionalAccess, this.AspectReferenceSpecification );
        }

        protected record struct ReceiverExpressionSyntax(
            ExpressionSyntax Syntax,
            bool RequiresNullConditionalAccessMember,
            AspectReferenceSpecification AspectReferenceSpecification ) { }

        private static AspectReferenceSpecification GetDefaultAspectReferenceSpecification()
            => new( TemplateExpansionContext.CurrentAspectLayerId.AssertNotNull(), AspectReferenceOrder.Final );

        protected ReceiverTypedExpressionSyntax GetReceiverInfo( IMember member, object? target )
        {
            var aspectReferenceSpecification = (target as UserReceiver)?.AspectReferenceSpecification ?? GetDefaultAspectReferenceSpecification();

            if ( target != null )
            {
                return new ReceiverTypedExpressionSyntax(
                    TypedExpressionSyntaxImpl.FromValue( target, member.Compilation, this._generationContext ),
                    target is NullConditionalUserExpression,
                    aspectReferenceSpecification );
            }
            else if ( member.IsStatic )
            {
                return new ReceiverTypedExpressionSyntax(
                    new ThisTypeUserReceiver( member.DeclaringType, aspectReferenceSpecification ).ToTypedExpressionSyntax( this._generationContext ),
                    false,
                    aspectReferenceSpecification );
            }
            else
            {
                return new ReceiverTypedExpressionSyntax(
                    new ThisInstanceUserReceiver( member.DeclaringType, aspectReferenceSpecification ).ToTypedExpressionSyntax( this._generationContext ),
                    false,
                    aspectReferenceSpecification );
            }
        }

        protected static INamedTypeSymbol? GetTargetTypeSymbol()
        {
            return TemplateExpansionContext.CurrentTargetDeclaration switch
            {
                INamedType type => type.GetSymbol().OriginalDefinition,
                IMember member => member.DeclaringType.GetSymbol().OriginalDefinition,
                null => null,
                _ => throw new AssertionFailedException( $"Unexpected target declaration: '{TemplateExpansionContext.CurrentTargetDeclaration}'." )
            };
        }
    }
}