// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Builders.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class MethodBuilder : MethodBaseBuilder, IMethodBuilderImpl
{
    private bool _isReadOnly;
    private bool _isIteratorMethod;

    public TypeParameterBuilderList TypeParameters { get; } = new();

    public bool IsReadOnly
    {
        get => this._isReadOnly;
        set
        {
            this.CheckNotFrozen();

            this._isReadOnly = value;
        }
    }

    public IReadOnlyList<IType> TypeArguments => this.TypeParameters;

    public IMethod? OverriddenMethod { get; set; }

    public MethodInfo ToMethodInfo() => CompileTimeMethodInfo.Create( this );

    IHasAccessors? IMethod.DeclaringMember => null;

    public override void Freeze()
    {
        base.Freeze();

        foreach ( var typeParameter in this.TypeParameters )
        {
            typeParameter.Freeze();
        }

        this.ReturnParameter.Freeze();
    }

    public ITypeParameterBuilder AddTypeParameter( string name )
    {
        this.CheckNotFrozen();

        var builder = new TypeParameterBuilder( this, this.TypeParameters.Count, name );
        this.TypeParameters.Add( builder );

        return builder;
    }

    IParameterBuilder IMethodBuilder.ReturnParameter => this.ReturnParameter;

    public IType ReturnType
    {
        get => this.ReturnParameter.Type;
        set
        {
            this.CheckNotFrozen();

            this.ReturnParameter.Type = value ?? throw new ArgumentNullException( nameof(value) );
        }
    }

    IType IMethod.ReturnType => this.ReturnParameter.Type;

    public override BaseParameterBuilder ReturnParameter { get; set; }

    IParameter IMethod.ReturnParameter => this.ReturnParameter;

    IParameterList IHasParameters.Parameters => this.Parameters;

    IParameterBuilderList IHasParametersBuilder.Parameters => this.Parameters;

    ITypeParameterList IGeneric.TypeParameters => this.TypeParameters;

    public bool IsGeneric => this.TypeParameters.Count > 0;

    public bool IsCanonicalGenericInstance => true;

    // We don't currently support adding other methods than default ones.
    public MethodKind MethodKind
        => this.DeclarationKind switch
        {
            DeclarationKind.Method => MethodKind.Default,
            DeclarationKind.Operator => MethodKind.Operator,
            DeclarationKind.Finalizer => MethodKind.Finalizer,
            _ => throw new AssertionFailedException( $"Unexpected DeclarationKind: {this.DeclarationKind}." )
        };

    public override MethodBase ToMethodBase() => this.ToMethodInfo();

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind { get; }

    public OperatorKind OperatorKind { get; }

    IMethod IMethod.Definition => this;
    
    bool IMethod.IsExtern => false;

    public IMethodInvoker With( InvokerOptions options ) => new MethodInvoker( this, options );

    public IMethodInvoker With( object? target, InvokerOptions options = default ) => new MethodInvoker( this, options, target );

    public IMethodInvoker With( IExpression target, InvokerOptions options = default ) => new MethodInvoker( this, options, target );

    public IExpression CreateInvokeExpression( IEnumerable<IExpression> args ) => new MethodInvoker( this ).CreateInvokeExpression( args );

    public object? Invoke( params object?[] args ) => new MethodInvoker( this ).Invoke( args );

    public object? Invoke( IEnumerable<IExpression> args ) => new MethodInvoker( this ).Invoke( args );

    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations { get; private set; } = Array.Empty<IMethod>();

    public bool? IsIteratorMethod => this._isIteratorMethod;

    internal void SetIsIteratorMethod( bool value ) => this._isIteratorMethod = value;

    public MethodBuilder(
        Advice advice,
        INamedType targetType,
        string name,
        DeclarationKind declarationKind = DeclarationKind.Method,
        OperatorKind operatorKind = OperatorKind.None )
        : base( advice, targetType, name )
    {
        Invariant.Assert(
            declarationKind == DeclarationKind.Operator
                            ==
                            (operatorKind != OperatorKind.None) );

        this.DeclarationKind = declarationKind;
        this.OperatorKind = operatorKind;

        this.ReturnParameter =
            new ParameterBuilder(
                this,
                -1,
                null,
                this.Compilation.Cache.SystemVoidType.AssertNotNull(),
                RefKind.None,
                this.ParentAdvice );
    }

    public void SetExplicitInterfaceImplementation( IMethod interfaceMethod ) => this.ExplicitInterfaceImplementations = new[] { interfaceMethod };

    public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

    public override IMember? OverriddenMember => (IMemberImpl?) this.OverriddenMethod;

    public IInjectMemberTransformation ToTransformation()
    {
        this.Freeze();

        return new IntroduceMethodTransformation( this.ParentAdvice, this );
    }

    IRef<IMethod> IMethod.ToRef() => throw new NotSupportedException();
}