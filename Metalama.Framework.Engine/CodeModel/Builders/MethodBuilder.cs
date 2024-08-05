// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class MethodBuilder : MethodBaseBuilder, IMethodBuilder, IMethodImpl
{
    private bool _isReadOnly;
    private bool _isIteratorMethod;

    public GenericParameterBuilderList TypeParameters { get; } = new();

    public bool IsReadOnly
    {
        get => this._isReadOnly;
        set
        {
            this.CheckNotFrozen();

            this._isReadOnly = value;
        }
    }

    // A builder is never accessed directly from user code and never represents a generic type instance,
    // so we don't need an implementation of GenericArguments.
    public IReadOnlyList<IType> TypeArguments => throw new NotSupportedException();

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

    public ParameterBuilder ReturnParameter { get; }

    IParameter IMethod.ReturnParameter => this.ReturnParameter;

    IParameterList IHasParameters.Parameters => this.Parameters;

    IParameterBuilderList IHasParametersBuilder.Parameters => this.Parameters;

    IGenericParameterList IGeneric.TypeParameters => this.TypeParameters;

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

    public override IRef<IMethodBase> ToMethodBaseRef() => this.BoxedRef;

    public override System.Reflection.MethodBase ToMethodBase() => this.ToMethodInfo();

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind { get; }

    public OperatorKind OperatorKind { get; }

    IMethod IMethod.Definition => this;

    bool IMethod.IsPartial => false;

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

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
    {
        var parameterTypes = this.Parameters.AsEnumerable<IParameter>().Select( p => p.Type );

        return DisplayStringFormatter.Format( format, context, $"{this.DeclaringType}.{this.Name}({parameterTypes})" );
    }

    public override IMember? OverriddenMember => (IMemberImpl?) this.OverriddenMethod;

    public override IRef<IMember> ToMemberRef() => this.BoxedRef;

    public IInjectMemberTransformation ToTransformation() => new IntroduceMethodTransformation( this.ParentAdvice, this );

    [Memo]
    public BoxedRef<IMethod> BoxedRef => new BoxedRef<IMethod>( this.ToValueTypedRef() );

    public override IRef<IDeclaration> ToIRef() => this.BoxedRef;

    IRef<IMethod> IMethod.ToRef() => this.BoxedRef;

    public override IRef<IMemberOrNamedType> ToMemberOrNamedTypeRef() => this.BoxedRef;
}