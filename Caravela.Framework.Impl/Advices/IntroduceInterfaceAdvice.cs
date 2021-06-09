// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroduceInterfaceAdvice : Advice
    {
        private readonly List<(IMethod Method, InterfaceMemberAttribute Attribute)> _aspectInterfaceMethods;
        private readonly List<(IProperty Property, InterfaceMemberAttribute Attribute)> _aspectInterfaceProperties;
        private readonly List<(IEvent Event, InterfaceMemberAttribute Attribute)> _aspectInterfaceEvents;
        private readonly Dictionary<INamedType, (bool IsIntroduced, bool Dummy)> _introducedAndImplementedInterfaces;
        private readonly List<IntroducedInterfaceSpecification> _introducedInterfaceTypes;

        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public IntroduceInterfaceAdvice(
            AspectInstance aspect,
            INamedType targetType,
            string? layerName ) : base( aspect, targetType, layerName, null )
        {
            this._aspectInterfaceMethods = new();
            this._aspectInterfaceProperties = new();
            this._aspectInterfaceEvents = new();
            this._introducedInterfaceTypes = new();

            // Initialize with interface the target type already implements.
            this._introducedAndImplementedInterfaces = targetType.AllImplementedInterfaces.ToDictionary( x => x, x => (false, false), targetType.Compilation.InvariantComparer );
        }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            var aspectTypeName = this.Aspect.AspectClass.AspectType.FullName;
            var compilation = this.TargetDeclaration.Compilation;
            var aspectType = compilation.TypeFactory.GetTypeByReflectionName( aspectTypeName );

            foreach (var aspectMethod in aspectType.Methods)
            {
                var interfaceMemberAttribute = GetInterfaceMemberAttribute( compilation, aspectMethod );

                if ( interfaceMemberAttribute != null )
                {
                    this._aspectInterfaceMethods.Add( (aspectMethod, interfaceMemberAttribute) );
                }
            }

            foreach ( var aspectProperty in aspectType.Properties )
            {
                var interfaceMemberAttribute = GetInterfaceMemberAttribute( compilation, aspectProperty );

                if ( interfaceMemberAttribute != null )
                {
                    this._aspectInterfaceProperties.Add( (aspectProperty, interfaceMemberAttribute) );
                }
            }

            foreach ( var aspectEvent in aspectType.Events )
            {
                var interfaceMemberAttribute = GetInterfaceMemberAttribute( compilation, aspectEvent );

                if ( interfaceMemberAttribute != null )
                {
                    this._aspectInterfaceEvents.Add( (aspectEvent, interfaceMemberAttribute) );
                }
            }

            InterfaceMemberAttribute? GetInterfaceMemberAttribute(ICompilation compilation, IMember member)
            {
                var interfaceMemberAttributeType = compilation.TypeFactory.GetTypeByReflectionType( typeof( InterfaceMemberAttribute ) );
                var interfaceMemberAttribute = member.Attributes.SingleOrDefault( a => compilation.InvariantComparer.Equals( interfaceMemberAttributeType, a.Constructor.DeclaringType ) );

                if ( interfaceMemberAttribute != null )
                {
                    var isExplicitValue = interfaceMemberAttribute.NamedArguments.Where( aa => aa.Key == nameof( InterfaceMemberAttribute.IsExplicit ) ).Select( aa => aa.Value ).LastOrDefault();
                    var isExplicit = isExplicitValue.IsAssigned ? (bool)isExplicitValue.Value : false;
                    return new InterfaceMemberAttribute() { IsExplicit = isExplicit };
                }
                else
                {
                    return null;
                }
            }
        }

        public void AddInterfaceImplementation( INamedType interfaceType, ConflictBehavior conflictBehavior, IReadOnlyList<InterfaceMemberSpecification>? explicitMemberSpecification, IDiagnosticAdder diagnosticAdder, AdviceOptions? options )
        {
            // Adding interfaces may run into three problems:
            //      1) Target type already implements the interface.
            //      2) Target type already implements an ancestor of the interface.
            //      3) The interface or it's ancestor was implemented by another IntroduceInterface call.

            if (this._introducedAndImplementedInterfaces.TryGetValue(interfaceType, out var impl) && impl.IsIntroduced == true)
            {
                // The aspect conflicts with itself, introducing the base interface after the derived interface.
                diagnosticAdder.Report(
                    AdviceDiagnosticDescriptors.InterfaceIsAlreadyIntroducedByTheAspect.CreateDiagnostic(
                        this.TargetDeclaration.GetDiagnosticLocation(),
                        (this.Aspect.AspectClass.DisplayName, interfaceType, this.TargetDeclaration) ) );
            }

            if (this._introducedAndImplementedInterfaces.ContainsKey(interfaceType))
            {
                // Conflict on the introduced interface itself.
                switch ( conflictBehavior )
                {
                    case ConflictBehavior.Fail:
                        // Report the diagnostic and return.
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.InterfaceIsAlreadyImplemented.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.DisplayName, interfaceType, this.TargetDeclaration) ) );
                        return;
                    case ConflictBehavior.Ignore:
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
                switch ( conflictBehavior )
                {
                    case ConflictBehavior.Fail:
                        foreach (var conflictingInterface in conflictingAncestorInterfaces)
                        {
                            diagnosticAdder.Report(
                                AdviceDiagnosticDescriptors.InterfaceIsAlreadyImplemented.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.DisplayName, interfaceType, this.TargetDeclaration) ) );
                        }

                        return;
                    case ConflictBehavior.Ignore:
                        // Nothing to do.
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            var interfacesToIntroduce = new HashSet<INamedType>(
                new[] { interfaceType }.Concat(interfaceType.AllImplementedInterfaces.Except( conflictingAncestorInterfaces )), 
                this.TargetDeclaration.Compilation.InvariantComparer);

            if ( explicitMemberSpecification == null )
            {
                // No explicit member specification was given, we have to detect introduced members corresponding to all interface members.
                var compilation = this.TargetDeclaration.Compilation;

                foreach ( var introducedInterface in interfacesToIntroduce )
                {
                    List<MemberSpecification> memberSpecifications = new();

                    foreach ( var interfaceMethod in introducedInterface.Methods )
                    {
                        var matchingAspectMethod = this._aspectInterfaceMethods
                            .SingleOrDefault(
                                am =>
                                    am.Method.Name == interfaceMethod.Name
                                    && am.Method.GenericParameters.Count == interfaceMethod.GenericParameters.Count
                                    && am.Method.Parameters.Count == interfaceMethod.Parameters.Count
                                    && am.Method.Parameters
                                        .Select( ( p, i ) => (p, i) )
                                        .All(
                                            amp =>
                                                compilation.InvariantComparer.Equals( amp.p.ParameterType, interfaceMethod.Parameters[amp.i].ParameterType )
                                                && amp.p.RefKind == interfaceMethod.Parameters[amp.i].RefKind ) );

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
                            memberSpecifications.Add( new MemberSpecification( interfaceMethod, null, matchingAspectMethod.Method, matchingAspectMethod.Attribute.IsExplicit ) );
                        }
                    }

                    foreach ( var interfaceProperty in introducedInterface.Properties )
                    {
                        var matchingAspectProperty = this._aspectInterfaceProperties
                            .SingleOrDefault(
                                ap =>
                                    ap.Property.Name == interfaceProperty.Name
                                    && ap.Property.Parameters.Count == interfaceProperty.Parameters.Count
                                    && ap.Property.Parameters
                                        .Select( ( p, i ) => (p, i) )
                                        .All(
                                            app =>
                                                compilation.InvariantComparer.Equals( app.p.ParameterType, interfaceProperty.Parameters[app.i].ParameterType )
                                                && app.p.RefKind == interfaceProperty.Parameters[app.i].RefKind ) );

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
                            memberSpecifications.Add( new MemberSpecification( interfaceProperty, null, matchingAspectProperty.Property, matchingAspectProperty.Attribute.IsExplicit ) );
                        }
                    }

                    foreach ( var interfaceEvent in introducedInterface.Events )
                    {
                        var matchingAspectEvent = this._aspectInterfaceEvents
                            .SingleOrDefault( ae => ae.Event.Name == interfaceEvent.Name );

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
                            memberSpecifications.Add( new MemberSpecification( interfaceEvent, null, matchingAspectEvent.Event, matchingAspectEvent.Attribute.IsExplicit ) );
                        }
                    }

                    this._introducedAndImplementedInterfaces.Add( interfaceType, (true, default) );
                    this._introducedInterfaceTypes.Add( new IntroducedInterfaceSpecification( interfaceType, memberSpecifications, conflictBehavior, options ) );
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

            foreach (var interfaceSpec in this._introducedInterfaceTypes)
            {
                var explicitImplementationBuilders = new List<MemberBuilder>();
                var overrides = new List<OverriddenMember>();
                var interfaceMemberMap = new Dictionary<IMember, IMember>();
                var interfaceTargetMap = new Dictionary<IMember, (bool IsAspectInterfaceMember, IMember TargetMember, IMember ImplementationMember)>();

                foreach ( var memberSpec in interfaceSpec.MemberSpecifications )
                {
                    // Collect implemented interface members and add non-observable transformations.
                    MemberBuilder memberBuilder;
                    switch ( memberSpec.InterfaceMember )
                    {
                        case IMethod interfaceMethod:
                            memberBuilder = this.GetImplMethodBuilder( interfaceMethod, memberSpec.IsExplicit );
                            interfaceMemberMap.Add( interfaceMethod, memberBuilder );

                            if ( memberSpec.IsExplicit )
                            {
                                interfaceTargetMap.Add(
                                    interfaceMethod,
                                    (memberSpec.AspectInterfaceTargetMember != null, memberSpec.AspectInterfaceTargetMember ?? memberSpec.TargetMember.AssertNotNull(), memberBuilder) );
                            }
                            else
                            {
                                overrides.Add(
                                    memberSpec.AspectInterfaceTargetMember != null
                                    ? new OverriddenMethod( this, (IMethod) memberBuilder, (IMethod) memberSpec.AspectInterfaceTargetMember, this.LinkerOptions )
                                    : new RedirectedMethod( this, (IMethod) memberBuilder, (IMethod) memberSpec.TargetMember.AssertNotNull(), this.LinkerOptions ) );
                            }

                            break;

                        case IProperty interfaceProperty:
                            memberBuilder = this.GetImplPropertyBuilder( interfaceProperty, memberSpec.IsExplicit );
                            interfaceMemberMap.Add( interfaceProperty, memberBuilder );

                            if ( memberSpec.IsExplicit )
                            {
                                interfaceTargetMap.Add(
                                    interfaceProperty,
                                    (memberSpec.AspectInterfaceTargetMember != null, memberSpec.AspectInterfaceTargetMember ?? memberSpec.TargetMember.AssertNotNull(), memberBuilder) );
                            }
                            else
                            {
                                overrides.Add(
                                    memberSpec.AspectInterfaceTargetMember != null
                                    ? new OverriddenProperty( this, (IProperty) memberBuilder, (IProperty) memberSpec.AspectInterfaceTargetMember, null, null, this.LinkerOptions )
                                    : new RedirectedProperty( this, (IProperty) memberBuilder, (IProperty) memberSpec.TargetMember.AssertNotNull(), this.LinkerOptions ) );
                            }

                            break;

                        case IEvent interfaceEvent:
                            memberBuilder = this.GetImplEventBuilder( interfaceEvent, memberSpec.IsExplicit );
                            interfaceMemberMap.Add( interfaceEvent, memberBuilder );

                            if ( memberSpec.IsExplicit )
                            {
                                interfaceTargetMap.Add(
                                    interfaceEvent,
                                    (memberSpec.AspectInterfaceTargetMember != null, memberSpec.AspectInterfaceTargetMember ?? memberSpec.TargetMember.AssertNotNull(), memberBuilder) );
                            }
                            else
                            {
                                overrides.Add(
                                    memberSpec.AspectInterfaceTargetMember != null
                                    ? new OverriddenEvent( this, (IEvent) memberBuilder, (IEvent) memberSpec.AspectInterfaceTargetMember, null, null, this.LinkerOptions )
                                    : new RedirectedEvent( this, (IEvent) memberBuilder, (IEvent) memberSpec.TargetMember.AssertNotNull(), this.LinkerOptions ) );
                            }

                            break;

                        default:
                            throw new AssertionFailedException();
                    }

                    explicitImplementationBuilders.Add( memberBuilder );
                }

                result = result.WithTransformations(
                    new IntroducedInterface( this, this.TargetDeclaration, interfaceSpec.InterfaceType, interfaceMemberMap, this.LinkerOptions ),
                    new IntroducedInterfaceImplementation( this, this.TargetDeclaration, interfaceSpec.InterfaceType, interfaceTargetMap, this.LinkerOptions ) );

                result = result.WithTransformations( explicitImplementationBuilders.Cast<ITransformation>() );
                result = result.WithTransformations( overrides.Cast<ITransformation>() );
            }

            return result;
        }

        private MemberBuilder GetImplMethodBuilder( IMethod interfaceMethod, bool isExplicit )
        {
            var methodBuilder = new MethodBuilder( this, this.TargetDeclaration, interfaceMethod.Name, this.LinkerOptions );

            methodBuilder.ReturnParameter.ParameterType = interfaceMethod.ReturnParameter.ParameterType;
            methodBuilder.ReturnParameter.RefKind = interfaceMethod.ReturnParameter.RefKind;

            foreach ( var interfaceParameter in interfaceMethod.Parameters )
            {
                _ = methodBuilder.AddParameter( interfaceParameter.Name, interfaceParameter.ParameterType, interfaceParameter.RefKind, interfaceParameter.DefaultValue );
            }

            foreach ( var interfaceGenericParameter in interfaceMethod.GenericParameters )
            {
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

            return methodBuilder;
        }

        private MemberBuilder GetImplPropertyBuilder( IProperty interfaceProperty, bool isExplicit )
        {
            var builder = new PropertyBuilder( 
                this, 
                this.TargetDeclaration, 
                interfaceProperty.Name,
                interfaceProperty.Getter != null,
                interfaceProperty.Setter != null,
                interfaceProperty.IsAutoPropertyOrField,
                interfaceProperty.Writeability == Writeability.InitOnly,
                this.LinkerOptions );

            builder.Type = interfaceProperty.Type;

            foreach ( var interfaceParameter in interfaceProperty.Parameters )
            {
                _ = builder.AddParameter( interfaceParameter.Name, interfaceParameter.ParameterType, interfaceParameter.RefKind, interfaceParameter.DefaultValue );
            }

            if ( isExplicit )
            {
                builder.SetExplicitInterfaceImplementation( interfaceProperty );
            }

            return builder;
        }

        private MemberBuilder GetImplEventBuilder( IEvent interfaceEvent, bool isExplicit )
        {
            var builder = new EventBuilder(
                this,
                this.TargetDeclaration,
                interfaceEvent.Name,
                interfaceEvent.Adder == null && interfaceEvent.Remover == null,
                this.LinkerOptions );

            builder.EventType = interfaceEvent.EventType;

            if ( isExplicit )
            {
                builder.SetExplicitInterfaceImplementation( interfaceEvent );
            }

            return builder;
        }

        private class IntroducedInterfaceSpecification
        {
            public INamedType InterfaceType { get; }

            public IReadOnlyList<MemberSpecification> MemberSpecifications { get; }

            public ConflictBehavior ConflictBehavior { get; }

            public AdviceOptions? Options { get; }

            public IntroducedInterfaceSpecification( INamedType interfaceType, IReadOnlyList<MemberSpecification> memberSpecification, ConflictBehavior conflictBehavior, AdviceOptions? options )
            {
                this.InterfaceType = interfaceType;
                this.MemberSpecifications = memberSpecification;
                this.ConflictBehavior = conflictBehavior;
                this.Options = options;
            }
        }

        private struct MemberSpecification
        {
            public IMember InterfaceMember { get; }

            public IMember? TargetMember { get; }

            public IMember? AspectInterfaceTargetMember { get; }

            public bool IsExplicit { get; }

            public MemberSpecification(IMember interfaceMember, IMember? targetMember, IMember? aspectInterfaceMember, bool isExplicit )
            {
                this.InterfaceMember = interfaceMember;
                this.TargetMember = targetMember;
                this.AspectInterfaceTargetMember = aspectInterfaceMember;
                this.IsExplicit = isExplicit;
            }
        }
    }
}