﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Creates <see cref="AspectClass"/>.
    /// </summary>
    internal class AspectClassFactory : TemplateClassFactory<AspectClass>
    {
        private readonly AspectDriverFactory _aspectDriverFactory;

        public AspectClassFactory( AspectDriverFactory aspectDriverFactory )
        {
            this._aspectDriverFactory = aspectDriverFactory;
        }

        protected override IEnumerable<TemplateTypeData> GetFrameworkClasses( Compilation compilation )
        {
            var frameworkAssemblyName = typeof(IAspect).Assembly.GetName();
            var frameworkAssembly = compilation.SourceModule.ReferencedAssemblySymbols.SingleOrDefault( x => x.Name == frameworkAssemblyName.Name );

            if ( frameworkAssembly == null )
            {
                return Array.Empty<TemplateTypeData>();
            }

            return new[] { typeof(OverrideMethodAspect), typeof(OverrideEventAspect), typeof(OverrideFieldOrPropertyAspect) }
                .SelectAsImmutableArray( t => new TemplateTypeData( null, t.FullName!, frameworkAssembly.GetTypeByMetadataName( t.FullName! )!, t ) );
        }

        protected override IEnumerable<string> GetTypeNames( CompileTimeProject project ) => project.AspectTypes;

        protected override bool TryCreate(
            ProjectServiceProvider serviceProvider,
            INamedTypeSymbol templateTypeSymbol,
            Type templateReflectionType,
            AspectClass? baseClass,
            CompileTimeProject? compileTimeProject,
            IDiagnosticAdder diagnosticAdder,
            CompilationContext compilationContext,
            [NotNullWhen( true )] out AspectClass? templateClass )
            => AspectClass.TryCreate(
                serviceProvider,
                templateTypeSymbol,
                templateReflectionType,
                baseClass,
                compileTimeProject,
                diagnosticAdder,
                compilationContext,
                this._aspectDriverFactory,
                out templateClass );
    }
}