// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal partial class ImplementInterfaceAdvice : Advice
    {
        private readonly List<(IMethod Method, TemplateInfo TemplateInfo)> _aspectInterfaceMethods = new();
        private readonly List<(IProperty Property, TemplateInfo TemplateInfo)> _aspectInterfaceProperties = new();
        private readonly List<(IEvent Event, TemplateInfo TemplateInfo)> _aspectInterfaceEvents = new();
        private readonly Dictionary<INamedType, (bool IsIntroduced, bool Dummy)> _introducedAndImplementedInterfaces;
        private readonly List<IntroducedInterfaceSpecification> _introducedInterfaceTypes;

        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public ImplementInterfaceAdvice(
            AspectInstance aspect,
            INamedType targetType,
            string? layerName ) : base( aspect, targetType, layerName, null )
        {
            this._introducedInterfaceTypes = new List<IntroducedInterfaceSpecification>();

            // Initialize with interface the target type already implements.
            this._introducedAndImplementedInterfaces = targetType.AllImplementedInterfaces.ToDictionary(
                x => x,
                _ => (false, false),
                targetType.Compilation.InvariantComparer );
        }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            var aspectTypeName = this.Aspect.AspectClass.AspectType.FullName.AssertNotNull();
            var compilation = this.TargetDeclaration.Compilation;
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
                if ( this.Aspect.AspectClass.TryGetInterfaceMember(
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
            Dictionary<string, object?>? tags )
        {
            // Adding interfaces may run into three problems:
            //      1) Target type already implements the interface.
            //      2) Target type already implements an ancestor of the interface.
            //      3) The interface or it's ancestor was implemented by another ImplementInterface call.

            if ( this._introducedAndImplementedInterfaces.TryGetValue( interfaceType, out var impl ) && impl.IsIntroduced )
            {
                // The aspect conflicts with itself, introducing the base interface after the derived interface.
                diagnosticAdder.Report(
                    AdviceDiagnosticDescriptors.InterfaceIsAlreadyIntroducedByTheAspect.CreateDiagnostic(
                        this.TargetDeclaration.GetDiagnosticLocation(),
                        (this.Aspect.AspectClass.DisplayName, interfaceType, this.TargetDeclaration) ) );
            }

            if ( this._introducedAndImplementedInterfaces.ContainsKey( interfaceType ) )
            {
                // Conflict on the introduced interface itself.
                switch ( overrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        // Report the diagnostic and return.
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.InterfaceIsAlreadyImplemented.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.DisplayName, interfaceType, this.TargetDeclaration) ) );

                        return;

                    case OverrideStrategy.Ignore:
                        // Nothing to do.
                        return;

                    default:
                        throw new NotImplementedException();
                }
            }

            var conflictingAncestorInterfaces = interfaceType.AllImplementedInterfaces.Intersect( this._introducedAndImplementedInterfaces.Keys ).ToList();

            if ( conflictingAncestorInterfaces.Count > 0 )
            {
                // Conflict on ancestor of the introduced interface.
                switch ( overrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        foreach ( var conflictingInterface in conflictingAncestorInterfaces )
                        {
                            diagnosticAdder.Report(
                                AdviceDiagnosticDescriptors.InterfaceIsAlreadyImplemented.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.DisplayName, conflictingInterface, this.TargetDeclaration) ) );
                        }

                        return;

                    case OverrideStrategy.Ignore:
                        // Nothing to do.
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            var interfacesToIntroduce = new HashSet<INamedType>(
                new[] { interfaceType }.Concat( interfaceType.AllImplementedInterfaces.Except( conflictingAncestorInterfaces ) ),
                this.TargetDeclaration.Compilation.InvariantComparer );

            if ( explicitMemberSpecification == null )
            {
                // No explicit member specification was given, we have to detect introduced members corresponding to all interface members.
                var compilation = this.TargetDeclaration.Compilation;

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
                                AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, interfaceType, interfaceMethod) ) );
                        }
                        else if (
                            !compilation.InvariantComparer.Equals(
                                interfaceMethod.ReturnParameter.ParameterType,
                                matchingAspectMethod.Method.ReturnParameter.ParameterType )
                            || interfaceMethod.ReturnParameter.RefKind != matchingAspectMethod.Method.ReturnParameter.RefKind )
                        {
                            diagnosticAdder.Report(
                                AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, interfaceType, matchingAspectMethod.Method,
                                     interfaceMethod) ) );
                        }
                        else
                        {
                            memberSpecifications.Add(
                                new MemberSpecification( interfaceMethod, null, matchingAspectMethod.Method, matchingAspectMethod.TemplateInfo ) );
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
                                AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, interfaceType, interfaceProperty) ) );
                        }
                        else if (
                            !compilation.InvariantComparer.Equals( interfaceProperty.Type, matchingAspectProperty.Property.Type )
                            || interfaceProperty.RefKind != matchingAspectProperty.Property.RefKind )
                        {
                            diagnosticAdder.Report(
                                AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, interfaceType, matchingAspectProperty.Property,
                                     interfaceProperty) ) );
                        }
                        else
                        {
                            memberSpecifications.Add(
                                new MemberSpecification(
                                    interfaceProperty,
                                    null,
                                    matchingAspectProperty.Property,
                                    matchingAspectProperty.TemplateInfo ) );
                        }
                    }

                    foreach ( var interfaceEvent in introducedInterface.Events )
                    {
                        var matchingAspectEvent = this._aspectInterfaceEvents.SingleOrDefault( ae => ae.Event.Name == interfaceEvent.Name );

                        if ( matchingAspectEvent.Event == null )
                        {
                            diagnosticAdder.Report(
                                AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, interfaceType, interfaceEvent) ) );
                        }
                        else if ( !compilation.InvariantComparer.Equals( interfaceEvent.EventType, matchingAspectEvent.Event.EventType ) )
                        {
                            diagnosticAdder.Report(
                                AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, interfaceType, matchingAspectEvent.Event,
                                     interfaceEvent) ) );
                        }
                        else
                        {
                            memberSpecifications.Add(
                                new MemberSpecification( interfaceEvent, null, matchingAspectEvent.Event, matchingAspectEvent.TemplateInfo ) );
                        }
                    }

                    this._introducedAndImplementedInterfaces.Add( interfaceType, (true, default) );

                    this._introducedInterfaceTypes.Add( new IntroducedInterfaceSpecification( interfaceType, memberSpecifications, overrideStrategy, tags ) );
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            var result = AdviceResult.Create();

            foreach ( var interfaceSpec in this._introducedInterfaceTypes )
            {
                var explicitImplementationBuilders = new List<MemberBuilder>();
                var overrides = new List<OverriddenMember>();
                var interfaceMemberMap = new Dictionary<IMember, IMember>();

                foreach ( var memberSpec in interfaceSpec.MemberSpecifications )
                {
                    // Collect implemented interface members and add non-observable transformations.
                    MemberBuilder memberBuilder;

                    switch ( memberSpec.InterfaceMember )
                    {
                        case IMethod interfaceMethod:
                            memberBuilder = this.GetImplMethodBuilder( interfaceMethod, memberSpec.IsExplicit );
                            interfaceMemberMap.Add( interfaceMethod, memberBuilder );

                            var implementationMethod = (IMethod) memberSpec.AspectInterfaceMember!;

                            overrides.Add(
                                memberSpec.AspectInterfaceMember != null
                                    ? new OverriddenMethod(
                                        this,
                                        (IMethod) memberBuilder,
                                        Template.Create( implementationMethod, memberSpec.TemplateInfo, TemplateKind.Introduction ) )
                                    : new RedirectedMethod(
                                        this,
                                        (IMethod) memberBuilder,
                                        (IMethod) memberSpec.TargetMember.AssertNotNull() ) );

                            break;

                        case IProperty interfaceProperty:
                            var buildAutoProperty = ((IProperty?) memberSpec.AspectInterfaceMember)?.IsAutoPropertyOrField == true;

                            memberBuilder = this.GetImplPropertyBuilder(
                                interfaceProperty,
                                (IProperty?) memberSpec.TargetMember ?? (IProperty) memberSpec.AspectInterfaceMember.AssertNotNull(),
                                buildAutoProperty,
                                memberSpec.IsExplicit );

                            interfaceMemberMap.Add( interfaceProperty, memberBuilder );

                            if ( ((IProperty?) memberSpec.AspectInterfaceMember)?.IsAutoPropertyOrField != true )
                            {
                                overrides.Add(
                                    memberSpec.AspectInterfaceMember != null
                                        ? new OverriddenProperty(
                                            this,
                                            (IProperty) memberBuilder,
                                            Template.Create( (IProperty) memberSpec.AspectInterfaceMember, memberSpec.TemplateInfo, TemplateKind.Introduction ),
                                            default,
                                            default )
                                        : new RedirectedProperty(
                                            this,
                                            (IProperty) memberBuilder,
                                            (IProperty) memberSpec.TargetMember.AssertNotNull() ) );
                            }

                            break;

                        case IEvent interfaceEvent:
                            var isEventField = memberSpec.AspectInterfaceMember != null
                                               && ((IEvent) memberSpec.AspectInterfaceMember).IsEventField();

                            memberBuilder = this.GetImplEventBuilder( interfaceEvent, isEventField, memberSpec.IsExplicit );
                            interfaceMemberMap.Add( interfaceEvent, memberBuilder );

                            if ( !isEventField )
                            {
                                overrides.Add(
                                    memberSpec.AspectInterfaceMember != null
                                        ? new OverriddenEvent(
                                            this,
                                            (IEvent) memberBuilder,
                                            Template.Create( (IEvent) memberSpec.AspectInterfaceMember, memberSpec.TemplateInfo, TemplateKind.Introduction ),
                                            default,
                                            default )
                                        : new RedirectedEvent(
                                            this,
                                            (IEvent) memberBuilder,
                                            (IEvent) memberSpec.TargetMember.AssertNotNull() ) );
                            }

                            break;

                        default:
                            throw new AssertionFailedException();
                    }

                    explicitImplementationBuilders.Add( memberBuilder );
                }

                result = result.WithTransformations( new IntroducedInterface( this, this.TargetDeclaration, interfaceSpec.InterfaceType, interfaceMemberMap ) );

                result = result.WithTransformations( explicitImplementationBuilders.ToArray<ITransformation>() );
                result = result.WithTransformations( overrides.ToArray<ITransformation>() );
            }

            return result;
        }

        private MemberBuilder GetImplMethodBuilder( IMethod interfaceMethod, bool isExplicit )
        {
            var methodBuilder = new MethodBuilder( this, this.TargetDeclaration, interfaceMethod.Name );

            methodBuilder.ReturnParameter.ParameterType = interfaceMethod.ReturnParameter.ParameterType;
            methodBuilder.ReturnParameter.RefKind = interfaceMethod.ReturnParameter.RefKind;

            foreach ( var interfaceParameter in interfaceMethod.Parameters )
            {
                _ = methodBuilder.AddParameter(
                    interfaceParameter.Name,
                    interfaceParameter.ParameterType,
                    interfaceParameter.RefKind,
                    interfaceParameter.DefaultValue );
            }

            foreach ( var interfaceGenericParameter in interfaceMethod.GenericParameters )
            {
                // TODO: Move this initialization into a second overload of add generic parameter.
                var genericParameterBuilder = methodBuilder.AddGenericParameter( interfaceGenericParameter.Name );
                genericParameterBuilder.IsContravariant = interfaceGenericParameter.IsContravariant;
                genericParameterBuilder.IsCovariant = interfaceGenericParameter.IsCovariant;
                genericParameterBuilder.HasDefaultConstructorConstraint = interfaceGenericParameter.HasDefaultConstructorConstraint;
                genericParameterBuilder.HasNonNullableValueTypeConstraint = interfaceGenericParameter.HasNonNullableValueTypeConstraint;
                genericParameterBuilder.HasReferenceTypeConstraint = interfaceGenericParameter.HasReferenceTypeConstraint;

                foreach ( var templateGenericParameterConstraint in genericParameterBuilder.TypeConstraints )
                {
                    genericParameterBuilder.TypeConstraints.Add( templateGenericParameterConstraint );
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

        private MemberBuilder GetImplPropertyBuilder( IProperty interfaceProperty, IProperty targetProperty, bool isAutoProperty, bool isExplicit )
        {
            var propertyBuilder = new PropertyBuilder(
                this,
                this.TargetDeclaration,
                interfaceProperty.Name,
                interfaceProperty.GetMethod != null || (!isExplicit && targetProperty.GetMethod != null),
                interfaceProperty.SetMethod != null || (!isExplicit && targetProperty.SetMethod != null),
                isAutoProperty,
                interfaceProperty.Writeability == Writeability.InitOnly );

            propertyBuilder.Type = interfaceProperty.Type;

            foreach ( var interfaceParameter in interfaceProperty.Parameters )
            {
                _ = propertyBuilder.AddParameter(
                    interfaceParameter.Name,
                    interfaceParameter.ParameterType,
                    interfaceParameter.RefKind,
                    interfaceParameter.DefaultValue );
            }

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

        private MemberBuilder GetImplEventBuilder( IEvent interfaceEvent, bool isEventField, bool isExplicit )
        {
            var eventBuilder = new EventBuilder(
                this,
                this.TargetDeclaration,
                interfaceEvent.Name,
                isEventField );

            eventBuilder.EventType = interfaceEvent.EventType;

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