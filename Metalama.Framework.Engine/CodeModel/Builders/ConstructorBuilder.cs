// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal class ConstructorBuilder : MethodBaseBuilder, IConstructorBuilder, IConstructorImpl
{
    public ConstructorInitializerKind InitializerKind => ConstructorInitializerKind.None;

    bool IConstructor.IsPrimary => false;

    public override IMember? OverriddenMember => null;

    public override bool IsExplicitInterfaceImplementation => false;

    public IInjectMemberTransformation ToTransformation()
        => this.IsStatic
            ? new IntroduceStaticConstructorTransformation( this.ParentAdvice, this )
            : new IntroduceConstructorTransformation( this.ParentAdvice, this );

    // This is implemented by BuiltConstructor and there is no point to support it here.
    public IConstructor GetBaseConstructor() => throw new NotSupportedException();

    public override string Name
    {
        get => this.IsStatic ? ".cctor" : ".ctor";
        set => throw new NotSupportedException();
    }

    public override DeclarationKind DeclarationKind => DeclarationKind.Constructor;

    public ConstructorBuilder( Advice advice, INamedType targetType )
        : base( advice, targetType, null! ) { }

    public ConstructorInfo ToConstructorInfo() => CompileTimeConstructorInfo.Create( this );

    IConstructor IConstructor.Definition => this;

    public override System.Reflection.MethodBase ToMethodBase() => this.ToConstructorInfo();
}