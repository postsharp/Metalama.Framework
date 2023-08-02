// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.Diagnostics;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine.Utilities;
using System.Collections;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;

internal sealed partial class UserProcessServiceHubEndpoint
{
    private sealed class ConsolidatedErrorDiagnosticCollection : IReadOnlyCollection<IDiagnosticData>
    {
        private readonly ImmutableDictionary<ProjectKey, ImmutableArray<IDiagnosticData>> _dictionary;

        public ConsolidatedErrorDiagnosticCollection( ImmutableDictionary<ProjectKey, ImmutableArray<IDiagnosticData>> dictionary )
        {
            this._dictionary = dictionary;
        }

        public IEnumerator<IDiagnosticData> GetEnumerator()
        {
            foreach ( var group in this._dictionary.Values )
            {
                foreach ( var diagnostic in group )
                {
                    yield return diagnostic;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        [Memo]
        public int Count => this._dictionary.Values.Sum( v => v.Length );
    }
}