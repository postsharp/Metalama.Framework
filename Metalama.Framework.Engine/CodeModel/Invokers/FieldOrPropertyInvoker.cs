// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Invokers;

internal sealed class FieldOrPropertyInvoker : Invoker<IFieldOrProperty>, IFieldOrPropertyInvoker, IUserExpression
{
    public FieldOrPropertyInvoker(
        IFieldOrProperty fieldOrProperty,
        InvokerOptions? options = default,
        object? target = null,
        SyntaxGenerationContext? syntaxGenerationContext = null ) : base(
        fieldOrProperty,
        options,
        target,
        syntaxGenerationContext ) { }

    private ExpressionSyntax CreatePropertyExpression( AspectReferenceTargetKind targetKind )
    {
        this.CheckInvocationOptionsAndTarget();

        var receiverInfo = this.GetReceiverInfo();

        var name = IdentifierName( this.GetCleanTargetMemberName() );
        var receiverSyntax = this.Member.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this.GenerationContext );

        ExpressionSyntax expression;

        if ( !receiverInfo.RequiresConditionalAccess )
        {
            expression = MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiverSyntax, name );
        }
        else
        {
            expression = ConditionalAccessExpression( receiverSyntax, MemberBindingExpression( name ) );
        }

        // Only create an aspect reference when the declaring type of the invoked declaration is ancestor of the target of the template (or it's declaring type).
        if ( GetTargetType()?.Is( this.Member.DeclaringType ) ?? false )
        {
            expression = expression.WithAspectReferenceAnnotation( receiverInfo.AspectReferenceSpecification.WithTargetKind( targetKind ) );
        }

        return expression;
    }

    IType IHasType.Type => this.Member.Type;

    RefKind IHasType.RefKind => this.Member.RefKind;

    bool IExpression.IsAssignable => this.Member.IsAssignable;

    public object SetValue( object? value )
    {
        var propertyAccess = this.CreatePropertyExpression( AspectReferenceTargetKind.PropertySetAccessor );

        var expression = AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            propertyAccess,
            TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.SerializationContext ) );

        return new SyntaxUserExpression( expression, this.Member.Type );
    }

    public ref object? Value
        => ref RefHelper.Wrap(
            new SyntaxUserExpression(
                this.CreatePropertyExpression( AspectReferenceTargetKind.Self ),
                this.Member.Type,
                this.IsRef(),
                this.Member.Writeability != Writeability.None ) );

    public IFieldOrPropertyInvoker With( InvokerOptions options ) => this.Options == options ? this : new FieldOrPropertyInvoker( this.Member, options );

    public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default )
        => this.Target == target && this.Options == options ? this : new FieldOrPropertyInvoker( this.Member, options, target );

    public TypedExpressionSyntax GetTypedExpressionSyntax()
        => new TypedExpressionSyntaxImpl(
            this.CreatePropertyExpression( AspectReferenceTargetKind.PropertyGetAccessor ),
            this.Member.Type,
            this.SerializationContext.SyntaxGenerationContext,
            this.IsRef() );

    private bool IsRef() => this.Member.DeclarationKind is DeclarationKind.Field || this.Member.RefKind is RefKind.Ref;

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
    {
        Invariant.Assert(
            this.SerializationContext.SyntaxGenerationContext.Equals( (syntaxGenerationContext as SyntaxSerializationContext)?.SyntaxGenerationContext ) );

        return this.GetTypedExpressionSyntax();
    }
}