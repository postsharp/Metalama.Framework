// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;


namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal abstract class MemberBuilder : MemberOrNamedTypeBuilder, IMemberBuilderImpl
{
    private bool _isVirtual;
    private bool _isAsync;
    private bool _isOverride;

    protected MemberBuilder( INamedType declaringType, string name, Advice advice ) : base( advice, declaringType, name ) { }

    public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

    public abstract bool IsExplicitInterfaceImplementation { get; }

    public bool IsVirtual
    {
        get => this._isVirtual;
        set
        {
            this.CheckNotFrozen();

            this._isVirtual = value;
        }
    }

    public bool IsAsync
    {
        get => this._isAsync;
        set
        {
            this.CheckNotFrozen();

            this._isAsync = value;
        }
    }

    public bool IsOverride
    {
        get => this._isOverride;
        set
        {
            this.CheckNotFrozen();

            this._isOverride = value;
        }
    }

    IMember IMember.Definition => this;

    IRef<IMember> IMember.ToRef() => throw new NotSupportedException();

    public bool HasImplementation => true;

    public override bool IsDesignTimeObservable => !this.IsOverride && !this.IsNew;

    public abstract IMember? OverriddenMember { get; }

    public override bool CanBeInherited => this.IsVirtual && !this.IsSealed && ((IDeclarationImpl) this.DeclaringType).CanBeInherited;

}