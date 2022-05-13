// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Utilities;

namespace Metalama.Framework.Engine.Utilities
{
    public static class EngineAssemblyMetadataReader
    {
        public static readonly AssemblyMetadataReader Instance = AssemblyMetadataReader.GetInstance( typeof(EngineAssemblyMetadataReader).Assembly );
    }
}