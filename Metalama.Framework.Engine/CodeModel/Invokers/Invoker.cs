// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal abstract class Invoker<T>
        where T : IMember
    {
        private readonly AspectReferenceOrder _order;

        protected InvokerOptions Options { get; }

        protected object? Target { get; }

        protected SyntaxGenerationContext GenerationContext { get; }

        protected Invoker( T member, InvokerOptions? options, object? target, SyntaxGenerationContext? syntaxGenerationContext = null )
        {
            options ??= InvokerOptions.Default;
            
            var isSelfTarget = target is null or ThisInstanceUserReceiver or ThisTypeUserReceiver;

            var orderOptions = GetOrderOptions( member, options.Value, isSelfTarget );

            if ( orderOptions == InvokerOptions.Base && !isSelfTarget )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(target),
                    "Cannot provide a target other than 'this', 'base' or the current type when specifying the InvokerOptions.Base option." );
            }

            var otherFlags = options.Value & ~InvokerOptions.OrderMask;

            this.Options = orderOptions | otherFlags;

            this.Target = target;
            this.Member = member;
            
            // Get the SyntaxGenerationContext. We fall back to the DefaultSyntaxGenerationContext because it is easy and because invokers can be called from
            // a non-template context e.g. a unit test or LinqPad.
            this.GenerationContext = syntaxGenerationContext ?? TemplateExpansionContext.CurrentSyntaxGenerationContextOrNull ?? member.GetCompilationModel().CompilationContext.DefaultSyntaxGenerationContext;

            this._order = orderOptions switch
            {
                InvokerOptions.Current => AspectReferenceOrder.Self,
                InvokerOptions.Base => AspectReferenceOrder.Base,
                InvokerOptions.Final => AspectReferenceOrder.Final,
                _ => throw new AssertionFailedException( $"Invalid value: {this.Options}." )
            };
        }

        private static InvokerOptions GetOrderOptions( IMember member, InvokerOptions options, bool isSelfTarget )
        {
            options &= InvokerOptions.OrderMask;

            if ( options != InvokerOptions.Default )
            {
                return options;
            }
            else if ( isSelfTarget && TemplateExpansionContext.IsTransformingDeclaration( member ) )
            {
                // When we expand a template, the default invoker for the declaration being overridden or introduced is
                // always the base one. Otherwise, if we keep the Current default option, the default behavior would be
                // to generate infinite recursions.
                
                return InvokerOptions.Base;
            }
            else
            {
                return InvokerOptions.Final;
            }
        }

        protected T Member { get; }

        protected readonly record struct ReceiverTypedExpressionSyntax(
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

        protected readonly record struct ReceiverExpressionSyntax(
            ExpressionSyntax Syntax,
            bool RequiresNullConditionalAccessMember,
            AspectReferenceSpecification AspectReferenceSpecification );

        private AspectReferenceSpecification GetDefaultAspectReferenceSpecification()
        // CurrentAspectLayerId may be null when we are not executing in a template execution context.
            => new( TemplateExpansionContext.CurrentAspectLayerId ?? default, this._order );

        protected ReceiverTypedExpressionSyntax GetReceiverInfo()
        {
            if ( this.Target is UserReceiver receiver )
            {
                if ( this._order == AspectReferenceOrder.Base )
                {
                    // Replace 'this' with 'base'.
                    receiver = receiver.WithAspectReferenceOrder( AspectReferenceOrder.Base );
                }

                return new ReceiverTypedExpressionSyntax(
                    receiver.ToTypedExpressionSyntax( this.GenerationContext ),
                    false,
                    receiver.AspectReferenceSpecification );
            }
            else
            {
                var aspectReferenceSpecification = this.GetDefaultAspectReferenceSpecification();

                if ( this.Target != null )
                {
                    var typedExpressionSyntax = TypedExpressionSyntaxImpl.FromValue( this.Target, this.Member.Compilation, this.GenerationContext );

                    return new ReceiverTypedExpressionSyntax(
                        typedExpressionSyntax,
                        (this.Options & InvokerOptions.NullConditional) != 0 && typedExpressionSyntax.CanBeNull,
                        aspectReferenceSpecification );
                }
                else if ( this.Member.IsStatic )
                {
                    return new ReceiverTypedExpressionSyntax(
                        new ThisTypeUserReceiver( this.Member.DeclaringType, aspectReferenceSpecification ).ToTypedExpressionSyntax( this.GenerationContext ),
                        false,
                        aspectReferenceSpecification );
                }
                else
                {
                    return new ReceiverTypedExpressionSyntax(
                        new ThisInstanceUserReceiver( this.Member.DeclaringType, aspectReferenceSpecification ).ToTypedExpressionSyntax(
                            this.GenerationContext ),
                        false,
                        aspectReferenceSpecification );
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