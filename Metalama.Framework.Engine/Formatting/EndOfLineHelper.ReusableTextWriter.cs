// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;
using System.Text;

namespace Metalama.Framework.Engine.Formatting
{
    internal partial class EndOfLineHelper
    {
        /// <summary>
        /// Implements an allocation-less text writer that writes into a string intended for repeated use.
        /// </summary>
        private class ReusableTextWriter : TextWriter
        {
            private char[] _data = new char[64];
            private int _offset;

            public override Encoding Encoding => Encoding.Unicode;

            public Span<char> Data => this._data.AsSpan().Slice( 0, this._offset );

            public override void Write( char value )
            {
                if ( this._offset >= this._data.Length )
                {
                    var newData = new char[this._data.Length * 2];
                    Buffer.BlockCopy( this._data, 0, newData, 0, this._offset );
                    this._data = newData;
                }

                this._data[this._offset++] = value;
            }

            public void Write( Span<char> chars, int start, int length )
            {
                // TODO: This is not optimal.
                chars = chars.Slice( start, length );

                for ( var i = 0; i < chars.Length; i++ )
                {
                    this.Write( chars[i] );
                }
            }

            public void Reset()
            {
                this._offset = 0;
            }
        }
    }
}