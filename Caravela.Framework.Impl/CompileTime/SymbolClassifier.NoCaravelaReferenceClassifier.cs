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
        private class VanillaClassifier : ISymbolClassifier
        {
            private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;

            public VanillaClassifier( IServiceProvider serviceProvider )
            {
                this._referenceAssemblyLocator = serviceProvider.GetService<ReferenceAssemblyLocator>();
            }

            public TemplateMemberKind GetTemplateMemberKind( ISymbol symbol ) => TemplateMemberKind.None;

            public TemplatingScope GetTemplatingScope( ISymbol symbol )
            {
                if ( SymbolClassifier.TryGetWellKnownScope( symbol, false, out var scopeFromWellKnown ) )
                {
                    return scopeFromWellKnown;
                }
                else
                {
                    var containingAssembly = symbol.ContainingAssembly;

                    return containingAssembly != null && this._referenceAssemblyLocator.StandardAssemblyNames.Contains( containingAssembly.Name )
                        ? TemplatingScope.Both
                        : TemplatingScope.RunTimeOnly;
                }
            }
        }
    }
}