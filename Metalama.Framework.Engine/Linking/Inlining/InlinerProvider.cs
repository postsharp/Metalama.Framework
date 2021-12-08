// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal class InlinerProvider
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

        public bool TryGetInliner( ResolvedAspectReference aspectReference, SemanticModel semanticModel, out Inliner? inliner )
        {
            // TODO: Optimize.
            inliner = this._inliners
                .Where( i => i.IsValidForTargetSymbol( aspectReference.ResolvedSemantic.Symbol ) )
                .Where( i => i.IsValidForContainingSymbol( aspectReference.ResolvedSemantic.Symbol ) )
                .Where( i => i.CanInline( aspectReference, semanticModel ) )
                .SingleOrDefault();

            return inliner != null;
        }
    }
}