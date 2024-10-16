// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.GenericContexts;

internal sealed class NullGenericContext : GenericContext
{
    internal override GenericContextKind Kind => GenericContextKind.Null;

    internal override ImmutableArray<IFullRef<IType>> TypeArguments => ImmutableArray<IFullRef<IType>>.Empty;

    public override IType Map( ITypeParameter typeParameter ) => typeParameter;

    public override bool Equals( GenericContext? other ) => other is NullGenericContext;

    protected override int GetHashCodeCore() => 0;
}