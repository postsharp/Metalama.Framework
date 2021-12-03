// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal sealed class SerializationBinaryWriter
    {
        private readonly BinaryWriter writer;
        private readonly Dictionary<string, int> strings = new Dictionary<string, int>( 64, StringComparer.Ordinal );
        private readonly Dictionary<string, int> dottedStrings = new Dictionary<string, int>( 64, StringComparer.Ordinal );

        public SerializationBinaryWriter( BinaryWriter writer )
        {
            this.writer = writer;
        }

        public void WriteCompressedInteger( Integer integer )
        {
            var value = integer.AbsoluteValue;
            var isNegative = integer.IsNegative;
            var signBit = (byte) (isNegative ? 0x80 : 0);

            // For unsigned compressed integers, the top 3 bits of the header are used to store the integer lenghts.
            if ( (value & 0x0f) == value )
            {
                this.writer.Write( (byte) (signBit | (byte) value) );
            }
            else if ( (value & 0x0fff) == value )
            {
                this.writer.Write( (byte) (0x10 | signBit | (byte) (value >> 8)) );
                this.writer.Write( (byte) (value & 0xff) );
            }
            else if ( (value & 0x0fffff) == value )
            {
                this.writer.Write( (byte) (0x20 | signBit | (byte) (value >> 16)) );
                this.writer.Write( (ushort) (value & 0xffff) );
            }
            else if ( (value & 0x0fffffffff) == value )
            {
                this.writer.Write( (byte) (0x30 | signBit | (byte) (value >> 32)) );
                this.writer.Write( (uint) (value & 0xffffffff) );
            }
            else
            {
                this.writer.Write( (byte) (0x40 | signBit) );
                this.writer.Write( value );
            }
        }

        public void WriteByte( byte value )
        {
            this.writer.Write( value );
        }

        public void WriteDouble( double value )
        {
            this.writer.Write( value );
        }

        public void WriteString( string value )
        {

            if ( value == null )
            {
                this.WriteCompressedInteger( -1 );
            }
            else if ( this.strings.TryGetValue( value, out var id ) )
            {
                this.WriteCompressedInteger( -id );
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes( value );
                this.WriteCompressedInteger( bytes.Length );
                this.writer.Write( bytes );
            }
        }

        public void WriteSByte( sbyte value )
        {
            this.writer.Write( value );
        }

        public void WriteDottedString( string value )
        {

            if ( value == null )
            {
                this.WriteCompressedInteger( -1 );
            }
            else if ( this.dottedStrings.TryGetValue( value, out var id ) )
            {
                this.WriteCompressedInteger( -id );
            }
            else
            {

                var lastDot = value.LastIndexOf( '.' );
                string name, scope;

                if ( lastDot < 0 )
                {
                    name = value;
                    scope = null;
                }
                else
                {
                    name = value.Substring( lastDot + 1 );
                    scope = value.Substring( 0, lastDot );
                }

                var bytes = Encoding.UTF8.GetBytes( name );
                this.WriteCompressedInteger( bytes.Length );
                this.writer.Write( bytes );

                this.WriteDottedString( scope );

                this.dottedStrings.Add( value, this.dottedStrings.Count + 2 );
            }
        }

        public void WriteSingle( float value )
        {
            this.writer.Write( value );
        }
    }
}