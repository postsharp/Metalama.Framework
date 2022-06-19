// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Diagnostics
{
    internal class DesignTimeDiagnosticDefinitions
    {
        private static DesignTimeDiagnosticDefinitions? _instance;

        /// <summary>
        /// Gets the set of <see cref="DiagnosticDescriptor"/> that are supported in the current design-time session.
        /// </summary>
        public ImmutableDictionary<string, DiagnosticDescriptor> SupportedDiagnosticDescriptors { get; }

        public ImmutableDictionary<string, DiagnosticDescriptor> UserDiagnosticDescriptors { get; }

        /// <summary>
        /// Gets the set of <see cref="SuppressionDescriptor"/> that are supported the current design-time session.
        /// </summary>
        public ImmutableDictionary<string, SuppressionDescriptor> SupportedSuppressionDescriptors { get; }

        public static DesignTimeDiagnosticDefinitions GetInstance()
            => LazyInitializer.EnsureInitialized( ref _instance, () => new DesignTimeDiagnosticDefinitions() )!;

        /// <summary>
        /// Gets the set of <see cref="DiagnosticDescriptor"/> that are defined by Metalama itself.
        /// </summary>
        public static ImmutableDictionary<string, DiagnosticDescriptor> StandardDiagnosticDescriptors { get; } = new DiagnosticDefinitionDiscoveryService()
            .GetDiagnosticDefinitions(
                typeof(TemplatingDiagnosticDescriptors),
                typeof(DesignTimeDiagnosticDescriptors),
                typeof(GeneralDiagnosticDescriptors),
                typeof(SerializationDiagnosticDescriptors),
                typeof(AdviceDiagnosticDescriptors),
                typeof(AspectLinkerDiagnosticDescriptors) )
            .Select( d => d.ToRoslynDescriptor() )
            .ToImmutableDictionary( d => d.Id, d => d, StringComparer.CurrentCultureIgnoreCase );

        private DesignTimeDiagnosticDefinitions()
        {
            var directoryOptions = ServiceProviderFactory.GetServiceProvider().GetRequiredService<IPathOptions>();
            var userDefinedDescriptors = UserDiagnosticRegistrationService.GetInstance( directoryOptions ).GetSupportedDescriptors();

            // The file may contain system descriptors by mistake. We must remove them otherwise we will have some duplicate key issue.

            this.SupportedDiagnosticDescriptors =
                StandardDiagnosticDescriptors.Values
                    .Concat( userDefinedDescriptors.Diagnostics.Where( d => !StandardDiagnosticDescriptors.ContainsKey( d.Id ) ) )
                    .ToImmutableDictionary( d => d.Id, d => d, StringComparer.OrdinalIgnoreCase );

            this.UserDiagnosticDescriptors = userDefinedDescriptors.Diagnostics.ToImmutableDictionary( d => d.Id, d => d, StringComparer.OrdinalIgnoreCase );

            this.SupportedSuppressionDescriptors =
                userDefinedDescriptors.Suppressions.ToImmutableDictionary( d => d.SuppressedDiagnosticId, d => d, StringComparer.OrdinalIgnoreCase );
        }
    }
}