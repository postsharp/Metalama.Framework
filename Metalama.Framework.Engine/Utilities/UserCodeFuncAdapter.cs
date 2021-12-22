// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.Utilities;

/// <summary>
/// Wraps a <see cref="Func{TResult}"/> into a <see cref="UserCodeFunc{TResult,TPayload}"/>.
/// </summary>
internal struct UserCodeFuncAdapter<T>
{
    // Intentionally not marking the struct as readonly to avoid defensive copies when passing by ref.

    private static readonly UserCodeFunc<T, UserCodeFuncAdapter<T>> _cachedDelegate = Func;

    private static T Func( ref UserCodeFuncAdapter<T> payload ) => payload._func();

    private readonly Func<T> _func;

    public UserCodeFuncAdapter( Func<T> func )
    {
        this._func = func;
    }

#pragma warning disable CA1822
    public UserCodeFunc<T, UserCodeFuncAdapter<T>> UserCodeFunc => _cachedDelegate;
#pragma warning restore CA1822
}