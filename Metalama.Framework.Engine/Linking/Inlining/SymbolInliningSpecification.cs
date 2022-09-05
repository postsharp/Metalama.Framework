// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal class SymbolInliningSpecification
    {
        public IntermediateSymbolSemantic Semantic { get; }

        public IReadOnlyDictionary<ResolvedAspectReference, Inliner> SelectedInliners { get; }

        public SymbolInliningSpecification( IntermediateSymbolSemantic semantic, params KeyValuePair<ResolvedAspectReference, Inliner>[] selectedInliners )
        {
            this.Semantic = semantic;
            var selectedInlinersDictionary = new Dictionary<ResolvedAspectReference, Inliner>( selectedInliners.Length );

            foreach ( var selectedInliner in selectedInliners )
            {
                selectedInlinersDictionary.Add( selectedInliner.Key, selectedInliner.Value );
            }

            this.SelectedInliners = selectedInlinersDictionary;
        }
    }
}