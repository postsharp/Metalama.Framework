// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// Represents the metadata of an aspect class. This class is compilation-independent. 
    /// </summary>
    internal class AspectClassMetadata : IAspectClassMetadata
    {
        private readonly Dictionary<string, TemplateDriver> _templateDrivers = new( StringComparer.Ordinal );

        private readonly IAspectDriver? _aspectDriver;

        private readonly IAspect _prototypeAspectInstance;
        private IReadOnlyList<AspectLayer>? _layers;

        public Type AspectType { get; }

        /// <inheritdoc />
        public string FullName { get; }

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <summary>
        /// Gets metadata of the base aspect class.
        /// </summary>
        public AspectClassMetadata? BaseClass { get; }

        public CompileTimeProject Project { get; }

        /// <summary>
        /// Gets the aspect driver of the current class, responsible for executing the aspect.
        /// </summary>
        public IAspectDriver AspectDriver => this._aspectDriver.AssertNotNull();

        /// <summary>
        /// Gets the list of layers of the current aspect.
        /// </summary>
        public IReadOnlyList<AspectLayer> Layers => this._layers.AssertNotNull();

        public Location? DiagnosticLocation { get; }

        /// <inheritdoc />
        public bool IsAbstract { get; }

        public bool CanExpandToSource { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectClassMetadata"/> class.
        /// </summary>
        /// <param name="aspectTypeSymbol"></param>
        /// <param name="aspectDriver">Can be null for testing.</param>
        private AspectClassMetadata(
            INamedTypeSymbol aspectTypeSymbol,
            AspectClassMetadata? baseClass,
            IAspectDriver? aspectDriver,
            CompileTimeProject project )
        {
            this.FullName = aspectTypeSymbol.GetReflectionNameSafe();
            this.DisplayName = aspectTypeSymbol.Name.TrimEnd( "Attribute" );
            this.IsAbstract = aspectTypeSymbol.IsAbstract;
            this.BaseClass = baseClass;
            this.Project = project;
            this._aspectDriver = aspectDriver;
            this.DiagnosticLocation = aspectTypeSymbol.GetDiagnosticLocation();

            if ( this.Project != null! )
            {
                this.AspectType = this.Project.GetType( this.FullName )!;

                this._prototypeAspectInstance =
                    (IAspect) FormatterServices.GetUninitializedObject( this.AspectType ).AssertNotNull();

                // TODO: We may have a custom attribute to enable that feature.
                this.CanExpandToSource = this.AspectType.GetConstructor( Type.EmptyTypes ) != null;
            }
            else
            {
                // CompileTimeProject may be null in tests. These lines makes the analyzer happy.

                this.AspectType = null!;
                this._prototypeAspectInstance = null!;
                this.Project = null!;
            }
        }

        /// <summary>
        /// Creates a new  <see cref="AspectInstance"/>.
        /// </summary>
        /// <param name="aspect">The instance of the aspect class.</param>
        /// <param name="target">The declaration on which the aspect was applied.</param>
        /// <returns></returns>
        public AspectInstance CreateAspectInstance( IAspect aspect, ICodeElement target ) => new( aspect, target, this );

        /// <summary>
        /// Creates an instance of the <see cref="AspectClassMetadata"/> class.
        /// </summary>
        public static bool TryCreate(
            INamedTypeSymbol aspectNamedType,
            AspectClassMetadata? baseAspectType,
            IAspectDriver? aspectDriver,
            CompileTimeProject compileTimeProject,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out AspectClassMetadata? aspectClassMetadata )
        {
            var layersBuilder = ImmutableArray.CreateBuilder<AspectLayer>();

            var newAspectType = new AspectClassMetadata( aspectNamedType, baseAspectType, aspectDriver, compileTimeProject );

            // Add the default part.
            layersBuilder.Add( new AspectLayer( newAspectType, null ) );

            // Add the parts defined in [ProvidesAspectLayers]. If it is not defined in the current type, look up in the base classes.

            for ( var type = aspectNamedType; type != null; type = type.BaseType )
            {
                var aspectLayersAttributeData =
                    type.GetAttributes().SingleOrDefault( a => a.AttributeClass?.Is( typeof(ProvidesAspectLayersAttribute) ) ?? false );

                if ( aspectLayersAttributeData != null )
                {
                    // TODO: Using global state makes it impossible to test.
                    if ( !AttributeDeserializer.SystemTypes.TryCreateAttribute<ProvidesAspectLayersAttribute>(
                        aspectLayersAttributeData,
                        diagnosticAdder,
                        out var aspectLayersAttribute ) )
                    {
                        aspectClassMetadata = null;

                        return false;
                    }

                    layersBuilder.AddRange( aspectLayersAttribute.Layers.Select( partName => new AspectLayer( newAspectType, partName ) ) );

                    break;
                }
            }

            newAspectType._layers = layersBuilder.ToImmutable();

            aspectClassMetadata = newAspectType;

            return true;
        }

        public TemplateDriver GetTemplateDriver( IMethod sourceTemplate )
        {
            var templateSymbol = sourceTemplate.GetSymbol().AssertNotNull();
            var id = templateSymbol.GetDocumentationCommentId()!;

            if ( this._templateDrivers.TryGetValue( id, out var templateDriver ) )
            {
                return templateDriver;
            }

            var templateName = TemplateNameHelper.GetCompiledTemplateName( templateSymbol );
            var compiledTemplateMethodInfo = this.AspectType.GetMethod( templateName );

            if ( compiledTemplateMethodInfo == null )
            {
                throw new AssertionFailedException( $"Could not find the compile template for {sourceTemplate}." );
            }

            templateDriver = new TemplateDriver( this, sourceTemplate.GetSymbol().AssertNotNull(), compiledTemplateMethodInfo );
            this._templateDrivers.Add( id, templateDriver );

            return templateDriver;
        }

        private static bool IsMethod( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.Constructor => false,
                MethodKind.StaticConstructor => false,
                MethodKind.AnonymousFunction => false,
                _ => true
            };

        private static bool IsConstructor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.Constructor => true,
                MethodKind.StaticConstructor => true,
                _ => false
            };

        public bool IsEligible( ISymbol symbol )
            => symbol switch
            {
                IMethodSymbol method => this._prototypeAspectInstance is IAspect<ICodeElement> ||
                                        this._prototypeAspectInstance is IAspect<IMember> ||
                                        this._prototypeAspectInstance is IAspect<IMethodBase> ||
                                        (this._prototypeAspectInstance is IAspect<IMethod> && IsMethod( method.MethodKind )) ||
                                        (this._prototypeAspectInstance is IAspect<IConstructor> && IsConstructor( method.MethodKind )),

                IPropertySymbol => this._prototypeAspectInstance is IAspect<ICodeElement> ||
                                   this._prototypeAspectInstance is IAspect<IMember> ||
                                   this._prototypeAspectInstance is IAspect<IFieldOrProperty> ||
                                   this._prototypeAspectInstance is IAspect<IProperty>,

                IFieldSymbol => this._prototypeAspectInstance is IAspect<ICodeElement> ||
                                this._prototypeAspectInstance is IAspect<IMember> ||
                                this._prototypeAspectInstance is IAspect<IFieldOrProperty> ||
                                this._prototypeAspectInstance is IAspect<IField>,

                IEventSymbol => this._prototypeAspectInstance is IAspect<ICodeElement> ||
                                this._prototypeAspectInstance is IAspect<IMember> ||
                                this._prototypeAspectInstance is IAspect<IEvent>,

                INamedTypeSymbol => this._prototypeAspectInstance is IAspect<ICodeElement> ||
                                    this._prototypeAspectInstance is IAspect<IMember> ||
                                    this._prototypeAspectInstance is IAspect<INamedType>,

                _ => false

                // TODO: parameters (using markers)
                // TODO: call IsEligible on the prototype
            };
    }
}