// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;

namespace Metalama.Framework.Impl.Utilities
{
    internal static class ExceptionHelper
    {
        public static string Format( this Exception exception, int removeLastStackFrames = 0 )
        {
            if ( removeLastStackFrames == 0 )
            {
                return exception.ToString();
            }
            else
            {
                // Removes the last frames. Typically those would be obfuscated and offer no meaningful information to the user.
                var lines = exception.ToString().Split( '\n' ).Select( l => l.TrimEnd() ).ToList();
                lines.RemoveRange( lines.Count - removeLastStackFrames, removeLastStackFrames );

                return string.Join( Environment.NewLine, lines );
            }
        }
    }
}