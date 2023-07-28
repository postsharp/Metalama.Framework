// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Diagnostics
{
    internal sealed class DesignTimeDiagnosticDefinitions
    {
        public ImmutableDictionary<string, DiagnosticDescriptor> SupportedDiagnosticDescriptors { get; }

        public ImmutableDictionary<string, DiagnosticDescriptor> UserDiagnosticDescriptors { get; }

        public ImmutableDictionary<string, SuppressionDescriptor> SupportedSuppressionDescriptors { get; }

        /// <summary>
        /// Gets the set of <see cref="DiagnosticDescriptor"/> that are defined by Metalama itself.
        /// </summary>
        internal static ImmutableDictionary<string, DiagnosticDescriptor> StandardDiagnosticDescriptors { get; } = new DiagnosticDefinitionDiscoveryService()
            .GetDiagnosticDefinitions(
                typeof(GeneralDiagnosticDescriptors),
                typeof(TemplatingDiagnosticDescriptors),
                typeof(SerializationDiagnosticDescriptors),
                typeof(DesignTimeDiagnosticDescriptors),
                typeof(AttributeDeserializerDiagnostics),
                typeof(AdviceDiagnosticDescriptors),
                typeof(AspectLinkerDiagnosticDescriptors),
                typeof(FrameworkDiagnosticDescriptors),
                typeof(LicensingDiagnosticDescriptors) )
            .Select( d => d.ToRoslynDescriptor() )
            .ToImmutableDictionary( d => d.Id, d => d, StringComparer.CurrentCultureIgnoreCase );

        internal DesignTimeDiagnosticDefinitions(
            ImmutableArray<DiagnosticDescriptor> diagnosticDescriptors,
            ImmutableArray<SuppressionDescriptor> suppressionDescriptors )
        {
            // The file may contain system descriptors by mistake. We must remove them otherwise we will have some duplicate key issue.

            this.SupportedDiagnosticDescriptors =
                StandardDiagnosticDescriptors.Values
                    .Concat( diagnosticDescriptors.Where( d => !StandardDiagnosticDescriptors.ContainsKey( d.Id ) ) )
                    .ToImmutableDictionary( d => d.Id, d => d, StringComparer.OrdinalIgnoreCase );

            this.UserDiagnosticDescriptors = diagnosticDescriptors.ToImmutableDictionary( d => d.Id, d => d, StringComparer.OrdinalIgnoreCase );

            this.SupportedSuppressionDescriptors =
                suppressionDescriptors.ToImmutableDictionary( d => d.SuppressedDiagnosticId, d => d, StringComparer.OrdinalIgnoreCase );
        }
    }
}