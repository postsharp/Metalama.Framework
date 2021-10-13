// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.Utilities
{
    /// <summary>
    /// Reads the <see cref="AssemblyMetadataAttribute"/> defined by the build pipeline.
    /// These attributes are defined by Directory.Build.targets.
    /// </summary>
    internal class AssemblyMetadataReader
    {
        private readonly Assembly _assembly;
        private readonly Dictionary<string, string> _metadata = new( StringComparer.OrdinalIgnoreCase );
        private static readonly ConditionalWeakTable<Assembly, AssemblyMetadataReader> _instances = new();

        private AssemblyMetadataReader( Assembly assembly )
        {
            this._assembly = assembly;

            foreach ( var attribute in assembly.GetCustomAttributes( typeof(AssemblyMetadataAttribute) ).Cast<AssemblyMetadataAttribute>() )
            {
                // In case of duplicates, we just ignore the first one. This happens with attributes describing package versions.
                this._metadata[attribute.Key] = attribute.Value;
            }
        }

        /// <summary>
        /// Gets an <see cref="AssemblyMetadataReader"/> for a given <see cref="Assembly"/>.
        /// </summary>
        public static AssemblyMetadataReader GetInstance( Assembly assembly ) => _instances.GetValue( assembly, a => new AssemblyMetadataReader( a ) );

        /// <summary>
        /// Gets the package version with which the current assembly was built.
        /// </summary>
        public string GetPackageVersion( string packageName )
            => this._metadata.TryGetValue( "Package:" + packageName, out var version )
                ? version
                : throw new AssertionFailedException(
                    $"The AssemblyMetadataAttribute for package '{packageName}' is not defined in assembly '{this._assembly.GetName()}'." );

        /// <summary>
        /// Gets the unique BuildId for this assembly.
        /// </summary>
        public Guid ModuleId => this._assembly.ManifestModule.ModuleVersionId;

        public Version Version => this._assembly.GetName().Version;

        /// <summary>
        /// Gets the unique BuildId for the main assembly.
        /// </summary>
        public static string BuildId
            => MainInstance.Version.ToString( 3 ) + "-" +
               string.Join( "", MainInstance.ModuleId.ToByteArray().Take( 4 ).Select( i => i.ToString( "x2", CultureInfo.InvariantCulture ) ) );

        public static AssemblyMetadataReader MainInstance => GetInstance( typeof(AssemblyMetadataReader).Assembly );
    }
}