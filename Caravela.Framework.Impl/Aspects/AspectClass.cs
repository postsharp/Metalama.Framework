﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Eligibility.Implementation;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Sdk;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Caravela.Framework.Impl.Aspects
{
    /// <summary>
    /// Represents the metadata of an aspect class. This class is compilation-independent. It is not used to represent a fabric class.
    /// </summary>
    internal partial class AspectClass : TemplateClass, IAspectClassImpl, IBoundAspectClass
    {
        private readonly UserCodeInvoker _userCodeInvoker;
        private readonly IAspectDriver? _aspectDriver;

        private readonly IAspect? _prototypeAspectInstance; // Null for abstract classes.
        private IReadOnlyList<AspectLayer>? _layers;

        private static readonly MethodInfo _tryInitializeEligibilityMethod = typeof(AspectClass).GetMethod(
            nameof(TryInitializeEligibility),
            BindingFlags.Instance | BindingFlags.NonPublic );

        private ImmutableArray<KeyValuePair<Type, IEligibilityRule<IDeclaration>>> _eligibilityRules;

        public override Type AspectType { get; }

        public override string FullName { get; }

        /// <inheritdoc />
        public string DisplayName { get; private set; }

        public string? Description { get; private set; }

        public override CompileTimeProject? Project { get; }

        public ImmutableArray<TemplateClass> TemplateClasses { get; }

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

        public bool IsInherited { get; private set; }

        public bool IsLiveTemplate { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectClass"/> class.
        /// </summary>
        /// <param name="aspectTypeSymbol"></param>
        /// <param name="aspectDriver">Can be null for testing.</param>
        protected internal AspectClass(
            IServiceProvider serviceProvider,
            INamedTypeSymbol aspectTypeSymbol,
            AspectClass? baseClass,
            CompileTimeProject? project,
            Type aspectType,
            IAspect? prototype,
            IDiagnosticAdder diagnosticAdder,
            Compilation compilation,
            AspectDriverFactory aspectDriverFactory ) : base( serviceProvider, compilation, aspectTypeSymbol, diagnosticAdder, baseClass )
        {
            this.FullName = aspectTypeSymbol.GetReflectionName();
            this.DisplayName = AttributeRef.GetShortName( aspectTypeSymbol.Name );
            this.IsAbstract = aspectTypeSymbol.IsAbstract;
            this.Project = project;
            this._userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
            this.DiagnosticLocation = aspectTypeSymbol.GetDiagnosticLocation();
            this.AspectType = aspectType;
            this._prototypeAspectInstance = prototype;

            this.TemplateClasses = ImmutableArray.Create<TemplateClass>( this );

            // This must be called after Members is built and assigned.
            this._aspectDriver = aspectDriverFactory.GetAspectDriver( this, aspectTypeSymbol );
        }

        private bool TryInitialize( IDiagnosticAdder diagnosticAdder )
        {
            if ( this._prototypeAspectInstance != null )
            {
                // Call BuildAspectClass
                var classBuilder = new Builder( this );

                try
                {
                    this._userCodeInvoker.Invoke( () => this._prototypeAspectInstance.BuildAspectClass( classBuilder ) );
                }
                catch ( Exception e )
                {
                    var diagnostic = GeneralDiagnosticDescriptors.ExceptionInUserCode.CreateDiagnostic(
                        null,
                        (AspectType: this.DisplayName, MethodName: nameof(IAspect.BuildAspectClass), e.GetType().Name, e.Format( 5 )) );

                    diagnosticAdder.Report( diagnostic );

                    return false;
                }

                this._layers = classBuilder.Layers.As<string?>().Prepend( null ).Select( l => new AspectLayer( this, l ) ).ToImmutableArray();

                // Call BuildEligibility for all relevant interface implementations.
                Dictionary<Type, IEligibilityRule<IDeclaration>> eligibilityRules = new();
                var eligibilitySuccess = true;

                foreach ( var implementedInterface in this._prototypeAspectInstance.GetType()
                    .GetInterfaces()
                    .Where( i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEligible<>) ) )
                {
                    eligibilitySuccess &= (bool)
                        _tryInitializeEligibilityMethod.MakeGenericMethod( implementedInterface.GenericTypeArguments[0] )
                            .Invoke( this, new object[] { diagnosticAdder, eligibilityRules } );
                }

                if ( !eligibilitySuccess )
                {
                    return false;
                }

                this._eligibilityRules = eligibilityRules.ToImmutableArray();
            }
            else
            {
                // Abstract aspect classes don't have any layer.
                this._layers = Array.Empty<AspectLayer>();
                
                // Abstract aspect classes don't have eligibility because they cannot be applied.
                this._eligibilityRules = ImmutableArray<KeyValuePair<Type, IEligibilityRule<IDeclaration>>>.Empty;
            }

            // TODO: get all eligibility rules from the prototype instance and combine them into a single rule.

            return true;
        }

        [Obfuscation( Exclude = true /* Reflection */ )]
        private bool TryInitializeEligibility<T>( IDiagnosticAdder diagnosticAdder, Dictionary<Type, IEligibilityRule<IDeclaration>> rules )
            where T : class, IDeclaration
        {
            if ( this._prototypeAspectInstance is IEligible<T> eligible )
            {
                var builder = new EligibilityBuilder<T>();

                try
                {
                    this._userCodeInvoker.Invoke( () => eligible.BuildEligibility( builder ) );
                }
                catch ( Exception e )
                {
                    var diagnostic = GeneralDiagnosticDescriptors.ExceptionInUserCode.CreateDiagnostic(
                        null,
                        (AspectType: this.DisplayName, MethodName: nameof(IAspect.BuildAspectClass), e.GetType().Name, e.Format( 5 )) );

                    diagnosticAdder.Report( diagnostic );

                    return false;
                }

                rules.Add( typeof(T), ((IEligibilityBuilder<T>) builder).Build() );
            }

            return true;
        }

        /// <summary>
        /// Creates a new  <see cref="AspectInstance"/> from a custom attribute.
        /// </summary>
        public AttributeAspectInstance CreateAspectInstanceFromAttribute(
            IAspect aspect,
            IDeclaration target,
            IAttribute attribute,
            CompileTimeProjectLoader loader )
            => new( aspect, target, this, attribute, loader );

        /// <summary>
        /// Creates a new <see cref="AspectInstance"/> by using the default constructor of the current class.
        /// This method is used by live templates.
        /// </summary>
        public AspectInstance CreateDefaultAspectInstance( IDeclaration target, in AspectPredecessor predecessor )
            => new( (IAspect) Activator.CreateInstance( this.AspectType ), target, this, predecessor );

        /// <summary>
        /// Creates an instance of the <see cref="AspectClass"/> class.
        /// </summary>
        public static bool TryCreate(
            IServiceProvider serviceProvider,
            INamedTypeSymbol aspectTypeSymbol,
            Type aspectReflectionType,
            AspectClass? baseAspectClass,
            CompileTimeProject? compileTimeProject,
            IDiagnosticAdder diagnosticAdder,
            Compilation compilation,
            AspectDriverFactory aspectDriverFactory,
            [NotNullWhen( true )] out AspectClass? aspectClass )
        {
            var prototype = aspectTypeSymbol.IsAbstract ? null : (IAspect) FormatterServices.GetUninitializedObject( aspectReflectionType ).AssertNotNull();

            aspectClass = new AspectClass(
                serviceProvider,
                aspectTypeSymbol,
                baseAspectClass,
                compileTimeProject,
                aspectReflectionType,
                prototype,
                diagnosticAdder,
                compilation,
                aspectDriverFactory );

            if ( !aspectClass.TryInitialize( diagnosticAdder ) )
            {
                return false;
            }

            return true;
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

        /// <summary>
        /// Determines the eligibility of a Roslyn symbol for the current aspect without constructing a <see cref="CompilationModel"/>
        /// for the symbol.
        /// </summary>
        public bool IsEligibleFast( ISymbol symbol )
        {
            var ourDeclarationInterface = symbol switch
            {
                IMethodSymbol method when IsMethod( method.MethodKind ) => typeof(IMethod),
                IMethodSymbol method when IsConstructor( method.MethodKind ) => typeof(IConstructor),
                IPropertySymbol => typeof(IProperty),
                IEventSymbol => typeof(IEvent),
                IFieldSymbol => typeof(IField),
                ITypeSymbol { TypeKind: TypeKind.TypeParameter } => typeof(ITypeParameter),
                INamedTypeSymbol => typeof(INamedType),
                IParameterSymbol => typeof(IParameter),
                _ => null
            };

            if ( ourDeclarationInterface == null )
            {
                return false;
            }

            var aspectInterface = typeof(IAspect<>).MakeGenericType( ourDeclarationInterface );

            return aspectInterface.IsAssignableFrom( this.AspectType );

            // TODO: call IsEligible on the prototype
        }

        public EligibleScenarios GetEligibility( IDeclaration targetDeclaration )
        {
            if ( this._eligibilityRules.IsDefaultOrEmpty )
            {
                // Linker tests do not set this member but don't need to test eligibility.
                return EligibleScenarios.Aspect;
            }
            
            var declarationType = targetDeclaration.GetType();
            var eligibility = EligibleScenarios.All;

            foreach ( var rule in this._eligibilityRules )
            {
                if ( rule.Key.IsAssignableFrom( declarationType ) )
                {
                    eligibility &= rule.Value.GetEligibility( targetDeclaration );

                    if ( eligibility == EligibleScenarios.None )
                    {
                        return EligibleScenarios.None;
                    }
                }
            }

            return eligibility;
        }

        public string? GetIneligibilityJustification( IDeclaration targetDeclaration, EligibleScenarios requestedEligibility )
        {
            var declarationType = targetDeclaration.GetType();

            var group = new AndEligibilityRule<IDeclaration>(
                this._eligibilityRules.Where( r => r.Key.IsAssignableFrom( declarationType ) )
                    .Select( r => r.Value )
                    .ToImmutableArray() );

            return group.GetIneligibilityJustification(
                    requestedEligibility,
                    new DescribedObject<IDeclaration>( targetDeclaration, $"'{targetDeclaration}'") )
                ?.ToString( UserMessageFormatter.Instance );
        }

        public override string ToString() => this.FullName;
    }
}