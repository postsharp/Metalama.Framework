// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal static class DiagnosticDefinitionHelper
    {
        public static IEnumerable<IDiagnosticDefinition> GetDiagnosticDefinitions( params Type[] type )
            => type.Select( GetDefinitions<IDiagnosticDefinition> ).SelectMany( d => d );

        public static IEnumerable<SuppressionDefinition> GetSuppressionDefinitions( params Type[] type )
            => type.Select( GetDefinitions<SuppressionDefinition> ).SelectMany( d => d );

        private static IEnumerable<T> GetDefinitions<T>( Type declaringTypes )
            => declaringTypes.GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
                .Where( f => typeof(T).IsAssignableFrom( f.FieldType ) )
                .Select( f => (T) f.GetValue( null ) )
                .Concat(
                    declaringTypes.GetProperties( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
                        .Where( p => typeof(T).IsAssignableFrom( p.PropertyType ) )
                        .Select( p => (T) p.GetValue( null ) ) );
    }
}