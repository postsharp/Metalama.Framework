// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Options;

/// <summary>
/// Extensions of the <see cref="IOverridable"/> interface.
/// </summary>
[CompileTime]
public static class OverridableExtensions
{
    /// <summary>
    /// Invokes <see cref="IOverridable.OverrideWith"/> in a type- and nullable-safe way.
    /// </summary>
    public static T? OverrideWithSafe<T>( this T? baseOptions, T? overrideOptions, in OverrideContext context )
        where T : class, IOverridable
    {
        if ( baseOptions == null )
        {
            return overrideOptions;
        }
        else if ( overrideOptions == null )
        {
            return baseOptions;
        }
        else
        {
            return (T) baseOptions.OverrideWith( overrideOptions, context );
        }
    }
    
    /// <summary>
    /// Invokes <see cref="IOverridable.OverrideWith"/> in a type--safe way.
    /// </summary>
    public static T OverrideWith<T>( this T baseOptions, T overrideOptions, in OverrideContext context )
        where T : class, IOverridable
        => (T) baseOptions.OverrideWith( overrideOptions, context );
}