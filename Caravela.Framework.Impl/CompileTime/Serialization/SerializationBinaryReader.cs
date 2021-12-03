// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal sealed class SerializationBinaryReader
    {
        private readonly BinaryReader reader;
        private readonly Dictionary<int, string> strings = new Dictionary<int, string>();
        private readonly Dictionary<int, string> dottedStrings = new Dictionary<int, string>();

        public SerializationBinaryReader( BinaryReader reader )
        {
            this.reader = reader;
        }

        public byte ReadByte()
        {
            return this.reader.ReadByte();
        }


        public string ReadString()
        {
            int header = this.ReadCompressedInteger();

            if ( header == -1 )
                return null;

            else if ( header < 0 )
            {
                string s;
                if ( !this.strings.TryGetValue( header, out s ) )
                    throw new MetaSerializationException( "Invalid serialized stream: invalid string identifier." );
                return s;
            }
            else
            {
                var bytes = this.reader.ReadBytes( header );
                var value = Encoding.UTF8.GetString( bytes, 0, bytes.Length );
                this.strings.Add( this.strings.Count + 1, value );
                return value;
            }
        }

        public DottedString ReadDottedString()
        {
            int header = this.ReadCompressedInteger();

            if (header == -1)
            {
                return null;
            }
            else if (header < 0)
            {
                string s;
                if (!this.dottedStrings.TryGetValue(-header, out s))
                    throw new MetaSerializationException( "Invalid serialized stream: invalid string identifier.");
                return s;
            }
            else
            {
                var bytes = this.reader.ReadBytes(header);
                var value = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                var parent = this.ReadDottedString();

                if (!parent.IsNull)
                {
                    value = parent + "." + value;
                }

                this.dottedStrings.Add(this.dottedStrings.Count + 2, value);
                return value;
            }
        }

        public Integer ReadCompressedInteger()
        {
            var header = this.reader.ReadByte();
            var isNegative = (header & 0x80) != 0;
            ulong value;

            switch ( header & 0x70 )
            {
                    // Unsigned
                case 0x00:
                    value = (ulong) (header & 0x0F);
                    break;

                case 0x10:
                    value = (uint) (header & 0x0F) << 8 | this.reader.ReadByte();
                    break;

                case 0x20:
                    value = (uint) (header & 0x0F) << 16 | this.reader.ReadUInt16();
                    break;

                case 0x30:
                    value = (ulong) (header & 0x0F) << 32 | this.reader.ReadUInt32();
                    break;

                case 0x40:
                    value = ((ulong) (header & 0x0F) << 64) | this.reader.ReadUInt64();
                    break;

                default:
                    throw new MetaSerializationException();
            }

            return new Integer( value, isNegative );
        }


        public double ReadDouble()
        {
            return this.reader.ReadDouble();
        }

        public float ReadSingle()
        {
            return this.reader.ReadSingle();
        }

        public sbyte ReadSByte()
        {
            return this.reader.ReadSByte();
        }
    }
}