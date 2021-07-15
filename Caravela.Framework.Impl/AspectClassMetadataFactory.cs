// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// Creates <see cref="AspectClass"/>.
    /// </summary>
    internal class AspectClassMetadataFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AspectDriverFactory _aspectDriverFactory;

        private readonly Dictionary<INamedTypeSymbol, AspectClass> _aspectClasses = new();

        public AspectClassMetadataFactory( IServiceProvider serviceProvider, AspectDriverFactory aspectDriverFactory )
        {
            this._serviceProvider = serviceProvider;
            this._aspectDriverFactory = aspectDriverFactory;
        }

        /// <summary>
        /// Gets the aspect classes in a given <see cref="Compilation"/> for the closure of all references <see cref="CompileTimeProject"/>
        /// instances.
        /// </summary>
        public IReadOnlyList<AspectClass> GetAspectClasses(
            Compilation compilation,
            CompileTimeProject? compileTimeProject,
            IDiagnosticAdder diagnosticAdder )
        {
            if ( compileTimeProject == null )
            {
                // No compile-time project means that there is no aspect at all.
                return Array.Empty<AspectClass>();
            }

            // Gets the aspect types in the current compilation, including aspects types in referenced assemblies.
            var aspectTypeDataDictionary =
                compileTimeProject.ClosureProjects
                    .SelectMany( p => p.AspectTypes.Select( t => (Project: p, TypeName: t) ) )
                    .ToDictionary(
                        item => item.TypeName,
                        item => new AspectTypeData( item.Project, item.TypeName, Type: compilation.GetTypeByMetadataName( item.TypeName ) ) );

            return this.GetAspectClasses( aspectTypeDataDictionary, diagnosticAdder );
        }

        /// <summary>
        /// Creates a list of <see cref="AspectClass"/> given input list of aspect types. This method is used for test only.
        /// </summary>
        public IReadOnlyList<AspectClass> GetAspectClasses(
            IReadOnlyList<INamedTypeSymbol> aspectTypes,
            CompileTimeProject compileTimeProject,
            IDiagnosticAdder diagnosticAdder )
        {
            var aspectTypesDiagnostics = aspectTypes.ToDictionary(
                t => t.GetReflectionNameSafe(),
                t => new AspectTypeData( compileTimeProject, t.GetReflectionNameSafe(), t ) );

            return this.GetAspectClasses( aspectTypesDiagnostics, diagnosticAdder );
        }

        private IReadOnlyList<AspectClass> GetAspectClasses(
            Dictionary<string, AspectTypeData> aspectTypeDataDictionary,
            IDiagnosticAdder diagnosticAdder )
        {
            // A local function that recursively processes an aspect type.
            bool TryProcessType( INamedTypeSymbol aspectType, CompileTimeProject project, [NotNullWhen( true )] out AspectClass? metadata )
            {
                if ( this._aspectClasses.TryGetValue( aspectType, out var existingValue ) )
                {
                    metadata = existingValue;

                    return true;
                }

                AspectClass? baseAspectClass = null;

                if ( aspectType.BaseType != null )
                {
                    // Process the base type.

                    if ( aspectTypeDataDictionary.TryGetValue( aspectType.BaseType.GetReflectionNameSafe(), out var baseData ) )
                    {
                        if ( !TryProcessType( aspectType.BaseType, baseData.Project, out baseAspectClass ) )
                        {
                            metadata = null;

                            return false;
                        }
                    }
                    else
                    {
                        // This is not an aspect class, typically System.Attribute.
                    }
                }

                var aspectDriver = this._aspectDriverFactory.GetAspectDriver( aspectType );

                if ( !AspectClass.TryCreate( this._serviceProvider, aspectType, baseAspectClass, aspectDriver, project, diagnosticAdder, out metadata ) )
                {
                    return false;
                }

                this._aspectClasses.Add( aspectType, metadata );

                return true;
            }

            // Process all types.
            var resultList = new List<AspectClass>( aspectTypeDataDictionary.Count );

            foreach ( var attributeTypeData in aspectTypeDataDictionary.Values )
            {
                if ( attributeTypeData.Type == null )
                {
                    diagnosticAdder.Report(
                        TemplatingDiagnosticDescriptors.CannotFindAspectInCompilation.CreateDiagnostic(
                            Location.None,
                            (attributeTypeData.TypeName, attributeTypeData.Project.RunTimeIdentity.Name) ) );

                    continue;
                }

                if ( TryProcessType( attributeTypeData.Type, attributeTypeData.Project, out var metadata ) )
                {
                    resultList.Add( metadata );
                }
            }

            return resultList;
        }

        private record AspectTypeData( CompileTimeProject Project, string TypeName, INamedTypeSymbol? Type );
    }
}