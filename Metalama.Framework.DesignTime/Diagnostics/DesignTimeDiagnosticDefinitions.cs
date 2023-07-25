// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.DesignTime.Services;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Diagnostics
{
    public sealed class DesignTimeDiagnosticDefinitions
    {
        private static DesignTimeDiagnosticDefinitions? _instance;

        /// <summary>
        /// Gets the set of <see cref="DiagnosticDescriptor"/> that are supported in the current design-time session.
        /// </summary>
        internal ImmutableDictionary<string, DiagnosticDescriptor> SupportedDiagnosticDescriptors { get; }

        internal ImmutableDictionary<string, DiagnosticDescriptor> UserDiagnosticDescriptors { get; }

        /// <summary>
        /// Gets the set of <see cref="SuppressionDescriptor"/> that are supported the current design-time session.
        /// </summary>
        internal ImmutableDictionary<string, SuppressionDescriptor> SupportedSuppressionDescriptors { get; }

        // ReSharper disable once RedundantSuppressNullableWarningExpression
        public static DesignTimeDiagnosticDefinitions GetInstance()
            => LazyInitializer.EnsureInitialized( ref _instance, () => new DesignTimeDiagnosticDefinitions() )!;

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

        private DesignTimeDiagnosticDefinitions()
        {
            var userDefinedDescriptors = DesignTimeServiceProviderFactory.GetSharedServiceProvider()
                .GetRequiredService<UserDiagnosticRegistrationService>()
                .GetSupportedDescriptors();

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