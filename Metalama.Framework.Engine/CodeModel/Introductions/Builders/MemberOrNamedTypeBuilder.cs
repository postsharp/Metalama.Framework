// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal abstract class MemberOrNamedTypeBuilder : NamedDeclarationBuilder, IMemberOrNamedTypeBuilder, IMemberOrNamedTypeImpl
{
    private Accessibility _accessibility;
    private bool _isSealed;
    private bool _isNew;
    private bool _usesNewKeyword;
    private bool _isAbstract;
    private bool _isStatic;

    public bool IsSealed
    {
        get => this._isSealed;
        set
        {
            this.CheckNotFrozen();
            this._isSealed = value;
        }
    }

    public bool IsNew
    {
        get => this._isNew;
        set
        {
            this.CheckNotFrozen();

            this._isNew = value;
        }
    }

    [DisallowNull]
    public bool? HasNewKeyword
    {
        get => this._usesNewKeyword;
        set
        {
            this.CheckNotFrozen();

            this._usesNewKeyword = value.AssertNotNull();
        }
    }

    public INamedType? DeclaringType { get; }

    public MemberInfo ToMemberInfo() => throw new NotImplementedException();

    public ExecutionScope ExecutionScope => ExecutionScope.RunTime;

    public Accessibility Accessibility
    {
        get => this._accessibility;
        set
        {
            this.CheckNotFrozen();

            this._accessibility = value;
        }
    }

    public bool IsAbstract
    {
        get => this._isAbstract;
        set
        {
            this.CheckNotFrozen();

            this._isAbstract = value;
        }
    }

    public bool IsStatic
    {
        get => this._isStatic;
        set
        {
            this.CheckNotFrozen();

            this._isStatic = value;
        }
    }

    public override IDeclaration ContainingDeclaration => this.DeclaringType.AssertNotNull( "Declaring type should not be null (missing override?)." );

    protected MemberOrNamedTypeBuilder( AspectLayerInstance aspectLayerInstance, INamedType? declaringType, string name ) : base( aspectLayerInstance, name )
    {
        this.DeclaringType = declaringType;
        this._usesNewKeyword = false;
    }

    IMemberOrNamedType IMemberOrNamedType.Definition => this;

    IRef<IMemberOrNamedType> IMemberOrNamedType.ToRef() => throw new NotSupportedException();

    bool IMemberOrNamedType.IsPartial => false;
}