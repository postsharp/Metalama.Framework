// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Invokers
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
                throw GeneralDiagnosticDescriptors.CannotAccessOpenGenericMember.CreateException( this._method );
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
            var name = this._method.GenericArguments.Any()
                ? LanguageServiceFactory.CSharpSyntaxGenerator.GenericName(
                    this._method.Name,
                    this._method.GenericArguments.Select( a => a.GetSymbol() ) )
                : SyntaxFactory.IdentifierName( this._method.Name );

            var arguments = this._method.GetArguments( this._method.Parameters, RuntimeExpression.FromValue( args ) );

            if ( this._method.MethodKind == MethodKind.LocalFunction )
            {
                var instanceExpression = RuntimeExpression.FromValue( instance );

                if ( instanceExpression != null )
                {
                    throw GeneralDiagnosticDescriptors.CannotProvideInstanceForLocalFunction.CreateException( this._method );
                }

                return new DynamicExpression(
                    SyntaxFactory.InvocationExpression(
                            name
                                .WithAspectReferenceAnnotation( this.AspectReference ) )
                        .AddArgumentListArguments( arguments ),
                    this._method.ReturnType,
                    false );
            }

            var receiver = this._method.GetReceiverSyntax( RuntimeExpression.FromValue( instance! ) );

            if ( this._invokerOperator == InvokerOperator.Default )
            {
                var invocationExpression = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiver, name )
                            .WithAspectReferenceAnnotation( this.AspectReference ) )
                    .AddArgumentListArguments( arguments );

                return new DynamicExpression( invocationExpression, this._method.ReturnType, false );
            }
            else
            {
                var invocationExpression = SyntaxFactory.ConditionalAccessExpression(
                        receiver,
                        SyntaxFactory.InvocationExpression( SyntaxFactory.MemberBindingExpression( name ) )
                            .AddArgumentListArguments( arguments ) )
                    .WithAspectReferenceAnnotation( this.AspectReference );

                return new DynamicExpression( invocationExpression, this._method.ReturnType.MakeNullable(), false );
            }
        }
    }
}