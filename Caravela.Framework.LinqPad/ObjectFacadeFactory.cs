// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using LINQPad;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.LinqPad
{
    internal static class ObjectFacadeFactory
    {
        private static readonly ConditionalWeakTable<object, ObjectFacade> _instances = new();

        internal static ObjectFacade? GetFacade( object? instance )
        {
            var isInlineType = instance == null || instance is IEnumerable || instance is string || instance.GetType().IsPrimitive
                               || instance is DumpContainer || instance is Hyperlinq;

            if ( isInlineType )
            {
                return null;
            }
            else
            {
                if ( !_instances.TryGetValue( instance!, out var proxy ) )
                {
                    proxy = new ObjectFacade( ObjectFacadeType.GetFormatterType( instance!.GetType() ), instance );
                    _instances.AddOrUpdate( instance, proxy );
                }

                return proxy;
            }
        }
    }
}