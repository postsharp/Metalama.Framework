// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class SymbolClassifier
    {
        /// <summary>
        /// An implementation of <see cref="ISymbolClassifier"/> for projects that don't have a reference to Caravela.
        /// </summary>
        private class VanillaClassifier : ISymbolClassifier
        {
            private readonly ReferenceAssemblyLocator _referenceAssemblyLocator = ReferenceAssemblyLocator.GetInstance();

            private static VanillaClassifier? _instance;

            public bool IsTemplate( ISymbol symbol ) => false;

            // We don't instantiate this member in the static constructor because we don't want to initialize ReferenceAssemblyLocator
            // from a static constructor (see ReferenceAssemblyLocator).
            public static ISymbolClassifier GetInstance() => _instance ??= new VanillaClassifier();

            public SymbolDeclarationScope GetSymbolDeclarationScope( ISymbol symbol )
            {
                if ( TryGetWellKnownScope( symbol, false, out var scopeFromWellKnown ) )
                {
                    return scopeFromWellKnown;
                }
                else
                {
                    return this._referenceAssemblyLocator.StandardAssemblyNames.Contains( symbol.ContainingAssembly.Name )
                        ? SymbolDeclarationScope.Both
                        : SymbolDeclarationScope.RunTimeOnly;
                }
            }
        }
    }
}