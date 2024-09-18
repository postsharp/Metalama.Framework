// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.CodeModel;

internal class NullGenericContext : IGenericContextImpl
{
    private NullGenericContext() { }

    public static NullGenericContext Instance { get; } = new();

    public bool IsEmptyOrIdentity => true;

    public GenericMap GenericMap => GenericMap.Empty;
}