// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel;

internal interface IGenericContextImpl : IGenericContext
{
    GenericMap GenericMap { get; }
}

internal static class GenericContextExtensions
{
    public static GenericMap GetGenericMap( this IGenericContext? genericContext ) => ((IGenericContextImpl) genericContext)?.GenericMap ?? GenericMap.Empty;
}

internal class NullGenericContext : IGenericContextImpl
{
    private NullGenericContext() { }

    public static NullGenericContext Instance { get; } = new();

    public bool IsEmptyOrIdentity => true;

    public GenericMap GenericMap => GenericMap.Empty;
}