// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Collections;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// An implementation of <see cref="IAssemblyLocator"/> that looks in metadata references of a <see cref="Compilation"/>.
    /// </summary>
    [ExcludeFromCodeCoverage] // Not used in tests.
    internal class AssemblyLocator : IAssemblyLocator
    {
        private const string _unknownAssemblyName = "*";

        private readonly ImmutableDictionaryOfArray<string, MetadataReference> _referencesByName;
        private readonly ILogger _logger;

        public AssemblyLocator( IServiceProvider serviceProvider, IEnumerable<MetadataReference> references )
        {
            this._referencesByName = references.ToMultiValueDictionary(
                x => GetAssemblyName( x ) ?? _unknownAssemblyName,
                x => x,
                StringComparer.OrdinalIgnoreCase );

            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "AssemblyLocator" );
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

            this._logger.Trace?.Log( $"Finding the location of '{assemblyIdentity}'." );

            reference = this._referencesByName[assemblyIdentity.Name].FirstOrDefault( MatchReference )
                        ?? this._referencesByName[_unknownAssemblyName].FirstOrDefault( MatchReference );

            if ( reference == null && this._logger.Info != null )
            {
                foreach ( var unmatchedReferences in this._referencesByName[assemblyIdentity.Name] )
                {
                    this._logger.Info?.Log(
                        $"The reference '{unmatchedReferences.Display}' was found but did not match the required reference '{assemblyIdentity}'." );
                }
            }

            // TODO: This implementation looks for exact matches only. More testing is required with assembly binding redirections.
            // However, this is should be tested from MSBuild.

            return reference != null;
        }
    }
}