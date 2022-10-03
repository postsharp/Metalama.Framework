// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.LamaSerialization
{
    internal readonly struct AssemblyTypeName
    {
        public AssemblyTypeName( string typeName, string assemblyName )
        {
            this.TypeName = typeName;
            this.AssemblyName = assemblyName;
        }

        public readonly string TypeName;
        public readonly string AssemblyName;
    }
}