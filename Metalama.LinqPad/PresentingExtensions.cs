// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.LinqPad
{
    /// <summary>
    /// Extension methods to present data in the LinqPad HTML view.
    /// </summary>
    public static class PresentingExtensions
    {
        // Excluded from documentation because it otherwise pollutes all types with the extension method.

        /// <exclude />
        public static object? AsHyperlink( object? obj ) => FacadePropertyFormatter.CreateHyperlink( obj );
    }
}