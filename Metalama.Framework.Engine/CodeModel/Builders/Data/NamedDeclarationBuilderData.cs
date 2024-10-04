// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal abstract class NamedDeclarationBuilderData : DeclarationBuilderData
{
    protected NamedDeclarationBuilderData( INamedDeclarationBuilderImpl builder, IRef<IDeclaration> containingDeclaration ) : base(
        builder,
        containingDeclaration )
    {
        this.Name = builder.Name;
    }

    public string Name { get; }
}