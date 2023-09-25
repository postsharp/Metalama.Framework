// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class ConstructorBuilder : MemberBuilder, IConstructorBuilder, IConstructorImpl
{
    public ConstructorInitializerKind InitializerKind => ConstructorInitializerKind.None;

    IParameterList IHasParameters.Parameters => (IParameterList) this.Parameters;

    public IParameterBuilderList Parameters => ParameterBuilderList.Empty;

    public override bool IsExplicitInterfaceImplementation => false;

    public override IMember? OverriddenMember => null;

    public IInjectMemberTransformation ToTransformation()
        => this.IsStatic
            ? new IntroduceStaticConstructorTransformation( this.ParentAdvice, this )
            : new ReplaceDefaultConstructorTransformation( this.ParentAdvice, this );

    // This is implemented by BuiltConstructor and there is no point to support it here.
    public IConstructor GetBaseConstructor() => throw new NotSupportedException();

    public override string Name
    {
        get => this.IsStatic ? ".cctor" : ".ctor";
        set => throw new NotSupportedException();
    }

    public override DeclarationKind DeclarationKind => DeclarationKind.Constructor;

    public ConstructorBuilder( INamedType targetType, Advice advice )
        : 
        base( targetType, null!, advice ) { }

    public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
    {
        throw new NotImplementedException();
    }

    public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
    {
        throw new NotImplementedException();
    }

    public ConstructorInfo ToConstructorInfo() => CompileTimeConstructorInfo.Create( this );

    IConstructor IConstructor.Definition => this;

    public System.Reflection.MethodBase ToMethodBase() => this.ToConstructorInfo();
}