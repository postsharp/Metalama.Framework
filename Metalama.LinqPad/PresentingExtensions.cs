// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.LinqPad
{
    /// <summary>
    /// Extension methods to present data in the LinqPad HTML view.
    /// </summary>
    public static class PresentingExtensions
    {
        // Excluded from documentation because it otherwise pollutes all types with the extension method.

        /// <summary>
        /// Represents the object as a hyperlink that, when clicked, expands into its detailed view.
        /// </summary>
        /// <exclude />
        public static object? AsHyperlink( this object? obj ) => FacadePropertyFormatter.CreateHyperlink( obj );
    }
}