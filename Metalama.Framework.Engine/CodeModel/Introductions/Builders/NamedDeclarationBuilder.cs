// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal abstract class NamedDeclarationBuilder : DeclarationBuilder, INamedDeclarationBuilderImpl
{
    private string _name;

    public virtual string Name
    {
        get => this._name;
        set
        {
            this.CheckNotFrozen();

            this._name = value;
        }
    }

    protected NamedDeclarationBuilder( AspectLayerInstance aspectLayerInstance, string name ) : base( aspectLayerInstance )
    {
        this._name = name;
    }
}