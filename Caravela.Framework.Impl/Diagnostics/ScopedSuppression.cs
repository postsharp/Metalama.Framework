// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Represents the suppression of a diagnostic of a given id in a given scope.
    /// </summary>
    public readonly struct ScopedSuppression
    {
        public SuppressionDefinition Definition { get; }

        public ICodeElement CodeElement { get; }

        public ScopedSuppression( SuppressionDefinition definition, ICodeElement codeElement )
        {
            this.Definition = definition;
            this.CodeElement = codeElement;
        }

        public override string ToString() => $"{this.Definition} in {this.CodeElement}";
    }
}