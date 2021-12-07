// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Impl.CompileTime
{
    /// <summary>
    /// Determines the kind of symbol: template, <see cref="TemplatingScope.CompileTimeOnly"/>,
    /// <see cref="TemplatingScope.RunTimeOnly"/>.
    /// </summary>
    internal interface ISymbolClassifier
    {
        TemplateInfo GetTemplateInfo( ISymbol symbol );

        /// <summary>
        /// Gets the scope of a symbol in the context of a template.
        /// </summary>
        TemplatingScope GetTemplatingScope( ISymbol symbol );
    }
}