// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    internal static class NamespaceHelper
    {
        /// <summary>
        /// Returns the left part before the last '.' of a string.
        /// </summary>
        public static string GetNamespace( string fullName )
        {
            var index = fullName.LastIndexOf( '.' );

            if ( index >= 0 )
            {
                return fullName.Substring( 0, index - 1 );
            }
            else
            {
                return "";
            }
        }
    }
}