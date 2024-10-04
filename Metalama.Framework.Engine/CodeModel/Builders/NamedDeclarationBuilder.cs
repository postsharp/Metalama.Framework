// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Engine.CodeModel.Builders;

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

    protected NamedDeclarationBuilder( Advice advice, string name ) : base( advice )
    {
        this._name = name;
    }
}