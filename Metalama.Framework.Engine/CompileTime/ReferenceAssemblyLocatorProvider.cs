// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// A global services that provides a singleton instance of <see cref="ReferenceAssemblyLocator"/>, which itself is
/// a project-scoped service but that can and should be shared among all projects. It only needs an arbitrary project context for
/// initialization.
/// </summary>
internal class ReferenceAssemblyLocatorProvider : IGlobalService
{
    private volatile ReferenceAssemblyLocator? _instance;
    private object _sync = new();

    public ReferenceAssemblyLocator GetInstance( ProjectServiceProvider serviceProvider )
    {
        if ( this._instance == null )
        {
            // We lock instead of doing an interlocked operation because instantiating the class is expensive.
            lock ( this._sync )
            {
                if ( this._instance == null )
                {
                    this._instance = new ReferenceAssemblyLocator( serviceProvider );
                }
            }
        }

        return this._instance;
    }
}