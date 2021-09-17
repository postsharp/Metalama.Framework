// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.Diagnostics
{
    internal class DesignTimeDiagnosticDefinitions
    {
        private static DesignTimeDiagnosticDefinitions? _instance;

        /// <summary>
        /// Gets the set of <see cref="DiagnosticDescriptor"/> that are supported in the current design-time session.
        /// </summary>
        public ImmutableDictionary<string, DiagnosticDescriptor> SupportedDiagnosticDescriptors { get; }

        /// <summary>
        /// Gets the set of <see cref="SuppressionDescriptor"/> that are supported the current design-time session.
        /// </summary>
        public ImmutableDictionary<string, SuppressionDescriptor> SupportedSuppressionDescriptors { get; }

        public static DesignTimeDiagnosticDefinitions GetInstance()
            => LazyInitializer.EnsureInitialized( ref _instance, () => new DesignTimeDiagnosticDefinitions() )!;

        /// <summary>
        /// Gets the set of <see cref="DiagnosticDescriptor"/> that are defined by Caravela itself.
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
            if ( !CaravelaCompilerInfo.IsActive )
            {
                CompilerServiceProvider.Initialize();
            }

            var directoryOptions = ServiceProviderFactory.GetServiceProvider().GetService<IPathOptions>();
            var userDefinedDescriptors = UserDiagnosticRegistrationService.GetInstance( directoryOptions ).GetSupportedDescriptors();

            // The file may contain system descriptors by mistake. We must remove them otherwise we will have some duplicate key issue.

            this.SupportedDiagnosticDescriptors =
                StandardDiagnosticDescriptors.Values
                    .Concat( userDefinedDescriptors.Diagnostics.Where( d => !StandardDiagnosticDescriptors.ContainsKey( d.Id ) ) )
                    .ToImmutableDictionary( d => d.Id, d => d, StringComparer.OrdinalIgnoreCase );

            this.SupportedSuppressionDescriptors =
                userDefinedDescriptors.Suppressions.ToImmutableDictionary( d => d.SuppressedDiagnosticId, d => d, StringComparer.OrdinalIgnoreCase );
        }
    }
}