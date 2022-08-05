// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class DesignTimeCompilationReference
{
    private readonly Compilation _compilation;
    private Func<Compilation,Compilation,CancellationToken,CompilationChanges?>? _incrementalChangesGetter;
    
    public ITransitiveAspectsManifest? TransitiveAspectsManifest { get; }

    public ICompilationVersion CompilationVersion { get; }


    public CompilationChanges? GetIncrementalChanges( Compilation from,CancellationToken cancellationToken ) => this._incrementalChangesGetter?.Invoke( from, this._compilation, cancellationToken );

    public DesignTimeCompilationReference(
        Compilation compilation,
        ICompilationVersion compilationVersion,
        Func<Compilation,Compilation,CancellationToken,CompilationChanges?>?  incrementalChangesGetter = null,
        ITransitiveAspectsManifest? transitiveAspectsManifest = null )
    {
        this._compilation = compilation;
        this.TransitiveAspectsManifest = transitiveAspectsManifest;
        this.CompilationVersion = compilationVersion;
        this._incrementalChangesGetter = incrementalChangesGetter;
    }
}