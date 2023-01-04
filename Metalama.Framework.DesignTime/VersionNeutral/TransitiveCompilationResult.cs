// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.Pipeline;

namespace Metalama.Framework.DesignTime.VersionNeutral;

internal class TransitiveCompilationResult : ITransitiveCompilationResult
{
    public bool IsSuccessful { get; }

    public bool IsPipelinePaused { get; }

    public byte[]? Manifest { get; }

    public TransitiveCompilationResult( bool isSuccessful, bool isPipelinePaused, byte[]? manifest )
    {
        this.IsSuccessful = isSuccessful;
        this.IsPipelinePaused = isPipelinePaused;
        this.Manifest = manifest;
    }

    public static TransitiveCompilationResult Failed() => new TransitiveCompilationResult( false, false, null );
}