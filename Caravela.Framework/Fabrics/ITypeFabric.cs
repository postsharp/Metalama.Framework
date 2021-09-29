// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Fabrics
{
    /// <summary>
    /// An interface that, when implemented by a nested type in a given type, allows that nested type to analyze and
    /// add aspects to the parent type.
    /// </summary>
    public interface ITypeFabric : IFabric
    {
        /// <summary>
        /// The user can implement this method to analyze types in the declaring type, add aspects, and report or suppress diagnostics.
        /// </summary>
        void AmendType( ITypeAmender builder );
    }
}