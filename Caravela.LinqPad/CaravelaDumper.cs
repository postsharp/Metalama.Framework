// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.LinqPad
{
    /// <summary>
    /// Provides a <see cref="ToDump"/> method, which can be used to format object trees in the way that <see cref="CaravelaDriver"/> does,
    /// but without using <see cref="CaravelaDriver"/>.
    /// </summary>
    public static class CaravelaDumper
    {
        /// <summary>
        /// Formats object trees in the way that <see cref="CaravelaDriver"/> does but without using <see cref="CaravelaDriver"/>.
        /// </summary>
        public static object? ToDump( object? obj ) => FacadeObjectFactory.GetFacade( obj ) ?? obj;
    }
}