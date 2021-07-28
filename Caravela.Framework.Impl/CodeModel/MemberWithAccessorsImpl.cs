// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class MemberWithAccessorsImpl
    {
        public static IMethod? GetAccessorImpl( this IEvent @event, MethodKind kind )
            => kind switch
            {
                MethodKind.EventAdd => @event.Adder,
                MethodKind.EventRaise => @event.Raiser,
                MethodKind.EventRemove => @event.Remover,
                _ => null
            };

        public static IMethod? GetAccessorImpl( this IProperty property, MethodKind kind )
            => kind switch
            {
                MethodKind.PropertyGet => property.Getter,
                MethodKind.PropertySet => property.Setter,
                _ => null
            };
    }
}