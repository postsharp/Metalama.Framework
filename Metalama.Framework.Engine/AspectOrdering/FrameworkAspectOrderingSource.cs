// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.AspectOrdering;

/// <summary>
/// An <see cref="IAspectOrderingSource"/> that orders aspects provided by <c>Metalama.Framework</c> before the other.
/// </summary>
internal class FrameworkAspectOrderingSource : IAspectOrderingSource
{
    private readonly ImmutableArray<AspectClass> _aspectTypes;

    public FrameworkAspectOrderingSource( ImmutableArray<AspectClass> aspectTypes )
    {
        this._aspectTypes = aspectTypes;
    }

    public IEnumerable<AspectOrderSpecification> GetAspectOrderSpecification( IDiagnosticAdder diagnosticAdder )
    {
        static bool IsFrameworkAspect( AspectClass t ) => t.Project?.RunTimeIdentity.Name == "Metalama.Framework";

        var frameworkAspects = this._aspectTypes.Where( IsFrameworkAspect ).ToList();

        foreach ( var aspectClass in this._aspectTypes )
        {
            if ( !IsFrameworkAspect( aspectClass ) )
            {
                foreach ( var frameworkAspect in frameworkAspects )
                {
                    yield return new AspectOrderSpecification( new[] { frameworkAspect.FullName, aspectClass.FullName } );
                }
            }
        }
    }
}