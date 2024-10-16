// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Reflection;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class ParameterBuilder : BaseParameterBuilder
{
    private string? _name;
    private TypedConstant? _defaultValue;
    private RefKind _refKind;
    private IType _type;

    public override RefKind RefKind
    {
        get => this._refKind;
        set
        {
            this.CheckNotFrozen();

            if ( this._refKind != value )
            {
                if ( this.IsReturnParameter )
                {
                    throw new InvalidOperationException( $"Changing the {nameof(this.RefKind)} property of a return parameter is not supported." );
                }

                this._refKind = value;
            }
        }
    }

    public override IType Type
    {
        get => this._type;
        set
        {
            this.CheckNotFrozen();

            this._type = this.Translate( value );
        }
    }

    public override string Name
    {
        get => this._name ?? "<return>";
        set
        {
            this.CheckNotFrozen();

            this._name = this._name != null
                ? value ?? throw new NotSupportedException( "Cannot set the parameter name to null." )
                : throw new NotSupportedException( "Cannot set the name of a return parameter." );
        }
    }

    public override int Index { get; }

    public override TypedConstant? DefaultValue
    {
        get => this._defaultValue;
        set
        {
            this.CheckNotFrozen();

            if ( this.IsReturnParameter )
            {
                throw new NotSupportedException( "Cannot set default value of a return parameter." );
            }

            this._defaultValue = this.Translate( value );
        }
    }

    public override bool IsParams => false;

    public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

    public override IHasParameters DeclaringMember { get; }

    public override ParameterInfo ToParameterInfo() => throw new NotImplementedException();

    public override bool IsReturnParameter => this.Index < 0;

    public ParameterBuilder( IHasParameters declaringMember, int index, string? name, IType type, RefKind refKind, Advice advice ) : base( advice )
    {
        this.DeclaringMember = declaringMember;
        this.Index = index;
        this._name = name;
        this._type = this.Translate( type );
        this._refKind = refKind;
    }

    // TODO: How to implement this?
    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.Name;

    public override SerializableDeclarationId ToSerializableId()
        => throw new NotSupportedException(
            "Getting a serializable identifier is not supported for a parameter that may still be in the process of being added to its method." );

    public override bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;

    protected override SyntaxKind AttributeTargetSyntaxKind => this.IsReturnParameter ? SyntaxKind.ReturnKeyword : SyntaxKind.None;
}