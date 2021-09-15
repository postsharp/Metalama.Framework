// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class SymbolClassificationService
    {
        /// <summary>
        /// An implementation of <see cref="ISymbolClassifier"/> for projects that don't have a reference to Caravela.
        /// </summary>
        private class NoCaravelaReferenceClassifier : ISymbolClassifier
        {
            private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;

            public NoCaravelaReferenceClassifier( IServiceProvider serviceProvider )
            {
                this._referenceAssemblyLocator = serviceProvider.GetService<ReferenceAssemblyLocator>();
            }

            public TemplateInfo GetTemplateInfo( ISymbol symbol ) => TemplateInfo.None;

            public TemplatingScope GetTemplatingScope( ISymbol symbol )
            {
                if ( symbol is ITypeParameterSymbol )
                {
                    throw new ArgumentOutOfRangeException( nameof(symbol), "Type parameters are not supported." );
                }

                if ( SymbolClassifier.TryGetWellKnownScope( symbol, false, out var scopeFromWellKnown ) )
                {
                    return scopeFromWellKnown;
                }
                else
                {
                    var containingAssembly = symbol.ContainingAssembly;

                    return containingAssembly != null && this._referenceAssemblyLocator.IsStandardAssemblyName( containingAssembly.Name )
                        ? TemplatingScope.Both
                        : TemplatingScope.RunTimeOnly;
                }
            }
        }
    }
}