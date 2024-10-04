// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Builders.Data;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders.Built;

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

    public MethodInfo ToMethodInfo() => this._methodBuilder.ToMethodInfo();

    IHasAccessors? IMethod.DeclaringMember => null;

    public override MethodBase ToMethodBase() => this.ToMethodInfo();

    [Memo]
    private IRef<IMethod> Ref => this.RefFactory.FromBuilt<IMethod>( this );

    public IRef<IMethod> ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    [Memo]
    public IParameter ReturnParameter => new BuiltParameter( this._methodBuilder.ReturnParameter, this.Compilation, this.GenericContext );

    [Memo]
    public IType ReturnType => this.MapType( this._methodBuilder.ReturnParameter.Type );

    [Memo]
    public ITypeParameterList TypeParameters
        => new TypeParameterList(
            this,
            this._methodBuilder.TypeParameters.AsBuilderList.Select( x => this.RefFactory.FromBuilderData<ITypeParameter>( x ) ).ToReadOnlyList() );

    public IReadOnlyList<IType> TypeArguments => this.TypeParameters;

    public bool IsGeneric => !this._methodBuilder.TypeParameters.IsDefault;

    public bool IsCanonicalGenericInstance => throw new NotImplementedException();

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => throw new NotImplementedException();

    [Memo]
    public IMethod? OverriddenMethod => this.MapDeclaration( this._methodBuilder.OverriddenMethod );

    [Memo]
    public IMethod Definition => this.Compilation.Factory.GetMethod( this._methodBuilder ).AssertNotNull();

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    bool IMethod.IsPartial => false;

    bool IMethod.IsExtern => false;

    public IMethodInvoker With( InvokerOptions options ) => this._methodBuilder.With( options );

    public IMethodInvoker With( object? target, InvokerOptions options = default ) => this._methodBuilder.With( target, options );

    public IMethodInvoker With( IExpression target, InvokerOptions options = default ) => this._methodBuilder.With( target, options );

    public IExpression CreateInvokeExpression( IEnumerable<IExpression> args ) => this._methodBuilder.CreateInvokeExpression( args );

    public object? Invoke( params object?[] args ) => this._methodBuilder.Invoke( args );

    public object? Invoke( IEnumerable<IExpression> args ) => this._methodBuilder.Invoke( args );

    public bool? IsIteratorMethod => this._methodBuilder.IsIteratorMethod;
}