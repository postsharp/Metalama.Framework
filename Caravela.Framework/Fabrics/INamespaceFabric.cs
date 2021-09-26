// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Fabrics
{
    /// <summary>
    /// Namespace fabrics are types that can provide aspects and constraints to types the same namespace as the namespace policy type itself.
    /// They can be arbitrarily named as long as they implement this interface, but their namespace matters.  
    /// </summary>
    public interface INamespaceFabric : IFabric
    {
        void BuildFabric( INamespaceFabricBuilder builder );
    }
}