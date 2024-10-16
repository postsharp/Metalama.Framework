// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.GenericContexts;

internal sealed class NullGenericContext : GenericContext
{
    public override GenericContextKind Kind => GenericContextKind.Null;

    public override IType Map( ITypeParameter typeParameter ) => typeParameter;

    public override bool Equals( GenericContext? other ) => other is NullGenericContext;

    protected override int GetHashCodeCore() => 0;
}