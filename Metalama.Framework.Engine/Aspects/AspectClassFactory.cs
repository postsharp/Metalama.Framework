// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
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

        public AspectClassFactory( IServiceProvider serviceProvider, AspectDriverFactory aspectDriverFactory ) : base( serviceProvider )
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
                .Select( t => new TemplateTypeData( null, t.FullName!, frameworkAssembly.GetTypeByMetadataName( t.FullName! )!, t ) );
        }

        protected override IEnumerable<string> GetTypeNames( CompileTimeProject project ) => project.AspectTypes;

        protected override bool TryCreate(
            INamedTypeSymbol templateTypeSymbol,
            Type templateReflectionType,
            AspectClass? baseClass,
            CompileTimeProject? compileTimeProject,
            IDiagnosticAdder diagnosticAdder,
            Compilation compilation,
            [NotNullWhen( true )] out AspectClass? templateClass )
            => AspectClass.TryCreate(
                this.ServiceProvider,
                templateTypeSymbol,
                templateReflectionType,
                baseClass,
                compileTimeProject,
                diagnosticAdder,
                compilation,
                this._aspectDriverFactory,
                out templateClass );
    }
}