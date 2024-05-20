// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Invokers;

internal sealed class ConstructorInvoker : Invoker<IConstructor>, IConstructorInvoker
{
    public ConstructorInvoker( IConstructor constructor ) : base( constructor, InvokerOptions.Final, null) { }

    public object? Invoke( params object?[]? args )
    {
        // For some reason, overload resolution chooses the wrong overload in the template,
        // so redirect to the correct one.
        if ( args is [IEnumerable<IExpression> expressionArgs] )
        {
            return this.Invoke( expressionArgs );
        }

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

        return this.InvokeConstructor( args );
    }

    private IExpression InvokeConstructor( object?[] args )
    {
        return new DelegateUserExpression(
            context =>
            {
                SimpleNameSyntax name;

                var receiverInfo = this.GetReceiverInfo( context );

                var type = context.SyntaxGenerator.Type( this.Member.DeclaringType );

                var arguments = this.Member.GetArguments(
                    this.Member.Parameters,
                    TypedExpressionSyntaxImpl.FromValues( args, context ),
                    context.SyntaxGenerationContext );

                var receiver = receiverInfo.WithSyntax( this.Member.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, context ) );

                return CreateObjectCreationExpression( type, arguments );
            },
            this.Member.DeclaringType);
    }

    public object? Invoke( IEnumerable<IExpression> args ) => this.Invoke( args.ToArray<object>() );

    private static ExpressionSyntax CreateObjectCreationExpression(
        TypeSyntax type,
        ArgumentSyntax[]? arguments )
    {
        // TODO: Field initializers.

        var expression =
            ObjectCreationExpression(
                type,
                ArgumentList( SeparatedList( arguments ) ),
                null );

        // Not using any aspect reference because these are not supported for constructor invocations.

        return expression;
    }

    public IExpression CreateInvokeExpression( IEnumerable<IExpression> args ) => this.InvokeConstructor( args.ToArray<object>() );
}