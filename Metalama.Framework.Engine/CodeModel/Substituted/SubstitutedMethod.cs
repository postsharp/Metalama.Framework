// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Substituted;

internal sealed class SubstitutedMethod : SubstitutedMember, IMethodImpl
{
 
    public SubstitutedMethod( BuiltMethod definition, INamedType declaringType )
        : base( declaringType )
    {
        this.Definition = definition;
    }
    
    public new BuiltMethod Definition { get; }

    protected override IMemberImpl GetDefinition() => this.Definition;

    IMethod IMethod.Definition => this.Definition;

    public System.Reflection.MethodBase ToMethodBase() => throw new NotImplementedException();

    [Memo]
    private BoxedRef<IMethod> BoxedRef => new( this.ToValueTypedRef() );

    IRef<IMethod> IMethod.ToRef() => this.BoxedRef;

    IRef<IMethodBase> IMethodBase.ToRef() => this.BoxedRef;

    public IGenericParameterList TypeParameters => this.Definition.TypeParameters;

    public IReadOnlyList<IType> TypeArguments => this.Definition.TypeArguments;

    public bool IsGeneric => this.Definition.IsGeneric;

    public bool IsCanonicalGenericInstance => false;

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => throw new NotImplementedException();

    // TODO: test invocations and invokers
    public object? Invoke( params object?[] args ) => new MethodInvoker( this ).Invoke( args );

    public object? Invoke( IEnumerable<IExpression> args ) => new MethodInvoker( this ).Invoke( args );

    public IMethodInvoker With( IExpression target, InvokerOptions options = default ) => new MethodInvoker( this, options, target );

    public IExpression CreateInvokeExpression( IEnumerable<IExpression> args ) => new MethodInvoker( this ).CreateInvokeExpression( args );

    public IMethodInvoker With( InvokerOptions options ) => new MethodInvoker( this, options );

    public IMethodInvoker With( object? target, InvokerOptions options = default ) => new MethodInvoker( this, options, target );

    bool IMethod.IsPartial => ((IMethod) this.Definition).IsPartial;

    public MethodKind MethodKind => this.Definition.MethodKind;

    [Memo]
    public IParameter ReturnParameter => new SubstitutedParameter( this, this.Definition.ReturnParameter );

    [Memo]
    public IParameterList Parameters => new SubstitutedParameterList( this, this.Definition.Parameters.SelectAsImmutableArray( param => new SubstitutedParameter( this, param ) ) );

    public IType ReturnType => this.Substitute( this.Definition.ReturnType );

    public IMethod? OverriddenMethod => (IMethod?) this.OverriddenMember;

    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations
        => this.Definition.ExplicitInterfaceImplementations.SelectAsReadOnlyList(
            m => this.Compilation.Factory.GetSubstitutedMethod( m, this.DeclaringType ) );

    public MethodInfo ToMethodInfo() => throw new NotImplementedException();

    // TODO: this is correct for BuiltMethod, but not for BuiltAccessor
    IHasAccessors? IMethod.DeclaringMember => null;

    public bool IsReadOnly => this.Definition.IsReadOnly;

    public OperatorKind OperatorKind => this.Definition.OperatorKind;
    
    bool IMethod.IsExtern => ((IMethod) this.Definition).IsExtern;

    public bool? IsIteratorMethod => this.Definition.IsIteratorMethod;

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
    {
        var parameterTypes = this.Parameters.AsEnumerable().Select( p => p.Type );

        return DisplayStringFormatter.Format( format, context, $"{this.DeclaringType}.{this.Name}({parameterTypes})" );
    }
}