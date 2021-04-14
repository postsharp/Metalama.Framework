// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Represents the suppression of a diagnostic of a given id in a given scope.
    /// </summary>
    public readonly struct ScopedSuppression
    {
        public string Id { get; }

        public ICodeElement CodeElement { get; }

        public ScopedSuppression( string id, ICodeElement codeElement )
        {
            this.Id = id;
            this.CodeElement = codeElement;
        }
    }
}