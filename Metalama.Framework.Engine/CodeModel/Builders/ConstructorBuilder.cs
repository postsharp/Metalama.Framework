﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class ConstructorBuilder : MethodBaseBuilder, IConstructorBuilder, IConstructorImpl
{
    private IRef<IConstructor>? _replacedImplicit;
    private ConstructorInitializerKind _initializerKind;

    public IRef<IConstructor>? ReplacedImplicit
    {
        get => this._replacedImplicit;
        set
        {
            this.CheckNotFrozen();
            this._replacedImplicit = value;
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

    public ConstructorBuilder( Advice advice, INamedType targetType )
        : base( advice, targetType, null! )
    {
        this.InitializerArguments = new List<(IExpression Expression, string? ParameterName)>();
    }

    public override IRef<IMemberOrNamedType> ToMemberOrNamedTypeRef() => this.Ref;

    public void AddInitializerArgument( IExpression expression, string? parameterName )
    {
        this.CheckNotFrozen();

        this.InitializerArguments.Add( (expression, parameterName) );
    }

    bool IConstructor.IsPrimary => false;

    public override IMember? OverriddenMember => null;

    public override IRef<IMember> ToMemberRef() => this.Ref;

    public override bool IsExplicitInterfaceImplementation => false;

    public IInjectMemberTransformation ToTransformation()
        => this.IsStatic
            ? new IntroduceStaticConstructorTransformation( this.ParentAdvice, this )
            : new IntroduceConstructorTransformation( this.ParentAdvice, this );

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

    public override IRef<IMethodBase> ToMethodBaseRef() => this.Ref;

    public override System.Reflection.MethodBase ToMethodBase() => this.ToConstructorInfo();

    public object Invoke( params object?[] args ) => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    public object Invoke( IEnumerable<IExpression> args ) => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    public IObjectCreationExpression CreateInvokeExpression() => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    public IObjectCreationExpression CreateInvokeExpression( params object?[] args )
        => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    public IObjectCreationExpression CreateInvokeExpression( params IExpression[] args )
        => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    public IObjectCreationExpression CreateInvokeExpression( IEnumerable<IExpression> args )
        => throw new NotSupportedException( "Constructor builders cannot be invoked." );

    [Memo]
    public BuilderRef<IConstructor> Ref => new( this );

    public override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    public new IRef<IConstructor> ToRef() => this.Ref;
}