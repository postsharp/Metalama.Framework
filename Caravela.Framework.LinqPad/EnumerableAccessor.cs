// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Caravela.Framework.LinqPad
{
    internal class EnumerableAccessor
    {
        private readonly MethodInfo? _getter;
        private static readonly ConcurrentDictionary<Type, EnumerableAccessor> _instances = new();

        public static EnumerableAccessor? Get( Type type ) => _instances.GetOrAdd( type, t => new EnumerableAccessor( t ) );

        private EnumerableAccessor( Type type )
        {
            this._getter = type.GetProperty( "Count", BindingFlags.Instance | BindingFlags.Public )?.GetMethod
                           ?? type.GetProperty( "Length", BindingFlags.Instance | BindingFlags.Public )?.GetMethod;

            if ( this._getter != null && this._getter.ReturnType != typeof(int) )
            {
                this._getter = null;
            }
        }

        public bool HasCount => this._getter != null;

        public int GetCount( object obj ) => (int) this._getter.AssertNotNull().Invoke( obj, null )!;
    }
}