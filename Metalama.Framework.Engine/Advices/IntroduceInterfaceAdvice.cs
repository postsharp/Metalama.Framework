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
        private readonly List<(IMethod Method, TemplateInfo TemplateInfo)> _aspectInterfaceMethods = new();
        private readonly List<(IProperty Property, TemplateInfo TemplateInfo)> _aspectInterfaceProperties = new();

        private readonly List<(IEvent Event, TemplateInfo TemplateInfo)> _aspectInterfaceEvents = new();

        private readonly List<IntroducedInterfaceSpecification> _introducedInterfaceTypes;

        private new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

        public ImplementInterfaceAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance template,
            INamedType targetType,
            string? layerName ) : base( aspect, template, targetType, layerName, TagReader.Empty )
        {
            this._introducedInterfaceTypes = new List<IntroducedInterfaceSpecification>();
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            var aspectTypeName = this.Aspect.AspectClass.FullName.AssertNotNull();
            var compilation = this.SourceCompilation;
            var aspectType = compilation.TypeFactory.GetTypeByReflectionName( aspectTypeName );

            foreach ( var aspectMethod in aspectType.Methods )
            {
                if ( TryGetInterfaceMemberTemplateInfo( aspectMethod, out var interfaceMemberAttribute ) )
                {
                    this._aspectInterfaceMethods.Add( (aspectMethod, interfaceMemberAttribute) );
                }
            }

            foreach ( var aspectProperty in aspectType.Properties )
            {
                if ( TryGetInterfaceMemberTemplateInfo( aspectProperty, out var interfaceMemberAttribute ) )
                {
                    this._aspectInterfaceProperties.Add( (aspectProperty, interfaceMemberAttribute) );
                }
            }

            foreach ( var aspectEvent in aspectType.Events )
            {
                if ( TryGetInterfaceMemberTemplateInfo( aspectEvent, out var interfaceMemberAttribute ) )
                {
                    this._aspectInterfaceEvents.Add( (aspectEvent, interfaceMemberAttribute) );
                }
            }

            bool TryGetInterfaceMemberTemplateInfo(
                IMember member,
                [NotNullWhen( true )] out TemplateInfo? templateInfo )
            {
                if ( this.TemplateInstance.TemplateClass.TryGetInterfaceMember(
                        member.GetSymbol().AssertNotNull( Justifications.ImplementingIntroducedInterfacesNotSupported ),
                        out var aspectClassMember ) )
                {
                    templateInfo = aspectClassMember.TemplateInfo;

                    return true;
                }
                else
                {
                    templateInfo = null;

                    return false;
                }
            }
        }

        public void AddInterfaceImplementation(
            INamedType interfaceType,
            OverrideStrategy overrideStrategy,
            IReadOnlyList<InterfaceMemberSpecification>? explicitMemberSpecification,
            IDiagnosticAdder diagnosticAdder,
            ITagReader tags )
        {
            // Adding interfaces may run into three problems:
            //      1) Target type already implements the interface.
            //      2) Target type already implements an ancestor of the interface.
            //      3) The interface or it's ancestor was implemented by another ImplementInterface call.

            var compilation = interfaceType.Compilation;
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            Location? GetDiagnosticLocation() => targetDeclaration.GetDiagnosticLocation();

            bool AlreadyContainsInterface( INamedType i ) => this._introducedInterfaceTypes.Any( x => x.InterfaceType.Equals( i ) );

            if ( AlreadyContainsInterface( interfaceType ) )
            {
                // The aspect conflicts with itself, introducing the base interface after the derived interface.
                diagnosticAdder.Report(
                    AdviceDiagnosticDescriptors.InterfaceIsAlreadyIntroducedByTheAspect.CreateRoslynDiagnostic(
                        GetDiagnosticLocation(),
                        (this.Aspect.AspectClass.ShortName, interfaceType, targetDeclaration) ) );
            }

            // We introduce all interfaces except the base interfaces that were added before. That means that the previous introductions
            // have precedence.
            var interfacesToIntroduce = new HashSet<INamedType>(
                new[] { interfaceType }.Concat( interfaceType.AllImplementedInterfaces ).Where( i => !AlreadyContainsInterface( i ) ),
                compilation.InvariantComparer );

            if ( explicitMemberSpecification != null )
            {
                throw new NotImplementedException();
            }

            // No explicit member specification was given, we have to detect introduced members corresponding to all interface members.

            foreach ( var introducedInterface in interfacesToIntroduce )
            {
                List<MemberSpecification> memberSpecifications = new();

                foreach ( var interfaceMethod in introducedInterface.Methods )
                {
                    var matchingAspectMethod =
                        this._aspectInterfaceMethods
                            .SingleOrDefault( am => am.Method.SignatureEquals( interfaceMethod ) );

                    if ( matchingAspectMethod.Method == null )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateRoslynDiagnostic(
                                GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, targetDeclaration, interfaceType, interfaceMethod) ) );
                    }
                    else if (
                        !compilation.InvariantComparer.Equals(
                            interfaceMethod.ReturnParameter.Type,
                            matchingAspectMethod.Method.ReturnParameter.Type )
                        || interfaceMethod.ReturnParameter.RefKind != matchingAspectMethod.Method.ReturnParameter.RefKind )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateRoslynDiagnostic(
                                GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, targetDeclaration, interfaceType, matchingAspectMethod.Method,
                                 interfaceMethod) ) );
                    }
                    else
                    {
                        memberSpecifications.Add(
                            new MemberSpecification( interfaceMethod, null, matchingAspectMethod.Method, matchingAspectMethod.TemplateInfo, tags ) );
                    }
                }

                foreach ( var interfaceProperty in introducedInterface.Properties )
                {
                    var matchingAspectProperty =
                        this._aspectInterfaceProperties
                            .SingleOrDefault( ap => ap.Property.SignatureEquals( interfaceProperty ) );

                    if ( matchingAspectProperty.Property == null )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateRoslynDiagnostic(
                                GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, targetDeclaration, interfaceType, interfaceProperty) ) );
                    }
                    else if (
                        !compilation.InvariantComparer.Equals( interfaceProperty.Type, matchingAspectProperty.Property.Type )
                        || interfaceProperty.RefKind != matchingAspectProperty.Property.RefKind )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateRoslynDiagnostic(
                                GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, targetDeclaration, interfaceType, matchingAspectProperty.Property,
                                 interfaceProperty) ) );
                    }
                    else
                    {
                        memberSpecifications.Add(
                            new MemberSpecification(
                                interfaceProperty,
                                null,
                                matchingAspectProperty.Property,
                                matchingAspectProperty.TemplateInfo,
                                tags ) );
                    }
                }

                foreach ( var interfaceEvent in introducedInterface.Events )
                {
                    var matchingAspectEvent = this._aspectInterfaceEvents.SingleOrDefault( ae => ae.Event.Name == interfaceEvent.Name );

                    if ( matchingAspectEvent.Event == null )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateRoslynDiagnostic(
                                GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, targetDeclaration, interfaceType, interfaceEvent) ) );
                    }
                    else if ( !compilation.InvariantComparer.Equals( interfaceEvent.Type, matchingAspectEvent.Event.Type ) )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateRoslynDiagnostic(
                                GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, targetDeclaration, interfaceType, matchingAspectEvent.Event,
                                 interfaceEvent) ) );
                    }
                    else
                    {
                        memberSpecifications.Add(
                            new MemberSpecification( interfaceEvent, null, matchingAspectEvent.Event, matchingAspectEvent.TemplateInfo, tags ) );
                    }
                }

                this._introducedInterfaceTypes.Add( new IntroducedInterfaceSpecification( introducedInterface, memberSpecifications, overrideStrategy ) );
            }
        }

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var transformations = new List<ITransformation>();

            foreach ( var interfaceSpec in this._introducedInterfaceTypes )
            {
                // Validate that the interface must be introduced to the specific target.

                if ( targetDeclaration.AllImplementedInterfaces.Any(
                        t => compilation.GetCompilationModel().InvariantComparer.Equals( t, interfaceSpec.InterfaceType ) ) )
                {
                    // Conflict on the introduced interface itself.
                    switch ( interfaceSpec.OverrideStrategy )
                    {
                        case OverrideStrategy.Fail:
                            // Report the diagnostic and return.
                            return AdviceResult.Create(
                                AdviceDiagnosticDescriptors.InterfaceIsAlreadyImplemented.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, interfaceSpec.InterfaceType, targetDeclaration) ) );

                        case OverrideStrategy.Ignore:
                            // Nothing to do.
                            continue;

                        default:
                            throw new NotImplementedException( $"The OverrideStrategy {interfaceSpec.OverrideStrategy} is not implemented." );
                    }
                }

                var interfaceMemberMap = new Dictionary<IMember, IMember>();

                foreach ( var memberSpec in interfaceSpec.MemberSpecifications )
                {
                    // Collect implemented interface members and add non-observable transformations.
                    MemberBuilder memberBuilder;

                    switch ( memberSpec.InterfaceMember )
                    {
                        case IMethod interfaceMethod:
                            memberBuilder = this.GetImplMethodBuilder( targetDeclaration, interfaceMethod, memberSpec.IsExplicit, memberSpec.Tags );
                            interfaceMemberMap.Add( interfaceMethod, memberBuilder );

                            var implementationMethod = (IMethod) memberSpec.AspectInterfaceMember!;

                            transformations.Add(
                                memberSpec.AspectInterfaceMember != null
                                    ? new OverriddenMethod(
                                        this,
                                        (IMethod) memberBuilder,
                                        TemplateMember.Create( implementationMethod, memberSpec.TemplateInfo, TemplateKind.Introduction ),
                                        memberSpec.Tags )
                                    : new RedirectedMethod(
                                        this,
                                        (IMethod) memberBuilder,
                                        (IMethod) memberSpec.TargetMember.AssertNotNull(),
                                        memberSpec.Tags ) );

                            break;

                        case IProperty interfaceProperty:
                            var aspectProperty = (IProperty?) memberSpec.AspectInterfaceMember;
                            var buildAutoProperty = aspectProperty?.IsAutoPropertyOrField == true;

                            memberBuilder = this.GetImplPropertyBuilder(
                                targetDeclaration,
                                interfaceProperty,
                                (IProperty?) memberSpec.TargetMember ?? (IProperty) memberSpec.AspectInterfaceMember.AssertNotNull(),
                                buildAutoProperty,
                                memberSpec.IsExplicit,
                                memberSpec.Tags );

                            interfaceMemberMap.Add( interfaceProperty, memberBuilder );

                            if ( aspectProperty?.IsAutoPropertyOrField != true )
                            {
                                var propertyTemplate = TemplateMember.Create( aspectProperty, memberSpec.TemplateInfo, TemplateKind.Introduction );
                                var accessorTemplates = propertyTemplate.GetAccessorTemplates();

                                transformations.Add(
                                    memberSpec.AspectInterfaceMember != null
                                        ? new OverriddenProperty(
                                            this,
                                            (IProperty) memberBuilder,
                                            propertyTemplate,
                                            accessorTemplates.Get,
                                            accessorTemplates.Set,
                                            memberSpec.Tags )
                                        : new RedirectedProperty(
                                            this,
                                            (IProperty) memberBuilder,
                                            (IProperty) memberSpec.TargetMember.AssertNotNull(),
                                            memberSpec.Tags ) );
                            }

                            break;

                        case IEvent interfaceEvent:
                            var isEventField = memberSpec.AspectInterfaceMember != null
                                               && ((IEvent) memberSpec.AspectInterfaceMember).IsEventField();

                            memberBuilder = this.GetImplEventBuilder( targetDeclaration, interfaceEvent, isEventField, memberSpec.IsExplicit );
                            interfaceMemberMap.Add( interfaceEvent, memberBuilder );

                            if ( !isEventField )
                            {
                                transformations.Add(
                                    memberSpec.AspectInterfaceMember != null
                                        ? new OverriddenEvent(
                                            this,
                                            (IEvent) memberBuilder,
                                            TemplateMember.Create(
                                                (IEvent) memberSpec.AspectInterfaceMember,
                                                memberSpec.TemplateInfo,
                                                TemplateKind.Introduction ),
                                            default,
                                            default,
                                            memberSpec.Tags )
                                        : new RedirectedEvent(
                                            this,
                                            (IEvent) memberBuilder,
                                            (IEvent) memberSpec.TargetMember.AssertNotNull(),
                                            memberSpec.Tags ) );
                            }

                            break;

                        default:
                            throw new AssertionFailedException();
                    }

                    transformations.Add( memberBuilder );
                }

                transformations.Add( new IntroducedInterface( this, targetDeclaration, interfaceSpec.InterfaceType, interfaceMemberMap ) );
            }

            return AdviceResult.Create( transformations );
        }

        private MemberBuilder GetImplMethodBuilder(
            INamedType declaringType,
            IMethod interfaceMethod,
            bool isExplicit,
            ITagReader tags )
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

        private MemberBuilder GetImplPropertyBuilder(
            INamedType declaringType,
            IProperty interfaceProperty,
            IProperty targetProperty,
            bool isAutoProperty,
            bool isExplicit,
            ITagReader tags )
        {
            var propertyBuilder = new PropertyBuilder(
                this,
                declaringType,
                interfaceProperty.Name,
                interfaceProperty.GetMethod != null || (!isExplicit && targetProperty.GetMethod != null),
                interfaceProperty.SetMethod != null || (!isExplicit && targetProperty.SetMethod != null),
                isAutoProperty,
                interfaceProperty.Writeability == Writeability.InitOnly,
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