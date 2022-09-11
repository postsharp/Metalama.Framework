// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel
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