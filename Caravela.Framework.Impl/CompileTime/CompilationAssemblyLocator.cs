// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Collections;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// An implementation of <see cref="IAssemblyLocator"/> that looks in metadata references of a <see cref="Compilation"/>.
    /// </summary>
    [ExcludeFromCodeCoverage] // Not used in tests.
    internal class CompilationAssemblyLocator : IAssemblyLocator
    {
        private readonly Compilation _compilation;
        private readonly ImmutableDictionaryOfArray<string, MetadataReference> _referencesByName;

        public CompilationAssemblyLocator( Compilation compilation )
        {
            this._compilation = compilation;

            this._referencesByName = compilation.References.Select(
                    r => r switch
                    {
                        PortableExecutableReference pe => (Name: Path.GetFileNameWithoutExtension( pe.FilePath ) ?? "*", Reference: r),
                        CompilationReference c => (Name: c.Compilation.AssemblyName ?? "*", Reference: r),
                        _ => (Name: "*", Reference: r)
                    } )
                .ToMultiValueDictionary( x => x.Name, x => x.Reference, StringComparer.OrdinalIgnoreCase);
        }

        public bool TryFindAssembly( AssemblyIdentity assemblyIdentity, [NotNullWhen( true )] out MetadataReference? reference )
        {
            bool MatchReference( MetadataReference r ) => this._compilation.GetAssemblyOrModuleSymbol( r ) is IAssemblySymbol assemblySymbol && assemblySymbol.Identity == assemblyIdentity;

            reference = this._referencesByName[assemblyIdentity.Name].FirstOrDefault( MatchReference )
                        ?? this._referencesByName["*"].FirstOrDefault( MatchReference );

            // TODO: This implementation looks for exact matches only. More testing is required with assembly binding redirections.
            // However, this is should be tested from MSBuild.

            return reference != null;
        }
    }
}