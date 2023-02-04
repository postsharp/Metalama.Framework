// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// A base interface for all invokers, which are a mechanism to invoke the semantic of declarations and to specify which
    /// layer of overrides should be invoked.
    /// </summary>
    [CompileTime]
    [Obsolete]
    public interface IInvoker { }
}