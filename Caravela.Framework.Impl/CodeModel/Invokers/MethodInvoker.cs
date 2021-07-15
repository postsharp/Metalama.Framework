// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Invokers
{
    internal class MethodInvoker : Invoker, IMethodInvoker
    {
        private readonly IMethod _method;

        public MethodInvoker( IMethod method, InvokerOrder order ) : base( method, order )
        {
            this._method = method;
        }

        public object Invoke( object? instance, params object?[] args )
        {
            if ( this._method.IsOpenGeneric )
            {
                throw GeneralDiagnosticDescriptors.CannotAccessOpenGenericMember.CreateException( this._method );
            }

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

            return new DynamicExpression(
                SyntaxFactory.InvocationExpression( 
                    SyntaxFactory.MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiver, name )
                    .WithAspectReferenceAnnotation( this.AspectReference ) )
                .AddArgumentListArguments( arguments ),
                this._method.ReturnType,
                false );
        }
    }
}