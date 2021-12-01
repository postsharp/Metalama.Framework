// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal struct AssemblyTypeName
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
