// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Code.Invokers;

/// <summary>
/// Options that influence the behavior of invokers, i.e. <see cref="IMethodInvoker"/>, <see cref="IFieldOrPropertyInvoker"/>,
/// <see cref="IEventInvoker"/> or <see cref="IIndexerInvoker"/>.
/// </summary>
[CompileTime]
[Flags]
public enum InvokerOptions
{
    /// <summary>
    /// By default, the invoker will generate a call to the implementation <i>before</i> the current aspect layer is applied,
    /// and will use the dot (<c>.</c>) operator for member access.
    /// </summary>
    Default,
    
    Base = Default,
    
    Before = Default,
    
    // TODO: implement 
    /// <summary>
    /// Causes the <i>current</i> implementation to be called, i.e. the implementation after all overrides by the current aspect. 
    /// </summary>
    Current = 1,
    
    /// <summary>
    /// Causes the <i>final</i> implementation to be called, i.e. the implementation after all overrides by aspects
    /// and, if the member is <c>virtual</c>, by derived classes through the <c>override</c> keyword. 
    /// </summary>
    Final = 2,
    
    OrderMask = Base | Final,
    
    /// <summary>
    /// Specifies that the null-conditional access operator (<c>?.</c> aka Elvis) has to be used instead of the dot operator. 
    /// </summary>
    NullConditional = 1024
}