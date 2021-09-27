// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class FabricAspectClass : IAspectClassGroup
    {
        public const string FabricAspectName = "<Fabric>";
        public string FullName => FabricAspectName;
        
        public string DisplayName => FabricAspectName;

        public string? Description => null;

        public bool IsAbstract => false;

        public IAspectClassImpl GetImplementation( IDeclaration target ) => throw new System.NotImplementedException();
    }
}