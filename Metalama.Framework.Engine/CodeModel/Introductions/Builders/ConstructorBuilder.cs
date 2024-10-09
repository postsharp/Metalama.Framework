// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using MethodBase = System.Reflection.MethodBase;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal sealed class ConstructorBuilder : MethodBaseBuilder, IConstructorBuilder, IConstructorImpl
{
    private IConstructor? _replacedImplicitConstructor;
    private ConstructorInitializerKind _initializerKind;

    public IConstructor? ReplacedImplicitConstructor
    {
        get => this._replacedImplicitConstructor;
        set
        {
            // We intentionally don't store a reference to the replaced constructor, but the constructor itself,
            // because references are always resolved to the _replacement_.

            Invariant.Assert( value is null or Constructor or ConstructorBuilder );
            this.CheckNotFrozen();
            this._replacedImplicitConstructor = value;
        }
    }

    public ConstructorInitializerKind InitializerKind
    {
        get => this._initializerKind;
        set
        {
            this.CheckNotFrozen();
            this._initializerKind = value;
        }
    }

    public List<(IExpression Expression, string? ParameterName)> InitializerArguments { get; }

    public ConstructorBuilder( AspectLayerInstance aspectLayerInstance, INamedType targetType )
        : base( aspectLayerInstance, targetType, null! )
    {
        this.InitializerArguments = [];
    }

    public void AddInitializerArgument( IExpression expression, string? parameterName )
    {
        this.CheckNotFrozen();

        this.InitializerArguments.Add( (expression, parameterName) );
    }

    bool IConstructor.IsPrimary => false;

    public override IMember? OverriddenMember => null;

    public override bool IsExplicitInterfaceImplementation => false;

    // This is implemented by BuiltConstructor and there is no point in supporting it here.
    public IConstructor GetBaseConstructor() => throw new NotSupportedException();

    public override string Name
    {
        get => this.IsStatic ? ".cctor" : ".ctor";
        set => throw new NotSupportedException( "Setting constructor name is not supported" );
    }

    public override DeclarationKind DeclarationKind => DeclarationKind.Constructor;

    public ConstructorInfo ToConstructorInfo() => CompileTimeConstructorInfo.Create( this );

    IConstructor IConstructor.Definition => this;

    public override BaseParameterBuilder? ReturnParameter
    {
        get => null;
        set => throw new NotSupportedException();
    }

    public override MethodBase ToMethodBase() => this.ToConstructorInfo();

    public new IRef<IConstructor> ToRef() => this.Immutable.ToRef();

    public object Invoke( params object?[] args ) => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    public object Invoke( IEnumerable<IExpression> args ) => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    public IObjectCreationExpression CreateInvokeExpression() => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    public IObjectCreationExpression CreateInvokeExpression( params object?[] args )
        => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    public IObjectCreationExpression CreateInvokeExpression( params IExpression[] args )
        => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    public IObjectCreationExpression CreateInvokeExpression( IEnumerable<IExpression> args )
        => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    public IInjectMemberTransformation ToTransformation()
    {
        return this.IsStatic
            ? new IntroduceStaticConstructorTransformation( this.AspectLayerInstance, this.Immutable )
            : new IntroduceConstructorTransformation( this.AspectLayerInstance, this.Immutable );
    }

/*
    public override ICompilationElement? Translate(
        CompilationModel newCompilation,
        IGenericContext? genericContext = null,
        Type? interfaceType = null )
        => this.ReplacedImplicitConstructor?.Translate( newCompilation ) ?? base.Translate( newCompilation, genericContext );
        */

    [Memo]
    public ConstructorBuilderData Immutable => new( this.AssertFrozen(), this.ContainingDeclaration.ToFullRef() );
}