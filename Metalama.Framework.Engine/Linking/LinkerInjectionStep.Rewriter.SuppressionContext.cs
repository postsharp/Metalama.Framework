// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerInjectionStep
{
    private sealed partial class Rewriter
    {
        /// <summary>
        /// Tracks the set of diagnostics being suppressed at any position in the syntax tree.
        /// </summary>
        private readonly struct SuppressionContext : IDisposable
        {
            private readonly ImmutableHashSet<string> _previousActiveSuppressions;
            private readonly Rewriter _parent;

            public ImmutableArray<string> NewSuppressions { get; }

            public SuppressionContext( Rewriter parent, IEnumerable<string> newSuppressions )
            {
                this._previousActiveSuppressions = parent._activeSuppressions;
                this._parent = parent;
                this.NewSuppressions = newSuppressions.Where( x => !parent._activeSuppressions.Contains( x ) ).ToImmutableArray();

                // Add the new suppressions to the active context.
                foreach ( var suppression in this.NewSuppressions )
                {
                    parent._activeSuppressions = parent._activeSuppressions.Add( suppression );
                }
            }

            public void Dispose() => this._parent._activeSuppressions = this._previousActiveSuppressions;
        }
    }
}