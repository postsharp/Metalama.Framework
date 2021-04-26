// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal class DiagnosticList : IDiagnosticAdder, IReadOnlyList<Diagnostic>
    {
        private List<Diagnostic>? _list;

        public DiagnosticList() { }

        private List<Diagnostic> GetList() => this._list ??= new List<Diagnostic>();

        public void ReportDiagnostic( Diagnostic diagnostic ) => this.GetList().Add( diagnostic );

        public IEnumerator<Diagnostic> GetEnumerator() => this._list?.GetEnumerator() ?? Enumerable.Empty<Diagnostic>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this._list?.Count ?? 0;

        public Diagnostic this[ int index ]
        {
            get
            {
                if ( this._list == null )
                {
                    throw new InvalidOperationException();
                }

                return this._list[index];
            }
        }
    }
}