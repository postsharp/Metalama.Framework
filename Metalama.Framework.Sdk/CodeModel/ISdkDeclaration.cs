// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// Extends the user-level <see cref="IDeclaration"/> interface with a <see cref="Symbol"/> property exposing the Roslyn <see cref="ISymbol"/>. 
    /// </summary>
    internal interface ISdkDeclaration : IDeclaration
    {
        /// <summary>
        /// Gets the Roslyn <see cref="ISymbol"/> for the current declaration, or throws <see cref="NotSupportedException"/>
        /// if <see cref="IDeclaration.Origin"/> is a <see cref="IAspectDeclarationOrigin"/>. Note that the symbol returned can be linked to a different
        /// Roslyn compilation than the one provided to the aspect weaver.
        /// </summary>
        ISymbol? Symbol { get; }
        
        /// <summary>
        /// Gets the Roslyn <see cref="Location"/> of the declaration, to emit diagnostics.
        /// </summary>
        Location? DiagnosticLocation { get; }
    }
}