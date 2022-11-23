// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Project
{
    /// <summary>
    /// Base interface to be inherited by all types that want to be exposed to <see cref="IServiceProvider"/>.
    /// </summary>
    [CompileTime]
    public interface IService { }

    public interface IServiceProvider<TBase> : IServiceProvider
    {
        T? GetService<T>() where T : class, TBase;

    }
    
    public interface IProjectService { }

}