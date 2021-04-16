// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal partial class Method
    {
        private readonly struct MethodInvocation : IMethodInvocation
        {
            private readonly Method _method;

            public MethodInvocation( Method method )
            {
                this._method = method;
            }

            public object Invoke( object? instance, params object[] args )
            {
                if ( this._method.IsOpenGeneric )
                {
                    throw GeneralDiagnosticDescriptors.CannotAccessOpenGenericMember.CreateException( this._method );
                }

                var name = this._method.GenericArguments.Any()
                    ? (SimpleNameSyntax) this._method.Compilation.SyntaxGenerator.GenericName(
                        this._method.Name,
                        this._method.GenericArguments.Select( a => a.GetSymbol() ) )
                    : SyntaxFactory.IdentifierName( this._method.Name );

                var arguments = this._method.GetArguments( this._method.Parameters, RuntimeExpression.FromDynamic( args ) );

                if ( ((IMethod) this._method).MethodKind == MethodKind.LocalFunction )
                {
                    var instanceExpression = RuntimeExpression.FromDynamic( instance );

                    if ( instanceExpression != null )
                    {
                        throw GeneralDiagnosticDescriptors.CannotProvideInstanceForLocalFunction.CreateException( this._method );
                    }

                    return new DynamicMember(
                        SyntaxFactory.InvocationExpression( name ).AddArgumentListArguments( arguments ),
                        this._method.ReturnType,
                        false );
                }

                var receiver = this._method.GetReceiverSyntax( RuntimeExpression.FromDynamic( instance! ) );

                return new DynamicMember(
                    SyntaxFactory.InvocationExpression( SyntaxFactory.MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiver, name ) )
                        .AddArgumentListArguments( arguments ),
                    this._method.ReturnType,
                    false );
            }
        }
    }
}