// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Metalama.Framework.Engine.CodeModel;

internal static class DisplayStringFormatter
{
    // Seems that ReSharper does not recognize the interpolation handler.
    // ReSharper disable EntityNameCapturedOnly.Global
    public static string Format(
        CodeDisplayFormat? format,
        CodeDisplayContext? context,
        [InterpolatedStringHandlerArgument( nameof(format), nameof(context) )]
        ref InterpolatedStringHandler handler )
        => handler.ToString();
    
    // ReSharper enable EntityNameCapturedOnly.Global

    [InterpolatedStringHandler]
    public readonly ref struct InterpolatedStringHandler
    {
        private readonly CodeDisplayFormat? _format;
        private readonly CodeDisplayContext? _context;
        private readonly StringBuilder _stringBuilder;

        // ReSharper disable UnusedParameter.Local
        public InterpolatedStringHandler( int literalLength, int formattedCount, CodeDisplayFormat? format, CodeDisplayContext? context )
        {
            this._format = format;
            this._context = context;
            this._stringBuilder = new();
        }

        public void AppendLiteral( string s ) => this._stringBuilder.Append( s );

        public void AppendFormatted( IDisplayable displayable ) => this._stringBuilder.Append( displayable.ToDisplayString( this._format, this._context ) );

        public void AppendFormatted( IEnumerable<IDisplayable> collection )
        {
            var first = true;
            
            foreach ( var item in collection )
            {
                if ( !first )
                {
                    this._stringBuilder.Append( ", " );
                }

                first = false;

                this._stringBuilder.Append( item.ToDisplayString( this._format, this._context ) );
            }
        }

        public void AppendFormatted( string s ) => this._stringBuilder.Append( s );
        
        public override string ToString() => this._stringBuilder.ToString();
    }
}