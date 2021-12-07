// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Impl.CodeModel
{
    internal static class MemberWithAccessorsImpl
    {
        public static IMethod? GetAccessorImpl( this IEvent @event, MethodKind kind )
            => kind switch
            {
                MethodKind.EventAdd => @event.AddMethod,
                MethodKind.EventRaise => @event.RaiseMethod,
                MethodKind.EventRemove => @event.RemoveMethod,
                _ => null
            };

        public static IMethod? GetAccessorImpl( this IFieldOrProperty fieldOrProperty, MethodKind kind )
            => kind switch
            {
                MethodKind.PropertyGet => fieldOrProperty.GetMethod,
                MethodKind.PropertySet => fieldOrProperty.SetMethod,
                _ => null
            };
    }
}