// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.Utilities.Caching;

internal class RecyclableObjectPool<T> : ObjectPool<T>
    where T : class, IRecyclable
{
    internal RecyclableObjectPool( Func<T> factory, bool trimOnFree = true ) : base( factory, trimOnFree ) { }

    internal RecyclableObjectPool( Func<T> factory, int size, bool trimOnFree = true ) : base( factory, size, trimOnFree ) { }

    internal RecyclableObjectPool( Func<ObjectPool<T>, T> factory, int size ) : base( factory, size ) { }

    protected override void Recycle( T obj ) => obj.Recycle();

    protected override void CleanUp( T obj ) => obj.CleanUp();
}