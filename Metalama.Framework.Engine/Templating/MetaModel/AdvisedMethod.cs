// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Reflection;
using MethodBase = System.Reflection.MethodBase;

namespace Metalama.Framework.Engine.Templating.MetaModel;

internal sealed class AdvisedMethod : AdvisedMember<IMethodImpl>, IAdvisedMethod, IMethodImpl
{
    public AdvisedMethod( IMethod underlying ) : base( (IMethodImpl) underlying ) { }

    public object? Invoke( params object?[] args )
    {
        if ( this.Invokers.Base != null )
        {
            return this.Invokers.Base.Invoke( this.IsStatic ? null : this.This, args );
        }
        else if ( this.ReturnType.Equals( SpecialType.Void ) )
        {
            return null;
        }
        else
        {
            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            return new SyntaxUserExpression(
                generationContext.SyntaxGenerator.DefaultExpression( this.ReturnType.GetSymbol() ),
                this.ReturnType );
        }
    }

    [Memo]
    public IAdvisedParameterList Parameters => new AdvisedParameterList( this.Underlying );

    IParameterList IHasParameters.Parameters => this.Underlying.Parameters;

    public MethodKind MethodKind => this.Underlying.MethodKind;

    public OperatorKind OperatorKind => this.Underlying.OperatorKind;

    public IMethod MethodDefinition => this.Underlying.MethodDefinition;

    public bool IsExtern => this.Underlying.IsExtern;

    public bool IsReadOnly => this.Underlying.IsReadOnly;

    public MethodBase ToMethodBase() => this.Underlying.ToMethodBase();

    public IParameter ReturnParameter => this.Underlying.ReturnParameter;

    public IType ReturnType => this.Underlying.ReturnType;

    public IGenericParameterList TypeParameters => this.Underlying.TypeParameters;

    public IReadOnlyList<IType> TypeArguments => this.Underlying.TypeArguments;

    public bool IsGeneric => this.Underlying.IsGeneric;

    public bool IsCanonicalGenericInstance => true;

    public IGeneric ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => this.Underlying.ConstructGenericInstance( typeArguments );

    public IInvokerFactory<IMethodInvoker> Invokers => this.Underlying.Invokers;

    public IMethod? OverriddenMethod => this.Underlying.OverriddenMethod;

    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => this.Underlying.ExplicitInterfaceImplementations;

    public MethodInfo ToMethodInfo() => this.Underlying.ToMethodInfo();

    public IMemberWithAccessors? DeclaringMember => this.Underlying.DeclaringMember;

    public IMember? OverriddenMember => this.OverriddenMethod;

    public bool? IsIteratorMethod => this.Underlying.IsIteratorMethod;
}