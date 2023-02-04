// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal class Invoker<T> 
        where T : IMember
    {
        protected readonly InvokerOptions _options;
        private readonly AspectReferenceOrder _order;

        protected SyntaxGenerationContext GenerationContext { get; }

        
        public Invoker( T declaration, InvokerOptions options )
        {
            this._options = options;
            this.Declaration = declaration;
            this.GenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            this._order = (options & InvokerOptions.Before) != 0 ? AspectReferenceOrder.Base : AspectReferenceOrder.Final;
        }

        public T Declaration { get; }

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

        private AspectReferenceSpecification GetDefaultAspectReferenceSpecification()
            => new( TemplateExpansionContext.CurrentAspectLayerId.AssertNotNull(), this._order );

        protected ReceiverTypedExpressionSyntax GetReceiverInfo( IMember member, object? target )
        {
            if ( target is UserReceiver receiver )
            {
                if ( this._order == AspectReferenceOrder.Base )
                {
                    // Replace 'this' with 'base'.
                    receiver = receiver.WithAspectReferenceOrder( AspectReferenceOrder.Base );
                }

                return new ReceiverTypedExpressionSyntax( receiver.ToTypedExpressionSyntax( this.GenerationContext ), false,  receiver.AspectReferenceSpecification );
            }
            else
            {
                var aspectReferenceSpecification = this.GetDefaultAspectReferenceSpecification();

                if ( target != null )
                {
                    var typedExpressionSyntax = TypedExpressionSyntaxImpl.FromValue( target, member.Compilation, this.GenerationContext );

                    return new ReceiverTypedExpressionSyntax(
                        typedExpressionSyntax,
                        (this._options & InvokerOptions.NullConditional) != 0,
                        aspectReferenceSpecification );
                }
                else if ( member.IsStatic )
                {
                    return new ReceiverTypedExpressionSyntax(
                        new ThisTypeUserReceiver( member.DeclaringType, aspectReferenceSpecification ).ToTypedExpressionSyntax( this.GenerationContext ),
                        false,
                        aspectReferenceSpecification );
                }
                else
                {
                       throw GeneralDiagnosticDescriptors.MustProvideInstanceForInstanceMember.CreateException( this.Declaration );
                 
                }
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