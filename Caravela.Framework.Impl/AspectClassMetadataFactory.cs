// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
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

            // Add the abstract aspect classes from the framework because they define the abstract templates. The knowledge of abstract templates
            // is used by AspectClass. It is easier to do it here than to do it at the level of CompileTimeProject.
            var frameworkAspectClasses =
                new[] { typeof(OverrideMethodAspect), typeof(OverrideEventAspect), typeof(OverrideFieldOrPropertyAspect) }
                    .Select( t => new AspectTypeData( null, t.FullName, compilation.GetTypeByMetadataNameSafe( t.FullName ), t ) );

            // Gets the aspect types in the current compilation, including aspects types in referenced assemblies.
            var aspectTypeDataDictionary =
                compileTimeProject.ClosureProjects
                    .SelectMany( p => p.AspectTypes.Select( t => (Project: p, TypeName: t) ) )
                    .Select(
                        item =>
                        {
                            var typeSymbol = compilation.GetTypeByMetadataName( item.TypeName );

                            if ( typeSymbol == null )
                            {
                                diagnosticAdder.Report(
                                    TemplatingDiagnosticDescriptors.CannotFindAspectInCompilation.CreateDiagnostic(
                                        Location.None,
                                        (item.TypeName, item.Project.RunTimeIdentity.Name) ) );

                                return null;
                            }

                            return new AspectTypeData(
                                item.Project,
                                item.TypeName,
                                typeSymbol,
                                compileTimeProject.AssertNotNull().GetType( typeSymbol.GetReflectionName() ) );
                        } )
                    .WhereNotNull()
                    .Concat( frameworkAspectClasses )
                    .ToDictionary(
                        item => item.TypeName,
                        item => item );

            return this.GetAspectClasses( aspectTypeDataDictionary, diagnosticAdder, compilation );
        }

        /// <summary>
        /// Creates a list of <see cref="AspectClass"/> given input list of aspect types. This method is used for test only.
        /// </summary>
        internal IReadOnlyList<AspectClass> GetAspectClasses(
            IReadOnlyList<INamedTypeSymbol> aspectTypes,
            CompileTimeProject compileTimeProject,
            IDiagnosticAdder diagnosticAdder )
        {
            var aspectTypesDiagnostics = aspectTypes.ToDictionary(
                t => t.GetReflectionName(),
                t => new AspectTypeData( compileTimeProject, t.GetReflectionName(), t, compileTimeProject.GetType( t.GetReflectionName() ) ) );

            return this.GetAspectClasses( aspectTypesDiagnostics, diagnosticAdder, null! );
        }

        private IReadOnlyList<AspectClass> GetAspectClasses(
            Dictionary<string, AspectTypeData> aspectTypeDataDictionary,
            IDiagnosticAdder diagnosticAdder,
            Compilation compilation )
        {
            // A local function that recursively processes an aspect type.
            bool TryProcessType(
                INamedTypeSymbol aspectTypeSymbol,
                Type aspectReflectionType,
                CompileTimeProject? project,
                [NotNullWhen( true )] out AspectClass? metadata )
            {
                if ( this._aspectClasses.TryGetValue( aspectTypeSymbol, out var existingValue ) )
                {
                    metadata = existingValue;

                    return true;
                }

                AspectClass? baseAspectClass = null;

                if ( aspectTypeSymbol.BaseType != null )
                {
                    // Process the base type.

                    if ( aspectTypeDataDictionary.TryGetValue( aspectTypeSymbol.BaseType.GetReflectionName(), out var baseData ) )
                    {
                        if ( !TryProcessType( aspectTypeSymbol.BaseType, aspectReflectionType.BaseType, baseData.Project, out baseAspectClass ) )
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

                if ( !AspectClass.TryCreate(
                    this._serviceProvider,
                    aspectTypeSymbol,
                    aspectReflectionType,
                    baseAspectClass,
                    project,
                    diagnosticAdder,
                    compilation,
                    this._aspectDriverFactory,
                    out metadata ) )
                {
                    return false;
                }

                this._aspectClasses.Add( aspectTypeSymbol, metadata );

                return true;
            }

            // Process all types.
            var resultList = new List<AspectClass>( aspectTypeDataDictionary.Count );

            foreach ( var attributeTypeData in aspectTypeDataDictionary.Values )
            {
                if ( TryProcessType( attributeTypeData.TypeSymbol, attributeTypeData.ReflectionType, attributeTypeData.Project, out var metadata ) )
                {
                    resultList.Add( metadata );
                }
            }

            return resultList;
        }

        private record AspectTypeData( CompileTimeProject? Project, string TypeName, INamedTypeSymbol TypeSymbol, Type ReflectionType );
    }
}