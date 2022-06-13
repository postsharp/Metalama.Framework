// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.Advices
{
    internal partial class ImplementInterfaceAdvice : Advice
    {
        private readonly List<InterfaceSpecification> _interfaceSpecifications;

        public IReadOnlyList<InterfaceMemberSpecification>? ExplicitMemberSpecifications { get; }

        public INamedType InterfaceType { get; }

        public OverrideStrategy OverrideStrategy { get; }

        public new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

        public IObjectReader Tags { get; }

        public ImplementInterfaceAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance template,
            INamedType targetType,
            INamedType interfaceType,
            OverrideStrategy overrideStrategy,
            IReadOnlyList<InterfaceMemberSpecification>? explicitMemberSpecifications,
            string? layerName,
            IObjectReader tags ) : base( aspect, template, targetType, layerName )
        {
            this.InterfaceType = interfaceType;
            this.ExplicitMemberSpecifications = explicitMemberSpecifications;
            this.OverrideStrategy = overrideStrategy;
            this._interfaceSpecifications = new List<InterfaceSpecification>();
            this.Tags = tags;
        }

        public override void Initialize( IServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
        {
            // When initializing, it is not known which types the target type is implementing.
            // Therefore, a specification for all interfaces should be prepared and only diagnostics related advice parameters and aspect class
            // should be reported.            

            var aspectTypeName = this.Aspect.AspectClass.FullName.AssertNotNull();
            var aspectType = this.SourceCompilation.GetCompilationModel().Factory.GetTypeByReflectionName( aspectTypeName );

            // We introduce all interfaces except the base interfaces that were added before. That means that the previous introductions
            // have precedence.
            var interfacesToIntroduce =
                new[] { (this.InterfaceType, IsTopLevel: true) }
                    .Concat( this.InterfaceType.AllImplementedInterfaces.Select( i => (InterfaceType: i, IsTopLevel: false) ) )
                    .ToDictionary( x => x.InterfaceType, x => x.IsTopLevel, this.SourceCompilation.InvariantComparer );

            if ( this.ExplicitMemberSpecifications != null )
            {
                // TODO: When interface member is not specified but there is an equal visible member in the base class, C# will take use it, so
                //       we can allow the user to specify member from the base class and not necessarily a new builder.
                throw new NotImplementedException();
            }

            // No explicit member specification was given, we have to detect introduced members corresponding to all interface members.
            foreach ( var pair in interfacesToIntroduce )
            {
                var introducedInterface = pair.Key;
                var isTopLevel = pair.Value;
                List<MemberSpecification> memberSpecifications = new();

                foreach ( var interfaceMethod in introducedInterface.Methods )
                {
                    if ( !TryGetAspectInterfaceMethod( interfaceMethod, out var matchingMethod, out var matchingTemplate ) )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateRoslynDiagnostic(
                                this.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), this.InterfaceType,
                                 interfaceMethod) ) );
                    }
                    else if (
                        !SignatureTypeSymbolComparer.Instance.Equals(
                            interfaceMethod.ReturnParameter.Type.GetSymbol().AssertNotNull(),
                            matchingMethod.ReturnParameter.Type.GetSymbol().AssertNotNull() )
                        || interfaceMethod.ReturnParameter.RefKind != matchingMethod.ReturnParameter.RefKind )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateRoslynDiagnostic(
                                this.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), this.InterfaceType,
                                 matchingMethod,
                                 interfaceMethod) ) );
                    }
                    else
                    {
                        memberSpecifications.Add( new MemberSpecification( interfaceMethod, null, matchingMethod, matchingTemplate, null ) );
                    }
                }

                foreach ( var interfaceIndexer in introducedInterface.Indexers )
                {
                    throw new NotImplementedException();
                }

                foreach ( var interfaceProperty in introducedInterface.Properties )
                {
                    if ( !TryGetAspectInterfaceProperty( interfaceProperty, out var matchingProperty, out var matchingTemplate ) )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateRoslynDiagnostic(
                                this.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), this.InterfaceType,
                                 interfaceProperty) ) );
                    }
                    else if (
                        !this.SourceCompilation.InvariantComparer.Equals( interfaceProperty.Type, matchingProperty.Type )
                        || interfaceProperty.RefKind != matchingProperty.RefKind )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateRoslynDiagnostic(
                                this.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), this.InterfaceType,
                                 matchingProperty,
                                 interfaceProperty) ) );
                    }
                    else
                    {
                        memberSpecifications.Add( new MemberSpecification( interfaceProperty, null, matchingProperty, matchingTemplate, null ) );
                    }
                }

                foreach ( var interfaceEvent in introducedInterface.Events )
                {
                    if ( !TryGetAspectInterfaceEvent( interfaceEvent, out var matchingEvent, out var matchingTemplate ) )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateRoslynDiagnostic(
                                this.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), this.InterfaceType,
                                 interfaceEvent) ) );
                    }
                    else if ( !this.SourceCompilation.InvariantComparer.Equals( interfaceEvent.Type, matchingEvent.Type ) )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateRoslynDiagnostic(
                                this.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), this.InterfaceType,
                                 matchingEvent,
                                 interfaceEvent) ) );
                    }
                    else
                    {
                        memberSpecifications.Add( new MemberSpecification( interfaceEvent, null, matchingEvent, matchingTemplate, null ) );
                    }
                }

                this._interfaceSpecifications.Add( new InterfaceSpecification( introducedInterface, isTopLevel, memberSpecifications ) );
            }

            bool TryGetAspectInterfaceMethod(
                IMethod interfaceMethod,
                [NotNullWhen( true )] out IMethod? aspectMethod,
                [NotNullWhen( true )] out TemplateClassMember? templateClassMember )
            {
                var method = aspectType.AllMethods.SingleOrDefault( m => m.SignatureEquals( interfaceMethod ) );

                if ( method != null && TryGetInterfaceMemberTemplate( method, out var classMember ) )
                {
                    aspectMethod = method;
                    templateClassMember = classMember;

                    return true;
                }

                aspectMethod = null;
                templateClassMember = null;

                return false;
            }

            bool TryGetAspectInterfaceProperty(
                IProperty interfaceProperty,
                [NotNullWhen( true )] out IProperty? aspectProperty,
                [NotNullWhen( true )] out TemplateClassMember? templateClassMember )
            {
                var property = aspectType.AllProperties.SingleOrDefault( p => p.SignatureEquals( interfaceProperty ) );

                if ( property != null && TryGetInterfaceMemberTemplate( property, out var classMember ) )
                {
                    aspectProperty = property;
                    templateClassMember = classMember;

                    return true;
                }

                aspectProperty = null;
                templateClassMember = null;

                return false;
            }

            bool TryGetAspectInterfaceEvent(
                IEvent interfaceEvent,
                [NotNullWhen( true )] out IEvent? aspectEvent,
                [NotNullWhen( true )] out TemplateClassMember? templateClassMember )
            {
                var @event = aspectType.AllEvents.SingleOrDefault( e => e.SignatureEquals( interfaceEvent ) );

                if ( @event != null && TryGetInterfaceMemberTemplate( @event, out var classMember ) )
                {
                    aspectEvent = @event;
                    templateClassMember = classMember;

                    return true;
                }

                aspectEvent = null;
                templateClassMember = null;

                return false;
            }

            bool TryGetInterfaceMemberTemplate(
                IMember member,
                [NotNullWhen( true )] out TemplateClassMember? templateClassMember )
            {
                return this.TemplateInstance.TemplateClass.TryGetInterfaceMember(
                    member.GetSymbol().AssertNotNull( Justifications.ImplementingIntroducedInterfacesNotSupported ),
                    out templateClassMember );
            }
        }

        public override AdviceImplementationResult Implement( IServiceProvider serviceProvider, ICompilation compilation )
        {
            // Adding interfaces may run into three problems:
            //      1) Target type already implements the interface.
            //      2) Target type already implements an ancestor of the interface.

            var targetType = this.TargetDeclaration.GetTarget( compilation );
            var diagnosticList = new DiagnosticList();

            var transformations = new List<ITransformation>();

            foreach ( var interfaceSpecification in this._interfaceSpecifications )
            {
                // Validate that the interface must be introduced to the specific target.

                if ( targetType.AllImplementedInterfaces.Any(
                        t => compilation.GetCompilationModel().InvariantComparer.Equals( t, interfaceSpecification.InterfaceType ) ) )
                {
                    // Conflict on the introduced interface itself.
                    switch ( this.OverrideStrategy )
                    {
                        case OverrideStrategy.Fail:
                            // Report the diagnostic.
                            diagnosticList.Report(
                                AdviceDiagnosticDescriptors.InterfaceIsAlreadyImplemented.CreateRoslynDiagnostic(
                                    targetType.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, interfaceSpecification.InterfaceType, targetType) ) );

                            break;

                        case OverrideStrategy.Ignore:
                            // Nothing to do.
                            break;

                        default:
                            throw new NotImplementedException( $"The OverrideStrategy {this.OverrideStrategy} is not implemented." );
                    }

                    continue;
                }

                var interfaceMemberMap = new Dictionary<IMember, IMember>();

                foreach ( var memberSpec in interfaceSpecification.MemberSpecifications )
                {
                    // Collect implemented interface members and add non-observable transformations.
                    MemberBuilder memberBuilder;

                    var mergedTags = ObjectReader.Merge( this.Tags, memberSpec.Tags );

                    switch ( memberSpec.InterfaceMember )
                    {
                        case IMethod interfaceMethod:
                            var existingMethod = targetType.Methods.SingleOrDefault( m => m.SignatureEquals( interfaceMethod ) );

                            if ( existingMethod != null && !memberSpec.IsExplicit )
                            {
                                // TODO: Handle WhenExists.
                                diagnosticList.Report(
                                    AdviceDiagnosticDescriptors.ImplicitInterfaceMemberConflict.CreateRoslynDiagnostic(
                                        targetType.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, interfaceSpecification.InterfaceType, targetType, existingMethod) ) );

                                continue;
                            }

                            var aspectMethod = (IMethod) memberSpec.AspectInterfaceMember!;
                            memberBuilder = this.GetImplMethodBuilder( targetType, interfaceMethod, memberSpec.IsExplicit, mergedTags );
                            interfaceMemberMap.Add( interfaceMethod, memberBuilder );

                            transformations.Add(
                                memberSpec.AspectInterfaceMember != null
                                    ? new OverrideMethodTransformation(
                                        this,
                                        (IMethod) memberBuilder,
                                        TemplateMember.Create(
                                                aspectMethod,
                                                memberSpec.TemplateClassMember,
                                                memberSpec.TemplateClassMember.TemplateInfo.Attribute.AssertNotNull(),
                                                TemplateKind.Introduction )
                                            .ForIntroduction(),
                                        mergedTags )
                                    : new RedirectMethodTransformation(
                                        this,
                                        (IMethod) memberBuilder,
                                        (IMethod) memberSpec.TargetMember.AssertNotNull(),
                                        mergedTags ) );

                            break;

                        case IProperty interfaceProperty:
                            var existingProperty = targetType.Properties.SingleOrDefault( p => p.SignatureEquals( interfaceProperty ) );

                            if ( existingProperty != null && !memberSpec.IsExplicit )
                            {
                                // TODO: Handle WhenExists.
                                diagnosticList.Report(
                                    AdviceDiagnosticDescriptors.ImplicitInterfaceMemberConflict.CreateRoslynDiagnostic(
                                        targetType.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, interfaceSpecification.InterfaceType, targetType, existingProperty) ) );

                                continue;
                            }

                            var aspectProperty = (IProperty?) memberSpec.AspectInterfaceMember;
                            var buildAutoProperty = aspectProperty?.IsAutoPropertyOrField == true;

                            var propertyBuilder = this.GetImplPropertyBuilder(
                                targetType,
                                interfaceProperty,
                                (IProperty?) memberSpec.TargetMember ?? (IProperty) memberSpec.AspectInterfaceMember.AssertNotNull(),
                                buildAutoProperty,
                                memberSpec.IsExplicit,
                                aspectProperty?.SetMethod?.IsImplicit ?? false,
                                mergedTags );

                            memberBuilder = propertyBuilder;

                            interfaceMemberMap.Add( interfaceProperty, memberBuilder );

                            if ( aspectProperty?.IsAutoPropertyOrField != true )
                            {
                                var propertyTemplate = TemplateMember.Create( aspectProperty, memberSpec.TemplateClassMember, TemplateKind.Introduction );
                                var accessorTemplates = propertyTemplate.GetAccessorTemplates();

                                transformations.Add(
                                    memberSpec.AspectInterfaceMember != null
                                        ? new OverridePropertyTransformation(
                                            this,
                                            (IProperty) memberBuilder,
                                            accessorTemplates.Get.ForOverride( propertyBuilder.GetMethod ),
                                            accessorTemplates.Set.ForOverride( propertyBuilder.SetMethod ),
                                            mergedTags )
                                        : new RedirectPropertyTransformation(
                                            this,
                                            (IProperty) memberBuilder,
                                            (IProperty) memberSpec.TargetMember.AssertNotNull(),
                                            mergedTags ) );
                            }

                            break;

                        case IIndexer:
                            throw new NotImplementedException();

                        case IEvent interfaceEvent:
                            var existingEvent = targetType.Events.SingleOrDefault( p => p.SignatureEquals( interfaceEvent ) );

                            if ( existingEvent != null && !memberSpec.IsExplicit )
                            {
                                // TODO: Handle WhenExists.
                                diagnosticList.Report(
                                    AdviceDiagnosticDescriptors.ImplicitInterfaceMemberConflict.CreateRoslynDiagnostic(
                                        targetType.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, interfaceSpecification.InterfaceType, targetType, existingEvent) ) );

                                continue;
                            }

                            var aspectEvent = memberSpec.AspectInterfaceMember;
                            var isEventField = aspectEvent != null && ((IEvent) aspectEvent).IsEventField();

                            memberBuilder = this.GetImplEventBuilder( targetType, interfaceEvent, isEventField, memberSpec.IsExplicit );
                            interfaceMemberMap.Add( interfaceEvent, memberBuilder );

                            if ( !isEventField )
                            {
                                transformations.Add(
                                    memberSpec.AspectInterfaceMember != null
                                        ? new OverrideEventTransformation(
                                            this,
                                            (IEvent) memberBuilder,
                                            TemplateMember.Create(
                                                (IEvent) memberSpec.AspectInterfaceMember,
                                                memberSpec.TemplateClassMember,
                                                TemplateKind.Introduction ),
                                            default,
                                            default,
                                            mergedTags,
                                            null )
                                        : new RedirectEventTransformation(
                                            this,
                                            (IEvent) memberBuilder,
                                            (IEvent) memberSpec.TargetMember.AssertNotNull(),
                                            mergedTags ) );
                            }

                            break;

                        default:
                            throw new AssertionFailedException();
                    }

                    transformations.Add( memberBuilder );
                }

                if ( interfaceSpecification.IsTopLevel )
                {
                    // We are adding the interface only when 
                    transformations.Add( new IntroduceInterfaceTransformation( this, targetType, interfaceSpecification.InterfaceType, interfaceMemberMap ) );
                }
            }

            return AdviceImplementationResult.Create( transformations ).WithDiagnostics( diagnosticList.ToArray() );
        }

        private MemberBuilder GetImplMethodBuilder(
            INamedType declaringType,
            IMethod interfaceMethod,
            bool isExplicit,
            IObjectReader tags )
        {
            var methodBuilder = new MethodBuilder( this, declaringType, interfaceMethod.Name, tags )
            {
                ReturnParameter = { Type = interfaceMethod.ReturnParameter.Type, RefKind = interfaceMethod.ReturnParameter.RefKind }
            };

            foreach ( var interfaceParameter in interfaceMethod.Parameters )
            {
                _ = methodBuilder.AddParameter(
                    interfaceParameter.Name,
                    interfaceParameter.Type,
                    interfaceParameter.RefKind,
                    interfaceParameter.DefaultValue );
            }

            foreach ( var interfaceGenericParameter in interfaceMethod.TypeParameters )
            {
                // TODO: Move this initialization into a second overload of add generic parameter.
                var genericParameterBuilder = methodBuilder.AddTypeParameter( interfaceGenericParameter.Name );
                genericParameterBuilder.Variance = interfaceGenericParameter.Variance;
                genericParameterBuilder.TypeKindConstraint = interfaceGenericParameter.TypeKindConstraint;
                genericParameterBuilder.HasDefaultConstructorConstraint = interfaceGenericParameter.HasDefaultConstructorConstraint;

                foreach ( var templateGenericParameterConstraint in genericParameterBuilder.TypeConstraints )
                {
                    genericParameterBuilder.AddTypeConstraint( templateGenericParameterConstraint );
                }
            }

            if ( isExplicit )
            {
                methodBuilder.SetExplicitInterfaceImplementation( interfaceMethod );
            }
            else
            {
                methodBuilder.Accessibility = Accessibility.Public;
            }

            return methodBuilder;
        }

        private PropertyBuilder GetImplPropertyBuilder(
            INamedType declaringType,
            IProperty interfaceProperty,
            IProperty targetProperty,
            bool isAutoProperty,
            bool isExplicit,
            bool hasImplicitSetter,
            IObjectReader tags )
        {
            var propertyBuilder = new PropertyBuilder(
                this,
                declaringType,
                interfaceProperty.Name,
                interfaceProperty.GetMethod != null || (!isExplicit && targetProperty.GetMethod != null),
                interfaceProperty.SetMethod != null || (!isExplicit && targetProperty.SetMethod != null),
                isAutoProperty,
                interfaceProperty.Writeability == Writeability.InitOnly,
                false,
                hasImplicitSetter,
                tags );

            propertyBuilder.Type = interfaceProperty.Type;

            if ( isExplicit )
            {
                propertyBuilder.SetExplicitInterfaceImplementation( interfaceProperty );
            }
            else
            {
                propertyBuilder.Accessibility = Accessibility.Public;

                if ( propertyBuilder.GetMethod != null )
                {
                    if ( interfaceProperty.GetMethod != null )
                    {
                        propertyBuilder.GetMethod.Accessibility = Accessibility.Public;
                    }
                    else
                    {
                        propertyBuilder.GetMethod.Accessibility = targetProperty.GetMethod.AssertNotNull().Accessibility;
                    }
                }

                if ( propertyBuilder.SetMethod != null )
                {
                    if ( interfaceProperty.SetMethod != null )
                    {
                        propertyBuilder.SetMethod.Accessibility = Accessibility.Public;
                    }
                    else
                    {
                        propertyBuilder.SetMethod.Accessibility = targetProperty.SetMethod.AssertNotNull().Accessibility;
                    }
                }
            }

            return propertyBuilder;
        }

        private Location? GetDiagnosticLocation() => this.TargetDeclaration.GetTarget( this.SourceCompilation ).GetDiagnosticLocation();

        private MemberBuilder GetImplEventBuilder( INamedType declaringType, IEvent interfaceEvent, bool isEventField, bool isExplicit )
        {
            var eventBuilder = new EventBuilder(
                this,
                declaringType,
                interfaceEvent.Name,
                isEventField,
                this.Tags ) { Type = interfaceEvent.Type };

            if ( isExplicit )
            {
                eventBuilder.SetExplicitInterfaceImplementation( interfaceEvent );
            }
            else
            {
                eventBuilder.Accessibility = Accessibility.Public;
            }

            return eventBuilder;
        }
    }
}