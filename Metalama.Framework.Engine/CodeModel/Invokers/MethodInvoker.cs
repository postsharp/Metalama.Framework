// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal class MethodInvoker : Invoker, IMethodInvoker
    {
        private readonly IMethod _method;
        private readonly InvokerOperator _invokerOperator;

        public MethodInvoker( IMethod method, InvokerOrder order, InvokerOperator invokerOperator ) : base( method, order )
        {
            this._method = method;
            this._invokerOperator = invokerOperator;
        }

        public object? Invoke( object? instance, params object?[] args )
        {
            if ( this._method.IsOpenGeneric )
            {
                throw new InvalidOperationException(
                    $"Cannot invoke the '{this._method.ToDisplayString()}' method because the method or its declaring type has unbound type parameters." );
            }

            var parametersCount = this._method.Parameters.Count;

            if ( parametersCount > 0 && this._method.Parameters[parametersCount - 1].IsParams )
            {
                // The method has a 'params' param.
                if ( args.Length < parametersCount - 1 )
                {
                    throw GeneralDiagnosticDescriptors.MemberRequiresAtLeastNArguments.CreateException( (this._method, parametersCount - 1, args.Length) );
                }
            }
            else if ( args.Length != parametersCount )
            {
                throw GeneralDiagnosticDescriptors.MemberRequiresNArguments.CreateException( (this._method, parametersCount, args.Length) );
            }

            switch ( this._method.MethodKind )
            {
                case MethodKind.Default:
                case MethodKind.LocalFunction:
                    return this.InvokeDefaultMethod( instance, args );

                case MethodKind.EventAdd:
                    return ((IEvent) this._method.DeclaringMember!).Invokers.GetInvoker( this.Order, this._invokerOperator )!.Add( instance, args[0] );

                case MethodKind.EventRaise:
                    return ((IEvent) this._method.DeclaringMember!).Invokers.GetInvoker( this.Order, this._invokerOperator )!.Raise( instance, args );

                case MethodKind.EventRemove:
                    return ((IEvent) this._method.DeclaringMember!).Invokers.GetInvoker( this.Order, this._invokerOperator )!.Remove( instance, args[0] );

                case MethodKind.PropertyGet:
                    return ((IProperty) this._method.DeclaringMember!).Invokers.GetInvoker( this.Order, this._invokerOperator )!.GetValue( instance );

                case MethodKind.PropertySet:
                    return ((IProperty) this._method.DeclaringMember!).Invokers.GetInvoker( this.Order, this._invokerOperator )!.SetValue( instance, args[0] );

                default:
                    throw new NotImplementedException(
                        $"Cannot generate syntax to invoke the method '{this._method}' because method kind {this._method.MethodKind} is not implemented." );
            }
        }

        private object InvokeDefaultMethod( object? instance, object?[] args )
        {
            SimpleNameSyntax name;

            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            if ( this._method.IsGeneric )
            {
                name = GenericName(
                    Identifier( this._method.Name ),
                    TypeArgumentList(
                        SeparatedList(
                            this._method.TypeArguments.Select( t => generationContext.SyntaxGenerator.Type( t.GetSymbol() ) )
                                .ToArray() ) ) );
            }
            else
            {
                name = IdentifierName( this._method.Name );
            }

            var arguments = this._method.GetArguments(
                this._method.Parameters,
                RunTimeTemplateExpression.FromValue( args, this.Compilation, generationContext ) );

            if ( this._method.MethodKind == MethodKind.LocalFunction )
            {
                var instanceExpression = RunTimeTemplateExpression.FromValue( instance, this.Compilation, generationContext );

                if ( instanceExpression.Syntax.Kind() != SyntaxKind.NullLiteralExpression )
                {
                    throw GeneralDiagnosticDescriptors.CannotProvideInstanceForLocalFunction.CreateException( this._method );
                }

                return this.CreateInvocationExpression( null, name, arguments, AspectReferenceTargetKind.Self );
            }
            else
            {
                var instanceExpression =
                    this._method.GetReceiverSyntax(
                        RunTimeTemplateExpression.FromValue( instance!, this.Compilation, generationContext ),
                        generationContext );

                return this.CreateInvocationExpression( instanceExpression, name, arguments, AspectReferenceTargetKind.Self );
            }
        }

        private UserExpression CreateInvocationExpression(
            ExpressionSyntax? instanceExpression,
            SimpleNameSyntax name,
            ArgumentSyntax[]? arguments,
            AspectReferenceTargetKind targetKind )
        {
            if ( this._method.DeclaringType.IsOpenGeneric )
            {
                throw new InvalidOperationException(
                    $"Cannot invoke the '{this._method.ToDisplayString()}' method because the declaring type has unbound type parameters." );
            }

            ExpressionSyntax expression;
            IType returnType;

            if ( this._invokerOperator == InvokerOperator.Default )
            {
                returnType = this._method.ReturnType;

                ExpressionSyntax receiverExpression =
                    instanceExpression != null
                        ? MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, instanceExpression, name )
                        : name;

                // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
                if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), this._method.DeclaringType.GetSymbol().OriginalDefinition ) )
                {
                    receiverExpression = receiverExpression.WithAspectReferenceAnnotation( this.AspectReference.WithTargetKind( targetKind ) );
                }

                expression =
                    arguments != null
                        ? InvocationExpression(
                            receiverExpression,
                            ArgumentList( SeparatedList( arguments ) ) )
                        : InvocationExpression( receiverExpression );
            }
            else
            {
                returnType = this._method.ReturnType.ConstructNullable();

                if ( instanceExpression == null )
                {
                    throw new AssertionFailedException();
                }

                expression =
                    arguments != null
                        ? ConditionalAccessExpression(
                            instanceExpression,
                            InvocationExpression(
                                MemberBindingExpression( name ),
                                ArgumentList( SeparatedList( arguments ) ) ) )
                        : ConditionalAccessExpression(
                            instanceExpression,
                            InvocationExpression( MemberBindingExpression( name ) ) );

                // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
                if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), this._method.DeclaringType.GetSymbol().OriginalDefinition ) )
                {
                    expression = expression.WithAspectReferenceAnnotation( this.AspectReference.WithTargetKind( targetKind ) );
                }
            }

            return new UserExpression( expression, returnType );
        }
    }
}