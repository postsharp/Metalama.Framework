// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using System;

namespace Metalama.Framework.Engine.Advising;

internal class OverrideAccessorAdviceResult<T> : AdviceResult, IOverrideAdviceResult<IMethod> 
    where T : class, ICompilationElement
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