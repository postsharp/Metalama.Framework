// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
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
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// Represents the metadata of an aspect class. This class is compilation-independent. 
    /// </summary>
    internal partial class AspectClass : IAspectClass
    {
        private readonly Dictionary<string, TemplateDriver> _templateDrivers = new( StringComparer.Ordinal );

        public ImmutableDictionary<string, AspectClassMember> Members { get; }

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

        public CompileTimeProject? Project { get; }

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
        protected internal AspectClass(
            IServiceProvider serviceProvider,
            INamedTypeSymbol aspectTypeSymbol,
            AspectClass? baseClass,
            CompileTimeProject? project,
            Type aspectType,
            IAspect? prototype,
            IDiagnosticAdder diagnosticAdder,
            Compilation compilation,
            AspectDriverFactory aspectDriverFactory )
        {
            this.FullName = aspectTypeSymbol.GetReflectionNameSafe();
            this.DisplayName = aspectTypeSymbol.Name.TrimEnd( "Attribute" );
            this.IsAbstract = aspectTypeSymbol.IsAbstract;
            this.BaseClass = baseClass;
            this.Project = project;
            this._serviceProvider = serviceProvider;
            this._userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
            this.DiagnosticLocation = aspectTypeSymbol.GetDiagnosticLocation();
            this.AspectType = aspectType;
            this._prototypeAspectInstance = prototype;
            this.Members = this.GetMembers( compilation, aspectTypeSymbol, diagnosticAdder );

            // This must be called after Members is built and assigned.
            this._aspectDriver = aspectDriverFactory.GetAspectDriver( this, aspectTypeSymbol );
        }

        public bool TryGetInterfaceMember( ISymbol symbol, [NotNullWhen( true )] out AspectClassMember? member )
            => this.Members.TryGetValue( DocumentationCommentId.CreateDeclarationId( symbol ), out member )
               && member.TemplateInfo.AttributeType == TemplateAttributeType.InterfaceMember;

        private ImmutableDictionary<string, AspectClassMember> GetMembers( Compilation compilation, INamedTypeSymbol type, IDiagnosticAdder diagnosticAdder )
        {
            if ( compilation == null! )
            {
                // This is a test scenario where templates must not be detected.
                return ImmutableDictionary<string, AspectClassMember>.Empty;
            }

            var symbolClassifier = this._serviceProvider.GetService<SymbolClassificationService>().GetClassifier( compilation );

            var members = this.BaseClass?.Members.ToBuilder()
                          ?? ImmutableDictionary.CreateBuilder<string, AspectClassMember>( StringComparer.Ordinal );

            foreach ( var memberSymbol in type.GetMembers() )
            {
                var templateInfo = symbolClassifier.GetTemplateInfo( memberSymbol ).AssertNotNull();
                var memberName = memberSymbol.Name;

                if ( templateInfo.AttributeType == TemplateAttributeType.Introduction && memberSymbol is IMethodSymbol { AssociatedSymbol: not null } )
                {
                    // This is an accessor of an introduced event or property. We don't index them.
                    continue;
                }
                else if ( templateInfo.AttributeType == TemplateAttributeType.InterfaceMember )
                {
                    // For interface members, we don't require a unique name.
                    memberName = DocumentationCommentId.CreateDeclarationId( memberSymbol );
                }

                var aspectClassMember = new AspectClassMember(
                    memberName,
                    this,
                    templateInfo,
                    memberSymbol is IMethodSymbol { IsAsync: true },
                    memberSymbol );

                if ( !templateInfo.IsNone )
                {
                    if ( members.TryGetValue( memberName, out var existingMember ) && !memberSymbol.IsOverride &&
                         !existingMember.TemplateInfo.IsNone )
                    {
                        // Note we cannot get here when the member is defined in the same type because the compile-time assembly creation
                        // would have failed. The

                        // The template is already defined and we are not overwriting a template of the base class.
                        diagnosticAdder.Report(
                            GeneralDiagnosticDescriptors.TemplateWithSameNameAlreadyDefinedInBaseClass.CreateDiagnostic(
                                memberSymbol.GetDiagnosticLocation(),
                                (memberName, type.Name, existingMember.AspectClass.AspectType.Name) ) );

                        continue;
                    }

                    // Add or replace the template.
                    members[memberName] = aspectClassMember;
                }
                else
                {
                    if ( !members.ContainsKey( memberName ) )
                    {
                        members.Add( memberName, aspectClassMember );
                    }
                }
            }

            return members.ToImmutable();
        }

        private bool TryInitialize( IDiagnosticAdder diagnosticAdder )
        {
            if ( this._prototypeAspectInstance != null )
            {
                var builder = new Builder( this );

                try
                {
                    this._userCodeInvoker.Invoke( () => this._prototypeAspectInstance.BuildAspectClass( builder ) );
                }
                catch ( Exception e )
                {
                    var diagnostic = GeneralDiagnosticDescriptors.ExceptionInUserCode.CreateDiagnostic(
                        null,
                        (AspectType: this.DisplayName, MethodName: nameof(IAspect.BuildAspectClass), e.GetType().Name, e.Format( 5 )) );

                    diagnosticAdder.Report( diagnostic );

                    return false;
                }

                this._layers = builder.Layers.As<string?>().Prepend( null ).Select( l => new AspectLayer( this, l ) ).ToImmutableArray();
            }
            else
            {
                // Abstract aspect classes don't have any layer.
                this._layers = Array.Empty<AspectLayer>();
            }

            // TODO: get all eligibility rules from the prototype instance and combine them into a single rule.

            return true;
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
                ITypeSymbol { TypeKind: TypeKind.TypeParameter } => typeof(IGenericParameter),
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

        public override string ToString() => this.FullName;
    }
}