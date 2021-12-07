// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Impl.Utilities
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