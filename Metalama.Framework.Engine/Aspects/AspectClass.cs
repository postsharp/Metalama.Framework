﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Eligibility.Implementation;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Attribute = System.Attribute;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Represents the metadata of an aspect class. This class is compilation-independent. It is not used to represent a fabric class.
    /// </summary>
    public class AspectClass : TemplateClass, IAspectClassImpl, IBoundAspectClass, IValidatorDriverFactory
    {
        private readonly UserCodeInvoker _userCodeInvoker;
        private readonly IAspectDriver? _aspectDriver;
        private readonly IAspect? _prototypeAspectInstance; // Null for abstract classes.
        private ValidatorDriverFactory? _validatorDriverFactory;

        private static readonly MethodInfo _tryInitializeEligibilityMethod = typeof(AspectClass).GetMethod(
            nameof(TryInitializeEligibility),
            BindingFlags.Instance | BindingFlags.NonPublic );

        private ImmutableArray<KeyValuePair<Type, IEligibilityRule<IDeclaration>>> _eligibilityRules;

        public override Type AspectType { get; }

        public override string FullName { get; }

        public string ShortName { get; }

        /// <inheritdoc />
        public string DisplayName { get; }

        public string? Description { get; }

        internal override CompileTimeProject? Project { get; }

        CompileTimeProject? IAspectClassImpl.Project => this.Project;

        public ImmutableArray<TemplateClass> TemplateClasses { get; }

        /// <summary>
        /// Gets the aspect driver of the current class, responsible for executing the aspect.
        /// </summary>
        public IAspectDriver AspectDriver => this._aspectDriver.AssertNotNull();

        /// <summary>
        /// Gets the list of layers of the current aspect.
        /// </summary>
        internal ImmutableArray<AspectLayer> Layers { get; }

        public Location? DiagnosticLocation { get; }

        /// <inheritdoc />
        public bool IsAbstract { get; }

        public bool IsInherited { get; }

        public bool IsAttribute => typeof(Attribute).IsAssignableFrom( this.AspectType );

        Type IAspectClass.Type => this.AspectType;

        public bool IsLiveTemplate { get; }

        public bool HasError { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectClass"/> class.
        /// </summary>
        internal AspectClass(
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
            this.FullName = aspectTypeSymbol.GetReflectionName().AssertNotNull();
            this.DisplayName = this.ShortName = AttributeHelper.GetShortName( aspectTypeSymbol.Name );
            this.IsAbstract = aspectTypeSymbol.IsAbstract;
            this.Project = project;
            this._userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
            this.DiagnosticLocation = aspectTypeSymbol.GetDiagnosticLocation();
            this.AspectType = aspectType;
            this._prototypeAspectInstance = prototype;
            this.TemplateClasses = ImmutableArray.Create<TemplateClass>( this );

            List<string?> layers = new();

            if ( baseClass != null )
            {
                this.Description = baseClass.Description;
                this.IsInherited = baseClass.IsInherited;
                this.IsLiveTemplate = baseClass.IsLiveTemplate;
                layers.AddRange( baseClass.Layers.Select( l => l.LayerName ) );
            }
            else
            {
                layers.Add( null );
            }

            foreach ( var attribute in aspectTypeSymbol.GetAttributes() )
            {
                switch ( attribute.AttributeClass?.Name )
                {
                    case null:
                        continue;

                    case nameof(InheritedAttribute):
                        this.IsInherited = true;

                        break;

                    case nameof(LiveTemplateAttribute):
                        if ( !aspectTypeSymbol.HasDefaultConstructor() )
                        {
                            diagnosticAdder.Report(
                                GeneralDiagnosticDescriptors.LiveTemplateMustHaveDefaultConstructor.CreateRoslynDiagnostic(
                                    attribute.GetDiagnosticLocation(),
                                    aspectTypeSymbol ) );

                            this.HasError = true;
                        }
                        else
                        {
                            this.IsLiveTemplate = true;
                        }

                        break;

                    case nameof(LayersAttribute):
                        layers.AddRange( attribute.ConstructorArguments[0].Values.Select( v => (string?) v.Value ).Where( v => !string.IsNullOrEmpty( v ) ) );

                        break;

                    case nameof(DescriptionAttribute):
                        this.Description = (string?) attribute.ConstructorArguments[0].Value;

                        break;

                    case nameof(DisplayNameAttribute):
                        this.DisplayName = (string?) attribute.ConstructorArguments[0].Value ?? this.ShortName;

                        break;
                }
            }

            this.Layers = layers.Select( l => new AspectLayer( this, l ) ).ToImmutableArray();

            // This must be called after Members is built and assigned.
            this._aspectDriver = aspectDriverFactory.GetAspectDriver( this, aspectTypeSymbol );
        }

        private bool TryInitialize( IDiagnosticAdder diagnosticAdder )
        {
            if ( this.HasError )
            {
                return false;
            }

            if ( this._prototypeAspectInstance != null )
            {
                // Call BuildEligibility for all relevant interface implementations.
                List<KeyValuePair<Type, IEligibilityRule<IDeclaration>>> eligibilityRules = new();

                // Add additional rules defined by the driver.
                if ( this._aspectDriver is AspectDriver { EligibilityRule: { } eligibilityRule } )
                {
                    eligibilityRules.Add( new KeyValuePair<Type, IEligibilityRule<IDeclaration>>( typeof(IDeclaration), eligibilityRule ) );
                }

                var eligibilitySuccess = true;

                foreach ( var implementedInterface in this._prototypeAspectInstance.GetType()
                             .GetInterfaces()
                             .Where( i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEligible<>) ) )
                {
                    var declarationInterface = implementedInterface.GenericTypeArguments[0];

                    // If methods are eligible, we need to check that the target method is not a local function.
                    if ( declarationInterface == typeof(IMethod) )
                    {
                        eligibilityRules.Add(
                            new KeyValuePair<Type, IEligibilityRule<IDeclaration>>( typeof(IMethod), LocalFunctionEligibilityRule.Instance ) );
                    }

                    eligibilitySuccess &= (bool)
                        _tryInitializeEligibilityMethod.MakeGenericMethod( declarationInterface )
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
                // Abstract aspect classes don't have eligibility because they cannot be applied.
                this._eligibilityRules = ImmutableArray<KeyValuePair<Type, IEligibilityRule<IDeclaration>>>.Empty;
            }

            // TODO: get all eligibility rules from the prototype instance and combine them into a single rule.

            return true;
        }

        [Obfuscation( Exclude = true /* Reflection */ )]
        private bool TryInitializeEligibility<T>( IDiagnosticAdder diagnosticAdder, List<KeyValuePair<Type, IEligibilityRule<IDeclaration>>> rules )
            where T : class, IDeclaration
        {
            if ( this._prototypeAspectInstance is IEligible<T> eligible )
            {
                var builder = new EligibilityBuilder<T>();

                var executionContext = new UserCodeExecutionContext(
                    this.ServiceProvider,
                    diagnosticAdder,
                    UserCodeMemberInfo.FromDelegate( new Action<IEligibilityBuilder<T>>( eligible.BuildEligibility ) ) );

                if ( !this._userCodeInvoker.TryInvoke( () => eligible.BuildEligibility( builder ), executionContext ) )
                {
                    return false;
                }

                rules.Add( new KeyValuePair<Type, IEligibilityRule<IDeclaration>>( typeof(T), ((IEligibilityBuilder<T>) builder).Build() ) );
            }

            return true;
        }

        /// <summary>
        /// Creates a new  <see cref="AspectInstance"/> from a custom attribute.
        /// </summary>
        internal AspectInstance CreateAspectInstanceFromAttribute(
            IAspect aspect,
            in Ref<IDeclaration> target,
            IAttribute attribute,
            CompileTimeProjectLoader loader )
            => new( aspect, target, this, new AspectPredecessor( AspectPredecessorKind.Attribute, attribute ) );

        /// <summary>
        /// Creates a new <see cref="AspectInstance"/> by using the default constructor of the current class.
        /// This method is used by live templates.
        /// </summary>
        internal AspectInstance CreateAspectInstance( IDeclaration target, IAspect aspect, in AspectPredecessor predecessor )
            => new( aspect, target.ToTypedRef(), this, predecessor );

        /// <summary>
        /// Creates an instance of the <see cref="AspectClass"/> class.
        /// </summary>
        internal static bool TryCreate(
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
            IAspect? prototype;

            if ( aspectTypeSymbol.IsAbstract )
            {
                prototype = null;
            }
            else
            {
                var aspectInterfaceType = typeof(IAspect);

                if ( !aspectInterfaceType.IsAssignableFrom( aspectInterfaceType ) )
                {
                    // This happens in case of a bug in assembly resolution
                    // (typically AppDomain.AssemblyResolve event handler).
                    throw new AssertionFailedException( "Assembly version mismatch." );
                }

                var untypedPrototype = FormatterServices.GetUninitializedObject( aspectReflectionType ).AssertNotNull();

                prototype = (IAspect) untypedPrototype;
            }

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
                aspectClass = null;

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
                IMethodSymbol method when IsMethod( method.MethodKind ) => method.MethodKind != MethodKind.LocalFunction ? typeof(IMethod) : null,
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
        }

        public EligibleScenarios GetEligibility( IDeclaration obj )
        {
            if ( this._eligibilityRules.IsDefaultOrEmpty )
            {
                // Linker tests do not set this member but don't need to test eligibility.
                return EligibleScenarios.Aspect;
            }

            // We may execute user code, so we need to execute in a user context. This is not optimal, but we don't know,
            // in the current design, where we have user code. Also, we cannot report diagnostics in the current design,
            // so we have to let the exception fly.
            var executionContext = new UserCodeExecutionContext( this.ServiceProvider );

            return this._userCodeInvoker.Invoke( GetEligibilityCore, executionContext );

            // Implementation, which all runs in a user context.
            EligibleScenarios GetEligibilityCore()
            {
                var declarationType = obj.GetType();
                var eligibility = EligibleScenarios.All;

                // If the aspect cannot be inherited, remove the inheritance eligibility.
                if ( !this.IsInherited )
                {
                    eligibility &= ~EligibleScenarios.Inheritance;
                }

                // Evaluate all eligibility rules that apply to the target declaration type.
                foreach ( var rule in this._eligibilityRules )
                {
                    if ( rule.Key.IsAssignableFrom( declarationType ) )
                    {
                        eligibility &= rule.Value.GetEligibility( obj );

                        if ( eligibility == EligibleScenarios.None )
                        {
                            return EligibleScenarios.None;
                        }
                    }
                }

                return eligibility;
            }
        }

        public FormattableString? GetIneligibilityJustification( EligibleScenarios requestedEligibility, IDescribedObject<IDeclaration> describedObject )
        {
            var targetDeclaration = describedObject.Object;
            var declarationType = targetDeclaration.GetType();

            var group = new AndEligibilityRule<IDeclaration>(
                this._eligibilityRules.Where( r => r.Key.IsAssignableFrom( declarationType ) )
                    .Select( r => r.Value )
                    .ToImmutableArray() );

            // We may execute user code, so we need to execute in a user context. This is not optimal, but we don't know,
            // in the current design, where we have user code. Also, we cannot report diagnostics in the current design,
            // so we have to let the exception fly.
            var executionContext = new UserCodeExecutionContext( this.ServiceProvider );

            return this._userCodeInvoker.Invoke(
                () =>
                    group.GetIneligibilityJustification(
                        requestedEligibility,
                        new DescribedObject<IDeclaration>( targetDeclaration, $"'{targetDeclaration}'" ) ),
                executionContext );
        }

        public IAspect CreateDefaultInstance() => (IAspect) Activator.CreateInstance( this.AspectType );

        public override string ToString() => this.FullName;

        ValidatorDriver<TContext> IValidatorDriverFactory.GetValidatorDriver<TContext>( MethodInfo validateMethod )
        {
            this._validatorDriverFactory ??= ValidatorDriverFactory.GetInstance( this.AspectType );

            return this._validatorDriverFactory.GetValidatorDriver<TContext>( validateMethod );
        }

        private class LocalFunctionEligibilityRule : IEligibilityRule<IDeclaration>
        {
            public static LocalFunctionEligibilityRule Instance { get; } = new();

            private LocalFunctionEligibilityRule() { }

            public EligibleScenarios GetEligibility( IDeclaration obj )
                => ((IMethod) obj).MethodKind == Code.MethodKind.LocalFunction ? EligibleScenarios.None : EligibleScenarios.All;

            public FormattableString? GetIneligibilityJustification( EligibleScenarios requestedEligibility, IDescribedObject<IDeclaration> describedObject )
                => ((IMethod) describedObject.Object).MethodKind == Code.MethodKind.LocalFunction
                    ? $"{describedObject} is a local function"
                    : (FormattableString?) null;
        }
    }
}