// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal abstract class Invoker<T>
        where T : IMember
    {
        private readonly AspectReferenceOrder _order;

        protected InvokerOptions Options { get; }

        protected object? Target { get; }

        protected SyntaxGenerationContext GenerationContext { get; }

        protected SyntaxSerializationContext SerializationContext { get; }

        protected Invoker( T member, InvokerOptions? options, object? target, SyntaxGenerationContext? syntaxGenerationContext = null )
        {
            options ??= InvokerOptions.Default;

            var isSelfTarget = target is null or ThisInstanceUserReceiver or ThisTypeUserReceiver;

            var orderOptions = GetOrderOptions( member, options.Value, isSelfTarget );

            var otherFlags = options.Value & ~InvokerOptions.OrderMask;

            this.Options = orderOptions | otherFlags;

            this.Target = target;
            this.Member = member;

            // Get the SyntaxGenerationContext. We fall back to the DefaultSyntaxGenerationContext because it is easy and because invokers can be called from
            // a non-template context e.g. a unit test or LinqPad.
            this.GenerationContext = syntaxGenerationContext ?? TemplateExpansionContext.CurrentSyntaxGenerationContextOrNull
                ?? member.GetCompilationModel().CompilationContext.DefaultSyntaxGenerationContext;

            this.SerializationContext = new SyntaxSerializationContext( member.GetCompilationModel(), this.GenerationContext );

            this._order = orderOptions switch
            {
                InvokerOptions.Current => AspectReferenceOrder.Current,
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
                // always the base one.

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
            => new(
                null,
                TemplateExpansionContext.CurrentAspectLayerId ?? default,
                this._order,
                flags: this.Target == null ? AspectReferenceFlags.None : AspectReferenceFlags.CustomReceiver );

        protected string GetCleanTargetMemberName()
        {
            var definition = this.Member.Definition;

            return
                definition.IsExplicitInterfaceImplementation
                    ? definition.GetExplicitInterfaceImplementation().Name
                    : definition.Name;
        }

        protected ReceiverTypedExpressionSyntax GetReceiverInfo()
        {
            if ( this.Target is UserReceiver receiver )
            {
                receiver = receiver.WithAspectReferenceOrder( this._order );

                return new ReceiverTypedExpressionSyntax(
                    receiver.ToTypedExpressionSyntax( this.SerializationContext ),
                    false,
                    receiver.AspectReferenceSpecification );
            }
            else
            {
                var aspectReferenceSpecification = this.GetDefaultAspectReferenceSpecification();

                if ( this.Target != null )
                {
                    var typedExpressionSyntax = TypedExpressionSyntaxImpl.FromValue( this.Target, this.SerializationContext );

                    return new ReceiverTypedExpressionSyntax(
                        typedExpressionSyntax,
                        (this.Options & InvokerOptions.NullConditional) != 0 && typedExpressionSyntax.CanBeNull,
                        aspectReferenceSpecification );
                }
                else if ( this.Member.IsStatic )
                {
                    return new ReceiverTypedExpressionSyntax(
                        new ThisTypeUserReceiver( this.Member.DeclaringType, aspectReferenceSpecification )
                            .ToTypedExpressionSyntax( this.SerializationContext ),
                        false,
                        aspectReferenceSpecification );
                }
                else
                {
                    return new ReceiverTypedExpressionSyntax(
                        new ThisInstanceUserReceiver( this.Member.DeclaringType, aspectReferenceSpecification ).ToTypedExpressionSyntax(
                            this.SerializationContext ),
                        false,
                        aspectReferenceSpecification );
                }
            }
        }

        protected static INamedType? GetTargetType()
        {
            return TemplateExpansionContext.CurrentTargetDeclaration switch
            {
                INamedType type => type,
                IMember member => member.DeclaringType,
                IParameter parameter => parameter.DeclaringMember.DeclaringType,
                null => null,
                _ => throw new AssertionFailedException( $"Unexpected target declaration: '{TemplateExpansionContext.CurrentTargetDeclaration}'." )
            };
        }

        protected void CheckInvocationOptionsAndTarget()
        {
            // Specifying Base or Current option with non-default target is only allowed when the method is in the inheritance hierarchy of the template target.
            if ( this.Target != null && (this.Options & InvokerOptions.OrderMask) is InvokerOptions.Base or InvokerOptions.Current &&
                 !(GetTargetType()?.Is( this.Member.DeclaringType ) ?? false) )
            {
                throw GeneralDiagnosticDescriptors.CantInvokeBaseOrCurrentOutsideTargetType.CreateException(
                    (this.Member, GetTargetType()!, this.Options & InvokerOptions.OrderMask) );
            }
        }
    }
}