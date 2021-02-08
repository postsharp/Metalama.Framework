using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal static class DiagnosticHelper
    {
        public static IEnumerable<DiagnosticDescriptor> GetDiagnosticDescriptors( Type type )
        {
            foreach ( var field in type.GetFields( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static ) )
            {
                if ( field.FieldType == typeof( DiagnosticDescriptor ) )
                {
                    yield return (DiagnosticDescriptor) field.GetValue( null );
                }
            }
        }
    }
}
