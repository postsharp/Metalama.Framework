using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Caravela.Reactive.Implementation
{
    /// <summary>
    /// A container for a value that must be changed atomically.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct AtomicValue<T>
    {
        private static readonly bool _needsLock = TypeNeedsLock( typeof( T ) );

        private T _value;
        private SpinLock _lock;

        private static bool TypeNeedsLock( Type type )
        {
            return type.IsValueType && (type.IsGenericType || Marshal.SizeOf( type ) > IntPtr.Size);
        }

        public T Value
        {
            get
            {
                if ( _needsLock )
                {
                    return this._value;
                }
                else
                {
                    var lockTaken = false;

                    try
                    {

                        this._lock.TryEnter( ref lockTaken );
                        return this._value;
                    }
                    finally
                    {
                        if ( lockTaken )
                        {
                            this._lock.Exit();
                        }
                    }
                }
            }

            set
            {
                if ( _needsLock )
                {
                    var lockTaken = false;

                    try
                    {

                        this._lock.TryEnter( ref lockTaken );
                        this._value = value;
                    }
                    finally
                    {
                        if ( lockTaken )
                        {
                            this._lock.Exit();
                        }
                    }
                }
                else
                {
                    this._value = value;
                }
            }
        }
    }
}
