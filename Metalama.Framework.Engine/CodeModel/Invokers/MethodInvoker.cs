// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal sealed class MethodInvoker : Invoker<IMethod>, IMethodInvoker
    {
        public MethodInvoker( IMethod method, InvokerOptions? options = default, object? target = null ) : base( method, options, target ) { }

        public object? Invoke( params object?[]? args )
        {
            args ??= Array.Empty<object>();

            var parametersCount = this.Member.Parameters.Count;

            if ( parametersCount > 0 && this.Member.Parameters[parametersCount - 1].IsParams )
            {
                // The this.Declaration has a 'params' param.
                if ( args.Length < parametersCount - 1 )
                {
                    throw GeneralDiagnosticDescriptors.MemberRequiresAtLeastNArguments.CreateException( (this.Member, parametersCount - 1, args.Length) );
                }
            }
            else if ( args.Length != parametersCount )
            {
                throw GeneralDiagnosticDescriptors.MemberRequiresNArguments.CreateException( (this.Member, parametersCount, args.Length) );
            }

            this.CheckInvocationOptionsAndTarget();

            switch ( this.Member.MethodKind )
            {
                case MethodKind.Default:
                case MethodKind.LocalFunction:
                case MethodKind.ExplicitInterfaceImplementation:
                    return this.InvokeDefaultMethod( args );

                case MethodKind.EventAdd:
                    return ((IEvent) this.Member.DeclaringMember!).With( this.Target, this.Options ).Add( args[0] );

                case MethodKind.EventRaise:
                    return ((IEvent) this.Member.DeclaringMember!).With( this.Target, this.Options ).Raise( args );

                case MethodKind.EventRemove:
                    return ((IEvent) this.Member.DeclaringMember!).With( this.Target, this.Options ).Remove( args[0] );

                case MethodKind.PropertyGet:
                    switch ( this.Member.DeclaringMember )
                    {
                        case IProperty property:
                            return property.With( this.Target, this.Options ).Value;

                        case IIndexer indexer:
                            return indexer.With( this.Target, this.Options ).GetValue( args );

                        default:
                            throw new AssertionFailedException( $"Unexpected declaration for a PropertyGet: '{this.Member.DeclaringMember}'." );
                    }

                case MethodKind.PropertySet:
                    switch ( this.Member.DeclaringMember )
                    {
                        case IProperty property:
                            ((FieldOrPropertyInvoker) property.With( this.Target, this.Options )).SetValue( args[0] );

                            return null;

                        case IIndexer indexer:
                            indexer.With( this.Options ).SetValue( this.Target, args );

                            return null;

                        default:
                            throw new AssertionFailedException( $"Unexpected declaration for a PropertySet: '{this.Member.DeclaringMember}'." );
                    }

                default:
                    throw new NotImplementedException(
                        $"Cannot generate syntax to invoke the this.Declaration '{this.Member}' because this.Declaration kind {this.Member.MethodKind} is not implemented." );
            }
        }

        private object InvokeDefaultMethod( object?[] args )
        {
            SimpleNameSyntax name;

            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var receiverInfo = this.GetReceiverInfo();

            if ( this.Member.IsGeneric )
            {
                name = GenericName(
                    Identifier( this.GetCleanTargetMemberName() ),
                    TypeArgumentList(
                        SeparatedList( this.Member.TypeArguments.SelectAsImmutableArray( t => generationContext.SyntaxGenerator.Type( t.GetSymbol() ) ) ) ) );
            }
            else
            {
                name = IdentifierName( this.GetCleanTargetMemberName() );
            }

            var compilation = this.Member.Compilation;

            var arguments = this.Member.GetArguments(
                this.Member.Parameters,
                TypedExpressionSyntaxImpl.FromValues( args, compilation, generationContext ),
                generationContext );

            if ( this.Member.MethodKind == MethodKind.LocalFunction )
            {
                if ( receiverInfo.Syntax.Kind() != SyntaxKind.NullLiteralExpression )
                {
                    throw GeneralDiagnosticDescriptors.CannotProvideInstanceForLocalFunction.CreateException( this.Member );
                }

                return this.CreateInvocationExpression( receiverInfo.ToReceiverExpressionSyntax(), name, arguments, AspectReferenceTargetKind.Self );
            }
            else
            {
                var receiver = receiverInfo.WithSyntax( this.Member.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, generationContext ) );

                return this.CreateInvocationExpression( receiver, name, arguments, AspectReferenceTargetKind.Self );
            }
        }

        private SyntaxUserExpression CreateInvocationExpression(
            ReceiverExpressionSyntax receiverTypedExpressionSyntax,
            SimpleNameSyntax name,
            ArgumentSyntax[]? arguments,
            AspectReferenceTargetKind targetKind )
        {
            ExpressionSyntax expression;
            IType returnType;

            if ( !receiverTypedExpressionSyntax.RequiresNullConditionalAccessMember )
            {
                returnType = this.Member.ReturnType;

                ExpressionSyntax memberAccessExpression =
                    MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiverTypedExpressionSyntax.Syntax, name );

                // Only create an aspect reference when the declaring type of the invoked declaration is ancestor of the target of the template (or its declaring type).
                if ( GetTargetType()?.Is( this.Member.DeclaringType ) ?? false )
                {
                    memberAccessExpression =
                        memberAccessExpression.WithAspectReferenceAnnotation(
                            receiverTypedExpressionSyntax.AspectReferenceSpecification.WithTargetKind( targetKind ) );
                }

                expression =
                    arguments != null
                        ? InvocationExpression(
                            memberAccessExpression,
                            ArgumentList( SeparatedList( arguments ) ) )
                        : InvocationExpression( memberAccessExpression );
            }
            else
            {
                returnType = this.Member.ReturnType.ToNullableType();

                expression =
                    arguments != null
                        ? ConditionalAccessExpression(
                            receiverTypedExpressionSyntax.Syntax,
                            InvocationExpression(
                                MemberBindingExpression( name ),
                                ArgumentList( SeparatedList( arguments ) ) ) )
                        : ConditionalAccessExpression(
                            receiverTypedExpressionSyntax.Syntax,
                            InvocationExpression( MemberBindingExpression( name ) ) );

                // Only create an aspect reference when the declaring type of the invoked declaration is ancestor of the target of the template (or its declaring type).
                if ( GetTargetType()?.Is( this.Member.DeclaringType ) ?? false )
                {
                    expression = expression.WithAspectReferenceAnnotation(
                        receiverTypedExpressionSyntax.AspectReferenceSpecification.WithTargetKind( targetKind ) );
                }
            }

            return new SyntaxUserExpression( expression, returnType );
        }

        public IMethodInvoker With( InvokerOptions options ) => this.Options == options ? this : new MethodInvoker( this.Member, options );

        public IMethodInvoker With( object? target, InvokerOptions options = default )
            => this.Target == target && this.Options == options ? this : new MethodInvoker( this.Member, options, target );
    }
}