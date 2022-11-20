// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Linq;

namespace Metalama.Framework.Engine.Utilities.Diagnostics
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
                var lines = exception.ToString().Split( '\n' ).SelectArray( l => l.TrimEnd() ).ToMutableList();
                lines.RemoveRange( lines.Count - removeLastStackFrames, removeLastStackFrames );

                return string.Join( Environment.NewLine, lines );
            }
        }
    }
}