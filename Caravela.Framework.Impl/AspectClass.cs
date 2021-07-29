// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Impl.CodeModel;
using Caravela.Framework.Impl.Sdk;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Utilities;
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
    internal class AspectClass : IAspectClass
    {
        private readonly Dictionary<string, TemplateDriver> _templateDrivers = new( StringComparer.Ordinal );

        private readonly IServiceProvider _serviceProvider;
        private readonly UserCodeInvoker _userCodeInvoker;
        private readonly IAspectDriver? _aspectDriver;

        private readonly IAspect? _prototypeAspectInstance; // Null for abstract classes.
        private IReadOnlyList<AspectLayer>? _layers;

        public Type AspectType { get; }

        /// <inheritdoc />
        public string FullName { get; }

        /// <inheritdoc />
        public string DisplayName { get; private set; }

        public string? Description { get; private set; }

        /// <summary>
        /// Gets metadata of the base aspect class.
        /// </summary>
        public AspectClass? BaseClass { get; }

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

        public bool IsLiveTemplate { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectClass"/> class.
        /// </summary>
        /// <param name="aspectTypeSymbol"></param>
        /// <param name="aspectDriver">Can be null for testing.</param>
        private AspectClass(
            IServiceProvider serviceProvider,
            INamedTypeSymbol aspectTypeSymbol,
            AspectClass? baseClass,
            IAspectDriver? aspectDriver,
            CompileTimeProject project,
            Type aspectType,
            IAspect? prototype )
        {
            this.FullName = aspectTypeSymbol.GetReflectionNameSafe();
            this.DisplayName = aspectTypeSymbol.Name.TrimEnd( "Attribute" );
            this.IsAbstract = aspectTypeSymbol.IsAbstract;
            this.BaseClass = baseClass;
            this.Project = project;
            this._serviceProvider = serviceProvider;
            this._userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
            this._aspectDriver = aspectDriver;
            this.DiagnosticLocation = aspectTypeSymbol.GetDiagnosticLocation();
            this.AspectType = aspectType;
            this._prototypeAspectInstance = prototype;
        }

        private void Initialize()
        {
            if ( this._prototypeAspectInstance != null )
            {
                var builder = new AspectClassBuilder( this );
                this._userCodeInvoker.Invoke( () => this._prototypeAspectInstance.BuildAspectClass( builder ) );

                this._layers = builder.Layers.As<string?>().Prepend( null ).Select( l => new AspectLayer( this, l ) ).ToImmutableArray();
            }

            // TODO: get all eligibility rules from the prototype instance and combine them into a single rule.
        }

        /// <summary>
        /// Creates a new  <see cref="AspectInstance"/>.
        /// </summary>
        /// <param name="aspect">The instance of the aspect class.</param>
        /// <param name="target">The declaration on which the aspect was applied.</param>
        /// <returns></returns>
        public AspectInstance CreateAspectInstance( IAspect aspect, IDeclaration target ) => new( aspect, target, this );

        /// <summary>
        /// Creates an instance of the <see cref="AspectClass"/> class.
        /// </summary>
        public static bool TryCreate(
            IServiceProvider serviceProvider,
            INamedTypeSymbol aspectNamedType,
            AspectClass? baseAspectType,
            IAspectDriver? aspectDriver,
            CompileTimeProject compileTimeProject,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out AspectClass? aspectClass )
        {
            var aspectType = compileTimeProject.GetType( aspectNamedType.GetReflectionNameSafe() ).AssertNotNull();
            var prototype = aspectNamedType.IsAbstract ? null : (IAspect) FormatterServices.GetUninitializedObject( aspectType ).AssertNotNull();

            aspectClass = new AspectClass( serviceProvider, aspectNamedType, baseAspectType, aspectDriver, compileTimeProject, aspectType, prototype );
            aspectClass.Initialize();

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

            templateDriver = new TemplateDriver( this._serviceProvider, this, sourceTemplate.GetSymbol().AssertNotNull(), compiledTemplateMethodInfo );
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
                // TODO: Map the symbol to a code model declaration (which requires a CompilationModel), then simply
                // call our aggregate eligibility rule here.

                IMethodSymbol method =>
                    this._prototypeAspectInstance is IAspect<IDeclaration> ||
                    this._prototypeAspectInstance is IAspect<IMemberOrNamedType> ||
                    this._prototypeAspectInstance is IAspect<IMethodBase> ||
                    (this._prototypeAspectInstance is IAspect<IMethod> && IsMethod( method.MethodKind )) ||
                    (this._prototypeAspectInstance is IAspect<IConstructor> && IsConstructor( method.MethodKind )),

                IPropertySymbol => this._prototypeAspectInstance is IAspect<IDeclaration> ||
                                   this._prototypeAspectInstance is IAspect<IMemberOrNamedType> ||
                                   this._prototypeAspectInstance is IAspect<IFieldOrProperty> ||
                                   this._prototypeAspectInstance is IAspect<IProperty>,

                IFieldSymbol => this._prototypeAspectInstance is IAspect<IDeclaration> ||
                                this._prototypeAspectInstance is IAspect<IMemberOrNamedType> ||
                                this._prototypeAspectInstance is IAspect<IFieldOrProperty> ||
                                this._prototypeAspectInstance is IAspect<IField>,

                IEventSymbol => this._prototypeAspectInstance is IAspect<IDeclaration> ||
                                this._prototypeAspectInstance is IAspect<IMemberOrNamedType> ||
                                this._prototypeAspectInstance is IAspect<IEvent>,

                INamedTypeSymbol => this._prototypeAspectInstance is IAspect<IDeclaration> ||
                                    this._prototypeAspectInstance is IAspect<IMemberOrNamedType> ||
                                    this._prototypeAspectInstance is IAspect<INamedType>,

                _ => false

                // TODO: parameters (using markers)
                // TODO: call IsEligible on the prototype
            };

        public override string ToString() => this.FullName;

        private class AspectClassBuilder : IAspectClassBuilder, IAspectDependencyBuilder
        {
            private readonly AspectClass _parent;

            public AspectClassBuilder( AspectClass parent )
            {
                this._parent = parent;
            }

            public bool IsLiveTemplate
            {
                get => this._parent.IsLiveTemplate;
                set
                {
                    if ( value != this._parent.IsLiveTemplate )
                    {
                        if ( value )
                        {
                            if ( this._parent.AspectType.GetConstructor( Type.EmptyTypes ) == null )
                            {
                                throw new InvalidOperationException( "The aspect type must have a default constructor to be able to be a live template." );
                            }
                        }

                        this._parent.IsLiveTemplate = value;
                    }
                }
            }

            public string DisplayName { get => this._parent.DisplayName; set => this._parent.DisplayName = value; }

            public string? Description { get => this._parent.Description; set => this._parent.Description = value; }

            public ImmutableArray<string> Layers { get; set; } = ImmutableArray<string>.Empty;

            public IAspectDependencyBuilder Dependencies => this;

            public void RequiresAspect<TAspect>()
                where TAspect : Attribute, IAspect, new()
                => throw new NotImplementedException();
        }
    }
}