// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline;

public readonly struct FallibleResult<T>
{
    private readonly T _result;

    public bool IsSuccess { get; }

    public T Value => this.IsSuccess ? this._result : throw new InvalidOperationException( "Cannot get the result of the operation because it failed." );

    public static FallibleResult<T> Failed => new( default!, false );

    public static FallibleResult<T> Succeeded( T value ) => new( value, true );

    public static implicit operator FallibleResult<T>( T value ) => new( value, true );

    private FallibleResult( T result, bool isSuccess )
    {
        this._result = result;
        this.IsSuccess = isSuccess;
    }

    public override string ToString() => this.IsSuccess ? this._result?.ToString() ?? "null" : "<Failed>";
}

public readonly struct FallibleResultWithDiagnostics<T>
{
    private readonly T _result;

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public bool IsSuccess { get; }

    public T Value => this.IsSuccess ? this._result : throw new InvalidOperationException( "Cannot get the result of the operation because it failed." );

    public static FallibleResultWithDiagnostics<T> Failed( ImmutableArray<Diagnostic> diagnostics ) => new( default!, false, diagnostics );

    public static FallibleResultWithDiagnostics<T> Succeeded( T value, ImmutableArray<Diagnostic> diagnostics = default ) => new( value, true, diagnostics );

    public static implicit operator FallibleResultWithDiagnostics<T>( T value ) => new( value, true, default );

    private FallibleResultWithDiagnostics( T result, bool isSuccess, ImmutableArray<Diagnostic> diagnostics )
    {
        this._result = result;
        this.IsSuccess = isSuccess;
        this.Diagnostics = diagnostics.IsDefault ? ImmutableArray<Diagnostic>.Empty : diagnostics;
    }

    public override string ToString() => this.IsSuccess ? this._result?.ToString() ?? "null" : "<Failed>";
}