// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Impl
{
    internal static class DiagnosticHelper
    {
        public static IEnumerable<DiagnosticDescriptor> GetDiagnosticDescriptors( Type type )
        {
            foreach ( var field in type.GetFields( BindingFlags.Public | BindingFlags.Static ) )
            {
                if ( field.FieldType == typeof(DiagnosticDescriptor) )
                {
                    yield return (DiagnosticDescriptor) field.GetValue( null );
                }
            }
        }
    }
}