// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Collections;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// An implementation of <see cref="IAssemblyLocator"/> that looks in metadata references of a <see cref="Compilation"/>.
    /// </summary>
    [ExcludeFromCodeCoverage] // Not used in tests.
    internal class AssemblyLocator : IAssemblyLocator
    {
        private const string _unknownAssemblyName = "*";

        private readonly ImmutableDictionaryOfArray<string, MetadataReference> _referencesByName;

        public AssemblyLocator( IEnumerable<MetadataReference> references )
        {
            this._referencesByName = references.ToMultiValueDictionary(
                x => GetAssemblyName( x ) ?? _unknownAssemblyName,
                x => x,
                StringComparer.OrdinalIgnoreCase );
        }

        private static string? GetAssemblyName( MetadataReference r )
            => r switch
            {
                PortableExecutableReference pe => Path.GetFileNameWithoutExtension( pe.FilePath ),
                CompilationReference c => c.Compilation.AssemblyName ?? "*",
                _ => null
            };

        private static AssemblyIdentity? GetAssemblyIdentity( MetadataReference r )
            => r switch
            {
                PortableExecutableReference { FilePath: { } } pe => AssemblyName.GetAssemblyName( pe.FilePath ).ToAssemblyIdentity(),
                CompilationReference c => c.Compilation.Assembly.Identity,
                _ => null
            };

        public bool TryFindAssembly( AssemblyIdentity assemblyIdentity, [NotNullWhen( true )] out MetadataReference? reference )
        {
            bool MatchReference( MetadataReference r ) => GetAssemblyIdentity( r ) == assemblyIdentity;

            reference = this._referencesByName[assemblyIdentity.Name].FirstOrDefault( MatchReference )
                        ?? this._referencesByName[_unknownAssemblyName].FirstOrDefault( MatchReference );

            // TODO: This implementation looks for exact matches only. More testing is required with assembly binding redirections.
            // However, this is should be tested from MSBuild.

            return reference != null;
        }
    }
}