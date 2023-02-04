// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
    internal partial class MethodInvoker : Invoker<IMethod>, IMethodInvoker
    {
        public MethodInvoker( IMethod method, InvokerOptions options = default ) : base( method, options ) { }

        public object? Invoke( object? target, params object?[] args )
        {
            var parametersCount = this.Declaration.Parameters.Count;

            if ( parametersCount > 0 && this.Declaration.Parameters[parametersCount - 1].IsParams )
            {
                // The this.Declaration has a 'params' param.
                if ( args.Length < parametersCount - 1 )
                {
                    throw GeneralDiagnosticDescriptors.MemberRequiresAtLeastNArguments.CreateException(
                        (Declaration: this.Declaration, parametersCount - 1, args.Length) );
                }
            }
            else if ( args.Length != parametersCount )
            {
                throw GeneralDiagnosticDescriptors.MemberRequiresNArguments.CreateException( (Declaration: this.Declaration, parametersCount, args.Length) );
            }

            switch ( this.Declaration.MethodKind )
            {
                case MethodKind.Default:
                case MethodKind.LocalFunction:
                    return this.InvokeDefaultMethod( target, args );

                case MethodKind.EventAdd:
                    return ((IEvent) this.Declaration.DeclaringMember!).GetInvoker( this._options ).Add( target, args[0] );

                case MethodKind.EventRaise:
                    return ((IEvent) this.Declaration.DeclaringMember!).GetInvoker( this._options ).Raise( target, args );

                case MethodKind.EventRemove:
                    return ((IEvent) this.Declaration.DeclaringMember!).GetInvoker( this._options ).Remove( target, args[0] );

                case MethodKind.PropertyGet:
                    switch ( this.Declaration.DeclaringMember )
                    {
                        case IProperty property:
                            return property.GetInvoker( this._options ).GetValue( target );

                        case IIndexer indexer:
                            return indexer.GetInvoker( this._options ).GetValue( target, args );

                        default:
                            throw new AssertionFailedException( $"Unexpected declaration for a PropertyGet: '{this.Declaration.DeclaringMember}'." );
                    }

                case MethodKind.PropertySet:
                    switch ( this.Declaration.DeclaringMember )
                    {
                        case IProperty property:
                            property.GetInvoker( this._options ).SetValue( target, args[0] );

                            return null;

                        case IIndexer indexer:
                            indexer.GetInvoker( this._options ).SetValue( target, args );

                            return null;

                        default:
                            throw new AssertionFailedException( $"Unexpected declaration for a PropertySet: '{this.Declaration.DeclaringMember}'." );
                    }

                default:
                    throw new NotImplementedException(
                        $"Cannot generate syntax to invoke the this.Declaration '{this.Declaration}' because this.Declaration kind {this.Declaration.MethodKind} is not implemented." );
            }
        }

        private object InvokeDefaultMethod( object? target, object?[] args )
        {
            SimpleNameSyntax name;

            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var receiverInfo = this.GetReceiverInfo( this.Declaration, target );

            if ( this.Declaration.IsGeneric )
            {
                name = GenericName(
                    Identifier( this.Declaration.Name ),
                    TypeArgumentList(
                        SeparatedList( this.Declaration.TypeArguments.SelectAsImmutableArray( t => generationContext.SyntaxGenerator.Type( t.GetSymbol() ) ) ) ) );
            }
            else
            {
                name = IdentifierName( this.Declaration.Name );
            }

            var compilation = this.Declaration.Compilation;

            var arguments = this.Declaration.GetArguments(
                this.Declaration.Parameters,
                TypedExpressionSyntaxImpl.FromValues( args, compilation, generationContext ),
                generationContext );

            if ( this.Declaration.MethodKind == MethodKind.LocalFunction )
            {
                if ( receiverInfo.Syntax.Kind() != SyntaxKind.NullLiteralExpression )
                {
                    throw GeneralDiagnosticDescriptors.CannotProvideInstanceForLocalFunction.CreateException( this.Declaration );
                }

                return this.CreateInvocationExpression( receiverInfo.ToReceiverExpressionSyntax(), name, arguments, AspectReferenceTargetKind.Self );
            }
            else
            {
                var receiver = receiverInfo.WithSyntax( this.Declaration.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, generationContext ) );

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
                returnType = this.Declaration.ReturnType;

                ExpressionSyntax memberAccessExpression =
                    MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiverTypedExpressionSyntax.Syntax, name );

                // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
                if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), this.Declaration.DeclaringType.GetSymbol().OriginalDefinition ) )
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
                returnType = this.Declaration.ReturnType.ToNullableType();

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
                if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), this.Declaration.DeclaringType.GetSymbol().OriginalDefinition ) )
                {
                    expression = expression.WithAspectReferenceAnnotation(
                        receiverTypedExpressionSyntax.AspectReferenceSpecification.WithTargetKind( targetKind ) );
                }
            }

            return new SyntaxUserExpression( expression, returnType );
        }
    }
}