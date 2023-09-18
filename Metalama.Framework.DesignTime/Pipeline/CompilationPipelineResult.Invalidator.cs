﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Diagnostics;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed partial class AspectPipelineResult
{
    public sealed class Invalidator
    {
        private readonly AspectPipelineResult _parent;
        private readonly ImmutableDictionary<string, SyntaxTreePipelineResult>.Builder _syntaxTreeBuilders;
        private readonly ImmutableDictionary<string, SyntaxTreePipelineResult>.Builder _invalidSyntaxTreeBuilders;

        public Invalidator( AspectPipelineResult parent )
        {
            this._parent = parent;
            this._syntaxTreeBuilders = parent.SyntaxTreeResults.ToBuilder();

            this._invalidSyntaxTreeBuilders = parent._invalidSyntaxTreeResults.ToBuilder();
        }

        public void InvalidateSyntaxTree( string path )
        {
            Logger.DesignTime.Trace?.Log( $"DesignTimeSyntaxTreeResultCache.InvalidateCache({path}): removed from cache." );

            if ( this._syntaxTreeBuilders.TryGetValue( path, out var oldSyntaxTreeResult ) )
            {
                this._syntaxTreeBuilders.Remove( path );
                this._invalidSyntaxTreeBuilders.Add( path, oldSyntaxTreeResult );
            }
        }

        public AspectPipelineResult ToImmutable()
        {
            return new AspectPipelineResult(
                this._parent.Configuration,
                this._syntaxTreeBuilders.ToImmutable(),
                this._invalidSyntaxTreeBuilders.ToImmutable(),
                this._parent.IntroducedSyntaxTrees,
                this._parent._inheritableAspects,
                this._parent.ReferenceValidators,
                this._parent.InheritableOptions );
        }
    }
}