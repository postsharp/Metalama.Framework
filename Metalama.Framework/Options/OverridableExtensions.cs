// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Options;

[CompileTime]
public static class OverridableExtensions
{
    public static T? OverrideWithSafe<T>( this T? baseOptions, T? overrideOptions, in HierarchicalOptionsOverrideContext context )
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
}