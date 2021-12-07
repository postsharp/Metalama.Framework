// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Metalama.Framework.Impl.CompileTime.Serialization
{
    internal sealed class SerializationBinaryReader
    {
        private readonly BinaryReader _reader;
        private readonly Dictionary<int, string> _strings = new();
        private readonly Dictionary<int, string> _dottedStrings = new();

        public SerializationBinaryReader( BinaryReader reader )
        {
            this._reader = reader;
        }

        public byte ReadByte()
        {
            return this._reader.ReadByte();
        }

        public string? ReadString()
        {
            int header = this.ReadCompressedInteger();

            if ( header == -1 )
            {
                return null;
            }
            else if ( header < 0 )
            {
                if ( !this._strings.TryGetValue( header, out var s ) )
                {
                    throw new MetaSerializationException( "Invalid serialized stream: invalid string identifier." );
                }

                return s;
            }
            else
            {
                var bytes = this._reader.ReadBytes( header );
                var value = Encoding.UTF8.GetString( bytes, 0, bytes.Length );
                this._strings.Add( this._strings.Count + 1, value );

                return value;
            }
        }

        public DottedString ReadDottedString()
        {
            int header = this.ReadCompressedInteger();

            if ( header == -1 )
            {
                return DottedString.Null;
            }
            else if ( header < 0 )
            {
                if ( !this._dottedStrings.TryGetValue( -header, out var s ) )
                {
                    throw new MetaSerializationException( "Invalid serialized stream: invalid string identifier." );
                }

                return s;
            }
            else
            {
                var bytes = this._reader.ReadBytes( header );
                var value = Encoding.UTF8.GetString( bytes, 0, bytes.Length );
                var parent = this.ReadDottedString();

                if ( !parent.IsNull )
                {
                    value = parent + "." + value;
                }

                this._dottedStrings.Add( this._dottedStrings.Count + 2, value );

                return value;
            }
        }

        public Integer ReadCompressedInteger()
        {
            var header = this._reader.ReadByte();
            var isNegative = (header & 0x80) != 0;
            ulong value;

            switch ( header & 0x70 )
            {
                // Unsigned
                case 0x00:
                    value = (ulong) (header & 0x0F);

                    break;

                case 0x10:
                    value = ((uint) (header & 0x0F) << 8) | this._reader.ReadByte();

                    break;

                case 0x20:
                    value = ((uint) (header & 0x0F) << 16) | this._reader.ReadUInt16();

                    break;

                case 0x30:
                    value = ((ulong) (header & 0x0F) << 32) | this._reader.ReadUInt32();

                    break;

                case 0x40:
                    value = ((ulong) (header & 0x0F) << 64) | this._reader.ReadUInt64();

                    break;

                default:
                    throw new MetaSerializationException();
            }

            return new Integer( value, isNegative );
        }

        public double ReadDouble()
        {
            return this._reader.ReadDouble();
        }

        public float ReadSingle()
        {
            return this._reader.ReadSingle();
        }

        public sbyte ReadSByte()
        {
            return this._reader.ReadSByte();
        }
    }
}