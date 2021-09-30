// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Project
{
    /// <summary>
    /// Base interface to be inherited by all interfaces that want to be exposed to <see cref="IServiceProvider"/>.
    /// </summary>
    [CompileTimeOnly]
    public interface IService { }
}