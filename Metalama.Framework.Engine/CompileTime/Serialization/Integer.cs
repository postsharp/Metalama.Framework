// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.CompileTime.Serialization
{
    internal readonly struct Integer
    {
        public Integer( ulong absoluteValue, bool isNegative )
        {
            this.AbsoluteValue = absoluteValue;
            this.IsNegative = isNegative;
        }

        public readonly ulong AbsoluteValue;
        public readonly bool IsNegative;

        public static implicit operator int( Integer integer )
        {
            // NOTE: in case of absolute value == abs(int.MinVal); casting such a value directly to a (positive) int throws OverflowException
            if ( integer is { IsNegative: true, AbsoluteValue: 1 + (ulong) int.MaxValue } )
            {
                return int.MinValue;
            }

            checked
            {
                return integer.IsNegative ? -(int) integer.AbsoluteValue : (int) integer.AbsoluteValue;
            }
        }

        public static implicit operator long( Integer integer )
        {
            checked
            {
                if ( integer is { IsNegative: true, AbsoluteValue: 1 + (ulong) long.MaxValue } )
                {
                    return long.MinValue;
                }

                return integer.IsNegative ? -(long) integer.AbsoluteValue : (long) integer.AbsoluteValue;
            }
        }

        public static implicit operator short( Integer integer )
        {
            checked
            {
                return integer.IsNegative ? (short) -(long) integer.AbsoluteValue : (short) integer.AbsoluteValue;
            }
        }

        public static implicit operator uint( Integer integer )
        {
            if ( integer.IsNegative )
            {
                throw new InvalidCastException();
            }

            checked
            {
                return (uint) integer.AbsoluteValue;
            }
        }

        public static implicit operator ushort( Integer integer )
        {
            if ( integer.IsNegative )
            {
                throw new InvalidCastException();
            }

            checked
            {
                return (ushort) integer.AbsoluteValue;
            }
        }

        public static implicit operator ulong( Integer integer )
        {
            if ( integer.IsNegative )
            {
                throw new InvalidCastException();
            }

            return integer.AbsoluteValue;
        }

        public static implicit operator Integer( int integer )
            => integer switch
            {
                int.MinValue => new Integer( checked(-(long) int.MinValue), true ),
                < 0 => new Integer( (ulong) -integer, true ),
                _ => new Integer( (ulong) integer, false )
            };

        public static implicit operator Integer( long integer )
            => integer switch
            {
                long.MinValue => new Integer( 1 + (ulong) long.MaxValue, true ),
                < 0 => new Integer( (ulong) -integer, true ),
                _ => new Integer( (ulong) integer, false )
            };

        public static implicit operator Integer( short integer )
            => integer switch
            {
                < 0 => new Integer( (ulong) -integer, true ),
                _ => new Integer( (ulong) integer, false )
            };

        public static implicit operator Integer( uint integer ) => new( integer, false );

        public static implicit operator Integer( ulong integer ) => new( integer, false );

        public static implicit operator Integer( ushort integer ) => new( integer, false );
    }
}