// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal sealed class BuiltMethod : BuiltMember, IMethodImpl
{
    private readonly MethodBuilderData _methodBuilder;

    public BuiltMethod( MethodBuilderData builder, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this._methodBuilder = builder;
    }

    public override DeclarationBuilderData BuilderData => this._methodBuilder;

    protected override NamedDeclarationBuilderData NamedDeclarationBuilder => this._methodBuilder;

    protected override MemberOrNamedTypeBuilderData MemberOrNamedTypeBuilder => this._methodBuilder;

    protected override MemberBuilderData MemberBuilder => this._methodBuilder;

    public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

    [Memo]
    public IParameterList Parameters
        => new ParameterList(
            this,
            this.Compilation.GetParameterCollection( this._methodBuilder.ToRef() ) );

    public MethodKind MethodKind => this._methodBuilder.MethodKind;

    public OperatorKind OperatorKind => this._methodBuilder.OperatorKind;

    public bool IsReadOnly => this._methodBuilder.IsReadOnly;

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations
        => this._methodBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( this.MapDeclaration );

    public MethodInfo ToMethodInfo() => CompileTimeMethodInfo.Create( this );

    IHasAccessors? IMethod.DeclaringMember => null;

    [Memo]
    private IRef<IMethod> Ref => this.RefFactory.FromBuilt<IMethod>( this );

    public MethodBase ToMethodBase() => throw new NotImplementedException();

    IRef<IMethodBase> IMethodBase.ToRef() => this.ToRef();

    public IRef<IMethod> ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    [Memo]
    public IParameter ReturnParameter => new BuiltParameter( this._methodBuilder.ReturnParameter, this.Compilation, this.GenericContext, this );

    [Memo]
    public IType ReturnType => this.MapType( this._methodBuilder.ReturnParameter.Type );

    [Memo]
    public ITypeParameterList TypeParameters
        => new TypeParameterList(
            this,
            this._methodBuilder.TypeParameters.Select( x => this.RefFactory.FromBuilderData<ITypeParameter>( x ) ).ToReadOnlyList() );

    public IReadOnlyList<IType> TypeArguments => this.TypeParameters;

    public bool IsGeneric => !this._methodBuilder.TypeParameters.IsDefault;

    public bool IsCanonicalGenericInstance => throw new NotImplementedException();

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => throw new NotImplementedException();

    [Memo]
    public IMethod? OverriddenMethod => this.MapDeclaration( this._methodBuilder.OverriddenMethod );

    [Memo]
    public IMethod Definition => this.Compilation.Factory.GetMethod( this._methodBuilder ).AssertNotNull();

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    bool IMethod.IsExtern => false;

    public IMethodInvoker With( InvokerOptions options ) => new MethodInvoker( this, options );

    public IMethodInvoker With( object? target, InvokerOptions options = default ) => new MethodInvoker( this, options, target );

    public IMethodInvoker With( IExpression target, InvokerOptions options = default ) => new MethodInvoker( this, options, target );

    public IExpression CreateInvokeExpression( IEnumerable<IExpression> args ) => new MethodInvoker( this ).CreateInvokeExpression( args );

    public object? Invoke( params object?[] args ) => new MethodInvoker( this ).Invoke( args );

    public object? Invoke( IEnumerable<IExpression> args ) => new MethodInvoker( this ).Invoke( args );

    public bool? IsIteratorMethod => this._methodBuilder.IsIteratorMethod;
}