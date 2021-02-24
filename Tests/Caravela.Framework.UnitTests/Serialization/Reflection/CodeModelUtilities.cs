// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using Caravela.Framework.Code;

namespace Caravela.Framework.UnitTests.Templating.Serialization.Reflection
{
    public static class CodeModelUtilities
    {
        public static IMethod Method( this INamedType type, string name )
        {
            return type.Methods.Single( m => m.Name == name );
        }

        public static IProperty Property( this INamedType type, string name )
        {
            return type.Properties.Single( m => m.Name == name );
        }

        public static IEvent Event( this INamedType type, string name )
        {
            return type.Events.Single( m => m.Name == name );
        }
    }
}