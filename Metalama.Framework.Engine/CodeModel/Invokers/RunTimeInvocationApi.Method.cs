// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
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
    internal partial class RunTimeInvocationApi
    {
        public object? Invoke( IMethod method, object? target, params object?[] args )
        {
            var parametersCount = method.Parameters.Count;

            if ( parametersCount > 0 && method.Parameters[parametersCount - 1].IsParams )
            {
                // The method has a 'params' param.
                if ( args.Length < parametersCount - 1 )
                {
                    throw GeneralDiagnosticDescriptors.MemberRequiresAtLeastNArguments.CreateException( (method, parametersCount - 1, args.Length) );
                }
            }
            else if ( args.Length != parametersCount )
            {
                throw GeneralDiagnosticDescriptors.MemberRequiresNArguments.CreateException( (method, parametersCount, args.Length) );
            }

            switch ( method.MethodKind )
            {
                case MethodKind.Default:
                case MethodKind.LocalFunction:
                    return this.InvokeDefaultMethod( method, target, args );

                case MethodKind.EventAdd:
                    return ((IEvent) method.DeclaringMember!).Add( target, args[0] );

                case MethodKind.EventRaise:
                    return ((IEvent) method.DeclaringMember!).Raise( target, args );

                case MethodKind.EventRemove:
                    return ((IEvent) method.DeclaringMember!).Remove( target, args[0] );

                case MethodKind.PropertyGet:
                    switch ( method.DeclaringMember )
                    {
                        case IProperty property:
                            return property.GetValue( target );

                        case IIndexer indexer:
                            return indexer.GetValue( target, args );

                        default:
                            throw new AssertionFailedException( $"Unexpected declaration for a PropertyGet: '{method.DeclaringMember}'." );
                    }

                case MethodKind.PropertySet:
                    switch ( method.DeclaringMember )
                    {
                        case IProperty property:
                            property.SetValue( target, args[0] );

                            return null;

                        case IIndexer indexer:
                            indexer.SetValue( target, args );

                            return null;

                        default:
                            throw new AssertionFailedException( $"Unexpected declaration for a PropertySet: '{method.DeclaringMember}'." );
                    }

                default:
                    throw new NotImplementedException(
                        $"Cannot generate syntax to invoke the method '{method}' because method kind {method.MethodKind} is not implemented." );
            }
        }

        private object InvokeDefaultMethod( IMethod method, object? target, object?[] args )
        {
            SimpleNameSyntax name;

            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var receiverInfo = this.GetReceiverInfo( method, target );

            if ( method.IsGeneric )
            {
                name = GenericName(
                    Identifier( method.Name ),
                    TypeArgumentList(
                        SeparatedList( method.TypeArguments.SelectAsImmutableArray( t => generationContext.SyntaxGenerator.Type( t.GetSymbol() ) ) ) ) );
            }
            else
            {
                name = IdentifierName( method.Name );
            }

            var compilation = method.Compilation;

            var arguments = method.GetArguments(
                method.Parameters,
                TypedExpressionSyntaxImpl.FromValues( args, compilation, generationContext ),
                generationContext );

            if ( method.MethodKind == MethodKind.LocalFunction )
            {
                if ( receiverInfo.Syntax.Kind() != SyntaxKind.NullLiteralExpression )
                {
                    throw GeneralDiagnosticDescriptors.CannotProvideInstanceForLocalFunction.CreateException( method );
                }

                return CreateInvocationExpression( method, receiverInfo.ToReceiverExpressionSyntax(), name, arguments, AspectReferenceTargetKind.Self );
            }
            else
            {
                var receiver = receiverInfo.WithSyntax( method.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, generationContext ) );

                return CreateInvocationExpression( method, receiver, name, arguments, AspectReferenceTargetKind.Self );
            }
        }

        private static SyntaxUserExpression CreateInvocationExpression(
            IMethod method,
            ReceiverExpressionSyntax receiverTypedExpressionSyntax,
            SimpleNameSyntax name,
            ArgumentSyntax[]? arguments,
            AspectReferenceTargetKind targetKind )
        {
            ExpressionSyntax expression;
            IType returnType;

            if ( !receiverTypedExpressionSyntax.RequiresNullConditionalAccessMember )
            {
                returnType = method.ReturnType;

                ExpressionSyntax memberAccessExpression =
                    MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiverTypedExpressionSyntax.Syntax, name );

                // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
                if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), method.DeclaringType.GetSymbol().OriginalDefinition ) )
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
                returnType = method.ReturnType.ToNullableType();

                if ( receiverTypedExpressionSyntax == null )
                {
                    throw new AssertionFailedException(
                        $"Cannot generate a conditional access expression '{name.GetLocation()}' because there is no instance expression." );
                }

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

                // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
                if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), method.DeclaringType.GetSymbol().OriginalDefinition ) )
                {
                    expression = expression.WithAspectReferenceAnnotation(
                        receiverTypedExpressionSyntax.AspectReferenceSpecification.WithTargetKind( targetKind ) );
                }
            }

            return new SyntaxUserExpression( expression, returnType );
        }
    }
}