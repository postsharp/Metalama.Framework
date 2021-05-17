// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static class DesignTimeDiagnosticDefinitions
    {
        public static ImmutableArray<DiagnosticDescriptor> SupportedDiagnosticDescriptors { get; }

        public static ImmutableHashSet<string> SupportedDiagnosticIds { get; }

        static DesignTimeDiagnosticDefinitions()
        {
            CompilerServiceProvider.Initialize();

            SupportedDiagnosticDescriptors = DiagnosticDefinitionHelper
                .GetDiagnosticDefinitions(
                    typeof(TemplatingDiagnosticDescriptors),
                    typeof(DesignTimeDiagnosticDescriptors),
                    typeof(GeneralDiagnosticDescriptors),
                    typeof(SerializationDiagnosticDescriptors) )
                .Select( d => d.ToRoslynDescriptor() )
                .ToImmutableArray();

            SupportedDiagnosticIds = SupportedDiagnosticDescriptors.Select( x => x.Id ).ToImmutableHashSet();
        }

        private static readonly string[] _supportedSuppressions = { "CS1998", "IDE0051" };

        // TODO: This is a temporary hack to statically declare suppressions supported in the sample app.

        public static ImmutableDictionary<string, SuppressionDescriptor> SupportedSuppressionDescriptors { get; } = _supportedSuppressions.Select(
                id => new KeyValuePair<string, SuppressionDescriptor>(
                    "Caravela." + id,
                    new SuppressionDescriptor( "Caravela." + id, id, "Caravela" ) ) )
            .ToImmutableDictionary();

        public static bool IsSuppressionSupported( string id ) => SupportedSuppressionDescriptors.ContainsKey( id );
    }
}