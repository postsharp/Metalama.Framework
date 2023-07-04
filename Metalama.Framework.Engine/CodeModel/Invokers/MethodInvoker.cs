// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities.Comparers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
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

        public object? InvokeWithArgumentsObject( object argsObject )
        {
            if (this.Member.MethodKind != MethodKind.Default)
            {
                throw new NotImplementedException();
            }

            // TODO: somehow merge the code that's shared with InvokeDefaultMethod

            SimpleNameSyntax name;

            var receiverInfo = this.GetReceiverInfo();

            if ( this.Member.IsGeneric )
            {
                name = GenericName(
                    Identifier( this.GetCleanTargetMemberName() ),
                    TypeArgumentList(
                        SeparatedList( this.Member.TypeArguments.SelectAsImmutableArray( t => this.GenerationContext.SyntaxGenerator.Type( t.GetSymbol() ) ) ) ) );
            }
            else
            {
                name = IdentifierName( this.GetCleanTargetMemberName() );
            }

            var compilation = this.Member.Compilation;

            var argumentsObjectSyntax = TypedExpressionSyntaxImpl.FromValue( argsObject, compilation, this.GenerationContext );

            TypedExpressionSyntaxImpl[] argumentExpressions;

            // TODO: create a local variable for the arguments object?
            // TODO: test methods optional parameters and short array (it won't work)
            // TODO: formatting or refactoring
            switch ( argumentsObjectSyntax.ExpressionType )
            {
                case IArrayTypeSymbol arrayType when arrayType.IsSZArray:

                    var parametersCount = this.Member.Parameters.Count;

                    argumentExpressions = new TypedExpressionSyntaxImpl[parametersCount];

                    for (var i = 0; i < parametersCount; i++ )
                    {
                        var parameter = this.Member.Parameters[i];
                        if ( parameter.IsParams )
                        {
                            // args.Skip(parametersCount-1).Cast<ParamsParamElementType>().ToArray()

                            var enumerableTypeSyntax = this.GenerationContext.SyntaxGenerator.Type( this.GenerationContext.ReflectionMapper.GetTypeSymbol( typeof(Enumerable) ) );

                            // Don't skip if there's nothing to skip.
                            var skip = i == 0 ? argumentsObjectSyntax.Syntax : InvocationExpression( MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, enumerableTypeSyntax, IdentifierName( nameof(Enumerable.Skip) ) ) )
                                .AddArgumentListArguments(
                                    Argument(argumentsObjectSyntax.Syntax),
                                    Argument( SyntaxFactoryEx.LiteralExpression( i ) ) );

                            var parameterTypeSymbol = parameter.Type.GetSymbol();
                            var elementTypeSymbol = ((IArrayTypeSymbol) parameterTypeSymbol).ElementType;

                            // Don't cast if there's nothing to cast.
                            var cast = this.GenerationContext.CompilationContext.SymbolComparer.Equals( arrayType, parameterTypeSymbol ) ? skip : InvocationExpression( MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, enumerableTypeSyntax, GenericName( nameof(Enumerable.Cast) ).AddTypeArgumentListArguments(this.GenerationContext.SyntaxGenerator.Type( elementTypeSymbol ) ) ) )
                                .AddArgumentListArguments( Argument( skip ) );

                            // Don't call ToArray() if the result is already an array.
                            var toArray = cast == argumentsObjectSyntax.Syntax ? cast : InvocationExpression( MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, enumerableTypeSyntax, IdentifierName( nameof(Enumerable.ToArray) ) ) )
                                .AddArgumentListArguments( Argument( cast ) );

                            argumentExpressions[i] = new TypedExpressionSyntaxImpl( toArray, parameterTypeSymbol, this.GenerationContext );
                        }
                        else
                        {
                            argumentExpressions[i] = new TypedExpressionSyntaxImpl( ElementAccessExpression( argumentsObjectSyntax.Syntax ).AddArgumentListArguments( Argument( SyntaxFactoryEx.LiteralExpression( i ) ) ), arrayType.ElementType, this.GenerationContext, isReferenceable: true );
                        }
                    }

                    break;
                case INamedTypeSymbol namedType when namedType.IsTupleType:
                    argumentExpressions = namedType.TupleElements.SelectAsArray( e => new TypedExpressionSyntaxImpl( MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, argumentsObjectSyntax.Syntax, IdentifierName( e.Name ) ), e.Type, this.GenerationContext, isReferenceable: true ) );
                    break;
                default:
                    throw new InvalidOperationException( $"Type {argumentsObjectSyntax.ExpressionType} is not a supported argument object type. Only single-dimensional arrays and tuples are allowed." );
            }

            var arguments = this.Member.GetArguments(
                this.Member.Parameters,
                argumentExpressions,
                this.GenerationContext );

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
                var receiver = receiverInfo.WithSyntax( this.Member.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this.GenerationContext ) );

                return this.CreateInvocationExpression( receiver, name, arguments, AspectReferenceTargetKind.Self );
            }
        }

        private object InvokeDefaultMethod( object?[] args )
        {
            SimpleNameSyntax name;

            var receiverInfo = this.GetReceiverInfo();

            if ( this.Member.IsGeneric )
            {
                name = GenericName(
                    Identifier( this.GetCleanTargetMemberName() ),
                    TypeArgumentList(
                        SeparatedList( this.Member.TypeArguments.SelectAsImmutableArray( t => this.GenerationContext.SyntaxGenerator.Type( t.GetSymbol() ) ) ) ) );
            }
            else
            {
                name = IdentifierName( this.GetCleanTargetMemberName() );
            }

            var compilation = this.Member.Compilation;

            var arguments = this.Member.GetArguments(
                this.Member.Parameters,
                TypedExpressionSyntaxImpl.FromValues( args, compilation, this.GenerationContext ),
                this.GenerationContext );

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
                var receiver = receiverInfo.WithSyntax( this.Member.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this.GenerationContext ) );

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

                // Only create an aspect reference when the declaring type of the invoked declaration is ancestor of the target of the template (or it's declaring type).
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

                // Only create an aspect reference when the declaring type of the invoked declaration is ancestor of the target of the template (or it's declaring type).
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