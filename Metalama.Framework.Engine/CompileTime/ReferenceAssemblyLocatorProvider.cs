// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// A global service that provides an instance of <see cref="ReferenceAssemblyLocator"/>, which itself is
/// a project-scoped service but that can and should be shared among projects that have the same <see cref="IProjectOptions.CompileTimePackages"/>.
/// </summary>
internal sealed class ReferenceAssemblyLocatorProvider : IGlobalService
{
    private readonly object _sync = new();

    private volatile ImmutableDictionary<string, ReferenceAssemblyLocator> _referenceAssemblyLocators =
        ImmutableDictionary<string, ReferenceAssemblyLocator>.Empty;

    public ReferenceAssemblyLocator GetInstance( ProjectServiceProvider serviceProvider )
    {
        var projectOptions = serviceProvider.GetRequiredService<IProjectOptions>();

        var additionalPackageReferences = projectOptions.CompileTimePackages.IsDefaultOrEmpty
            ? string.Empty
            : ReferenceAssemblyLocator.GetAdditionalPackageReferences( projectOptions );
        
        if ( !this._referenceAssemblyLocators.TryGetValue( additionalPackageReferences, out var referenceAssemblyLocator ) )
        {
            // We lock instead of using ConcurrentDictionary because instantiating the class is expensive.
            lock ( this._sync )
            {
                referenceAssemblyLocator = new ReferenceAssemblyLocator( serviceProvider, additionalPackageReferences );
                this._referenceAssemblyLocators = this._referenceAssemblyLocators.Add( additionalPackageReferences, referenceAssemblyLocator );
            }
        }

        return referenceAssemblyLocator;
    }
}