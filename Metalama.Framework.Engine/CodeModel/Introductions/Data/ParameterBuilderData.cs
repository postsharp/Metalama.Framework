// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class ParameterBuilderData : DeclarationBuilderData
{
    private readonly IRef<IParameter> _ref;

    public string Name { get; }

    public IRef<IType> Type { get; }

    public RefKind RefKind { get; }

    public int Index { get; }

    public TypedConstant? DefaultValue { get; }

    public bool IsParams { get; }

    public ParameterBuilderData( BaseParameterBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new DeclarationBuilderDataRef<IParameter>( this );
        this.Name = builder.Name;
        this.Type = builder.Type.ToRef();
        this.RefKind = builder.RefKind;
        this.Index = builder.Index;
        this.DefaultValue = builder.DefaultValue;
        this.IsParams = builder.IsParams;
    }

    protected override IRef<IDeclaration> ToDeclarationRef() => this._ref;

    public new IRef<IParameter> ToRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;
}