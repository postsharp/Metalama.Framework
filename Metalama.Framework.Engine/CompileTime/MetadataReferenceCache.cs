// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Metalama.Framework.Engine.CompileTime
{
    internal static class MetadataReferenceCache
    {
        public static MetadataReference GetMetadataReference( string path )
            => CacheByFile<MetadataReference>.Get( path, p => MetadataReference.CreateFromFile( p ) );

        public static AssemblyName GetAssemblyName( string path ) => CacheByFile<AssemblyName>.Get( path, AssemblyName.GetAssemblyName );
    }
}