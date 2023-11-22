// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if TEST_OPTIONS
// @Skipped(#31108)
#endif

using Metalama.Framework.Engine.Utilities.Roslyn;
using System.Linq;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal sealed class InlinerProvider
    {
        private readonly Inliner[] _inliners = new Inliner[]
        {
            new MethodAssignmentInliner(),
            new MethodLocalDeclarationInliner(),
            new MethodReturnStatementInliner(),
            new MethodCastReturnStatementInliner(),
            new MethodInvocationInliner(),
            new MethodDiscardInliner(),
            new PropertyGetAssignmentInliner(),
            new PropertyGetReturnInliner(),
            new PropertyGetCastReturnInliner(),
            new PropertyGetLocalDeclarationInliner(),
            new PropertySetValueAssignmentInliner(),
            new EventAddAssignmentInliner(),
            new EventRemoveAssignmentInliner()
        };

        public bool TryGetInliner( ResolvedAspectReference aspectReference, ISemanticModel semanticModel, out Inliner? inliner )
        {
            // TODO: Optimize.
            inliner = this._inliners
                .SingleOrDefault(
                    i => i.IsValidForTargetSymbol( aspectReference.ResolvedSemantic.Symbol )
                         && i.IsValidForContainingSymbol( aspectReference.ResolvedSemantic.Symbol ) && i.CanInline( aspectReference, semanticModel ) );

            return inliner != null;
        }
    }
}