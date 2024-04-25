// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.Advising;

/// <summary>
/// An <see cref="AdviceResult"/> that does not have any property.
/// </summary>
internal class OverrideMemberAdviceResult<T> : AdviceResult, IOverrideAdviceResult<T>
    where T : class, ICompilationElement
{
    private readonly IRef<T>? _declaration;

    // Errpr constructor.
    public OverrideMemberAdviceResult() { }

    // Success constructor.
    public OverrideMemberAdviceResult( IRef<T>? declaration )
    {
        this._declaration = declaration;
    }

    public T Declaration => this.Resolve( this._declaration );

    public OverrideAccessorAdviceResult<T> GetAccessor( Func<T, IMethod?> getAccessor ) => new( this, getAccessor );
}

internal class OverrideAccessorAdviceResult<T> : AdviceResult, IOverrideAdviceResult<IMethod> where T : class, ICompilationElement
{
    private readonly OverrideMemberAdviceResult<T> _underlying;
    private readonly Func<T, IMethod?> _getMethod;

    public OverrideAccessorAdviceResult( OverrideMemberAdviceResult<T> underlying, Func<T, IMethod?> getMethod )
    {
        this._underlying = underlying;
        this._getMethod = getMethod;
    }

    [Memo]
    public IMethod Declaration => this._getMethod( this._underlying.Declaration ).AssertNotNull();
}