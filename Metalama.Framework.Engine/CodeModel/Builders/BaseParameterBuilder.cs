// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class BaseParameterBuilder : DeclarationBuilder, IParameterBuilder, IParameterImpl
{
    public abstract string Name { get; set; }

    public abstract IType Type { get; set; }

    public abstract RefKind RefKind { get; set; }

    public abstract int Index { get; }

    public abstract TypedConstant? DefaultValue { get; set; }

    public abstract bool IsParams { get; }

    public abstract IHasParameters DeclaringMember { get; }

    public abstract ParameterInfo ToParameterInfo();

    public abstract bool IsReturnParameter { get; }

    public sealed override IDeclaration? ContainingDeclaration => this.DeclaringMember;

    protected BaseParameterBuilder( Advice parentAdvice ) : base( parentAdvice ) { }

    bool IExpression.IsAssignable => true;

    public ref object? Value => ref RefHelper.Wrap( new SyntaxUserExpression( SyntaxFactory.IdentifierName( this.Name ), this.Type, true ) );

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => new(
            new TypedExpressionSyntaxImpl( SyntaxFactory.IdentifierName( this.Name ), this.Type, (SyntaxGenerationContext) syntaxGenerationContext, true ) );
}