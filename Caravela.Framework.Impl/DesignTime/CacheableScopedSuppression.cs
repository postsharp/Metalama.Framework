// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl.DesignTime
{
    internal readonly struct CacheableScopedSuppression
    {
        public string Id { get; }

        public string SymbolId { get; }

        public CacheableScopedSuppression( in ScopedSuppression suppression )
        {
            this.Id = suppression.Id;
            this.SymbolId = suppression.CodeElement.GetSymbol()?.GetDocumentationCommentId() ?? throw new AssertionFailedException();
        }

        public override string ToString() => $"{this.Id} in {this.SymbolId}";
    }
}