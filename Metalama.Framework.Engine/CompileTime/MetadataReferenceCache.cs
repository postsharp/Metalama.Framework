// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Metalama.Framework.Engine.CompileTime
{
    internal static class MetadataReferenceCache
    {
        private static readonly FileBasedCache<MetadataReference> _metadataReferences = new();
        private static readonly FileBasedCache<AssemblyName> _assemblyNames = new();

        public static MetadataReference GetMetadataReference( string path ) => _metadataReferences.Get( path, p => MetadataReference.CreateFromFile( p ) );

        public static AssemblyName GetAssemblyName( string path ) => _assemblyNames.Get( path, AssemblyName.GetAssemblyName );
    }
}