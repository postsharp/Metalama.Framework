// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Contracts;

/// <summary>
/// An implementation of <see cref="ICompilerServiceProvider"/> that is returned when there is a mismatch
/// of contract pre-release version.
/// </summary>
internal class InvalidCompilerServiceProvider : ICompilerServiceProvider
{
    public Version Version { get; }

    public ImmutableDictionary<string, int> ContractVersions { get; }

    public T? GetService<T>()
        where T : class, ICompilerService
        => null;

    public InvalidCompilerServiceProvider( Version version, ImmutableDictionary<string, int> contractVersions )
    {
        this.Version = version;
        this.ContractVersions = contractVersions;
    }
}