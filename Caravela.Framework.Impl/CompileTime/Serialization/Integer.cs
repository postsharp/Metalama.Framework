// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal struct Integer
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
            if(integer.IsNegative && integer.AbsoluteValue == 1 + (ulong)int.MaxValue)
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
                if (integer.IsNegative && integer.AbsoluteValue == 1+(ulong)long.MaxValue)
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
            if ( integer.IsNegative ) throw new InvalidCastException();

            checked
            {
                return (uint) integer.AbsoluteValue;
            }
        }

        public static implicit operator ushort( Integer integer )
        {
            if ( integer.IsNegative ) throw new InvalidCastException();

            checked
            {
                return (ushort) integer.AbsoluteValue;
            }
        }

        public static implicit operator ulong( Integer integer )
        {
            if ( integer.IsNegative ) throw new InvalidCastException();

            checked
            {
                return integer.AbsoluteValue;
            }
        }

        public static implicit operator Integer( int integer )
        {
            if ( integer < 0 )
            {
                if(integer == int.MinValue)
                {
                    return new Integer(checked(-(long)(int.MinValue)), true);
                }
                return new Integer( (ulong) (-integer), true );
            }
            else
                return new Integer( (ulong) integer, false );
        }

        public static implicit operator Integer( long integer )
        {
            if (integer == long.MinValue)
            {
                return new Integer(1+(ulong)long.MaxValue, true);
            }
            if ( integer < 0 )
                return new Integer( (ulong) (-integer), true );
            else
                return new Integer( (ulong) integer, false );
        }

        public static implicit operator Integer( short integer )
        {
            if ( integer < 0 )
                return new Integer( (ulong) (-integer), true );
            else
                return new Integer( (ulong) integer, false );
        }

        public static implicit operator Integer( uint integer )
        {
            return new Integer( integer, false );
        }

        public static implicit operator Integer( ulong integer )
        {
            return new Integer( integer, false );
        }

        public static implicit operator Integer( ushort integer )
        {
            return new Integer( integer, false );
        }
    }
}