// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Caravela.LinqPad
{
    /// <summary>
    /// Gets <see cref="FacadeObject"/> instances for any given input object.
    /// </summary>
    internal static class FacadeObjectFactory
    {
        private static readonly ConditionalWeakTable<object, FacadeObject> _instances = new();

        internal static FacadeObject? GetFacade( object? instance )
        {
            var isInlineType = instance == null || instance is IEnumerable || instance is string || instance.GetType().IsPrimitive
                               || (instance.GetType().Assembly.FullName is { } fullName && fullName.StartsWith( "LINQPad", StringComparison.OrdinalIgnoreCase ));

            if ( isInlineType )
            {
                return null;
            }
            else
            {
                if ( !_instances.TryGetValue( instance!, out var proxy ) )
                {
                    proxy = new FacadeObject( FacadeType.GetFormatterType( instance!.GetType() ), instance );
                    _instances.AddOrUpdate( instance, proxy );
                }

                return proxy;
            }
        }
    }
}