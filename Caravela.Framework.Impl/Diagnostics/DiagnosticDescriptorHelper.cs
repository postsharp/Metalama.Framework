// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal static class DiagnosticDescriptorHelper
    {
        public static IEnumerable<DiagnosticDescriptor> GetDiagnosticDescriptors( params Type[] type )
            => type.Select( GetDiagnosticDescriptors ).SelectMany( d => d );

        public static IEnumerable<DiagnosticDescriptor> GetDiagnosticDescriptors( Type type )
            => type.GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
                .Where( f => typeof(IStrongDiagnosticDescriptor).IsAssignableFrom( f.FieldType ) )
                .Select( f => (IStrongDiagnosticDescriptor) f.GetValue( null ) )
                .Select( d => d.ToDiagnosticDescriptor() );
    }
}