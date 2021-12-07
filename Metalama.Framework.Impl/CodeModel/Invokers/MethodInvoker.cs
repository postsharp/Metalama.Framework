// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Impl.Aspects;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Templating;
using Metalama.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.Impl.CodeModel.Invokers
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

            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            if ( this._method.IsGeneric )
            {
                name = SyntaxFactory.GenericName( this._method.Name )
                    .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList(
                                this._method.TypeArguments.Select(
                                        t =>
                                            syntaxGenerationContext.SyntaxGenerator.Type( t.GetSymbol() ) )
                                    .ToArray() ) ) );
            }
            else
            {
                name = SyntaxFactory.IdentifierName( this._method.Name );
            }

            var arguments = this._method.GetArguments(
                this._method.Parameters,
                RuntimeExpression.FromValue( args, this.Compilation, syntaxGenerationContext ) );

            if ( this._method.MethodKind == MethodKind.LocalFunction )
            {
                var instanceExpression = RuntimeExpression.FromValue( instance, this.Compilation, syntaxGenerationContext );

                if ( instanceExpression.Syntax.Kind() != SyntaxKind.NullLiteralExpression )
                {
                    throw GeneralDiagnosticDescriptors.CannotProvideInstanceForLocalFunction.CreateException( this._method );
                }

                return new UserExpression(
                    SyntaxFactory.InvocationExpression(
                            name
                                .WithAspectReferenceAnnotation( this.AspectReference ) )
                        .AddArgumentListArguments( arguments ),
                    this._method.ReturnType,
                    syntaxGenerationContext );
            }

            var receiver = this._method.GetReceiverSyntax(
                RuntimeExpression.FromValue( instance!, this.Compilation, syntaxGenerationContext ),
                syntaxGenerationContext );

            if ( this._invokerOperator == InvokerOperator.Default )
            {
                var invocationExpression = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiver, name )
                            .WithAspectReferenceAnnotation( this.AspectReference ) )
                    .AddArgumentListArguments( arguments );

                return new UserExpression( invocationExpression, this._method.ReturnType, syntaxGenerationContext );
            }
            else
            {
                var invocationExpression = SyntaxFactory.ConditionalAccessExpression(
                        receiver,
                        SyntaxFactory.InvocationExpression( SyntaxFactory.MemberBindingExpression( name ) )
                            .AddArgumentListArguments( arguments ) )
                    .WithAspectReferenceAnnotation( this.AspectReference );

                return new UserExpression( invocationExpression, this._method.ReturnType.ConstructNullable(), syntaxGenerationContext );
            }
        }
    }
}