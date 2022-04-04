// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Project
{
    /// <summary>
    /// Base interface to be inherited by all types that want to be exposed to <see cref="IServiceProvider"/>.
    /// </summary>
    [CompileTime]
    public interface IService { }
}