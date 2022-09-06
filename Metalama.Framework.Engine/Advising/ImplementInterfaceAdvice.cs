// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.Advising
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
            ICompilation sourceCompilation,
            INamedType interfaceType,
            OverrideStrategy overrideStrategy,
            IReadOnlyList<InterfaceMemberSpecification>? explicitMemberSpecifications,
            string? layerName,
            IObjectReader tags ) : base( aspect, template, targetType, sourceCompilation, layerName )
        {
            this.InterfaceType = interfaceType;
            this.ExplicitMemberSpecifications = explicitMemberSpecifications;
            this.OverrideStrategy = overrideStrategy;
            this._interfaceSpecifications = new List<InterfaceSpecification>();
            this.Tags = tags;
        }

        public override void Initialize( IServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
        {
            switch ( this.OverrideStrategy )
            {
                case OverrideStrategy.Fail:
                case OverrideStrategy.Ignore:
                    break;

                default:
                    diagnosticAdder.Report(
                        AdviceDiagnosticDescriptors.InterfaceUnsupportedOverrideStrategy.CreateRoslynDiagnostic(
                            this.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.InterfaceType, this.TargetDeclaration.GetTarget( this.SourceCompilation ),
                             this.OverrideStrategy) ) );

                    break;
            }

            // When initializing, it is not known which types the target type is implementing.
            // Therefore, a specification for all interfaces should be prepared and only diagnostics related advice parameters and aspect class
            // should be reported.            

            var aspectTypeName = this.Aspect.AspectClass.FullName.AssertNotNull();
            var aspectType = this.SourceCompilation.GetCompilationModel().Factory.GetTypeByReflectionName( aspectTypeName );

            // Prepare all interface types that need to be introduced.
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
                    _ = interfaceIndexer;

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

                this._interfaceSpecifications.Add( new InterfaceSpecification( introducedInterface, memberSpecifications ) );
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

        public override AdviceImplementationResult Implement(
            IServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            // Adding interfaces may run into three problems:
            //      1) Target type already implements the interface.
            //      2) Target type already implements an ancestor of the interface.

            var targetType = this.TargetDeclaration.GetTarget( compilation ).AssertNotNull();
            var diagnostics = new DiagnosticList();

            foreach ( var interfaceSpecification in this._interfaceSpecifications )
            {
                // Validate that the interface must be introduced to the specific target.
                if ( targetType.AllImplementedInterfaces.Any( t => compilation.InvariantComparer.Equals( t, interfaceSpecification.InterfaceType ) ) )
                {
                    if ( this.OverrideStrategy == OverrideStrategy.Fail )
                    {
                        diagnostics.Report(
                            AdviceDiagnosticDescriptors.InterfaceIsAlreadyImplemented.CreateRoslynDiagnostic(
                                targetType.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, interfaceSpecification.InterfaceType, targetType) ) );
                    }

                    continue;
                }

                var interfaceMemberMap = new Dictionary<IMember, IMember>();

                foreach ( var memberSpec in interfaceSpecification.MemberSpecifications )
                {
                    // Collect implemented interface members and add non-observable transformations.
                    MemberBuilder memberBuilder;

                    var mergedTags = ObjectReader.Merge( this.Tags, memberSpec.Tags );

                    bool isExplicit;

                    switch ( memberSpec.InterfaceMember )
                    {
                        case IMethod interfaceMethod:
                            var existingMethod = targetType.Methods.SingleOrDefault( m => m.SignatureEquals( interfaceMethod ) );

                            if ( existingMethod != null && !memberSpec.IsExplicit )
                            {
                                switch ( memberSpec.OverrideStrategy )
                                {
                                    case InterfaceMemberOverrideStrategy.Fail:
                                        diagnostics.Report(
                                            AdviceDiagnosticDescriptors.ImplicitInterfaceMemberAlreadyExists.CreateRoslynDiagnostic(
                                                targetType.GetDiagnosticLocation(),
                                                (this.Aspect.AspectClass.ShortName, interfaceMethod, targetType, existingMethod) ) );

                                        continue;

                                    case InterfaceMemberOverrideStrategy.MakeExplicit:
                                        isExplicit = true;

                                        break;

                                    default:
                                        throw new AssertionFailedException();
                                }
                            }
                            else
                            {
                                isExplicit = memberSpec.IsExplicit;
                            }

                            var aspectMethod = (IMethod) memberSpec.AspectInterfaceMember!;
                            memberBuilder = this.GetImplMethodBuilder( targetType, interfaceMethod, isExplicit );
                            interfaceMemberMap.Add( interfaceMethod, memberBuilder );

                            addTransformation(
                                memberSpec.AspectInterfaceMember != null
                                    ? new OverrideMethodTransformation(
                                        this,
                                        (IMethod) memberBuilder,
                                        TemplateMemberFactory.Create(
                                                aspectMethod,
                                                memberSpec.TemplateClassMember,
                                                (ITemplateAttribute) memberSpec.TemplateClassMember.TemplateInfo.Attribute.AssertNotNull(),
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
                                switch ( memberSpec.OverrideStrategy )
                                {
                                    case InterfaceMemberOverrideStrategy.Fail:
                                        diagnostics.Report(
                                            AdviceDiagnosticDescriptors.ImplicitInterfaceMemberAlreadyExists.CreateRoslynDiagnostic(
                                                targetType.GetDiagnosticLocation(),
                                                (this.Aspect.AspectClass.ShortName, interfaceProperty, targetType, existingProperty) ) );

                                        continue;

                                    case InterfaceMemberOverrideStrategy.MakeExplicit:
                                        isExplicit = true;

                                        break;

                                    default:
                                        throw new NotImplementedException( $"The strategy OverrideStrategy.{this.OverrideStrategy} is not implemented." );
                                }
                            }
                            else
                            {
                                isExplicit = memberSpec.IsExplicit;
                            }

                            var aspectProperty = (IProperty?) memberSpec.AspectInterfaceMember;
                            var buildAutoProperty = aspectProperty?.IsAutoPropertyOrField == true;

                            var propertyBuilder = this.GetImplPropertyBuilder(
                                targetType,
                                interfaceProperty,
                                (IProperty?) memberSpec.TargetMember ?? (IProperty) memberSpec.AspectInterfaceMember.AssertNotNull(),
                                buildAutoProperty,
                                isExplicit,
                                aspectProperty?.SetMethod?.IsImplicitlyDeclared ?? false,
                                mergedTags );

                            memberBuilder = propertyBuilder;

                            interfaceMemberMap.Add( interfaceProperty, memberBuilder );

                            if ( aspectProperty != null && aspectProperty.IsAutoPropertyOrField != true )
                            {
                                var propertyTemplate = TemplateMemberFactory.Create(
                                    aspectProperty,
                                    memberSpec.TemplateClassMember,
                                    TemplateKind.Introduction );

                                var accessorTemplates = propertyTemplate.GetAccessorTemplates();

                                addTransformation(
                                    memberSpec.AspectInterfaceMember != null
                                        ? new OverridePropertyTransformation(
                                            this,
                                            (IProperty) memberBuilder,
                                            propertyBuilder.GetMethod != null ? accessorTemplates.Get?.ForOverride( propertyBuilder.GetMethod ) : null,
                                            propertyBuilder.SetMethod != null ? accessorTemplates.Set?.ForOverride( propertyBuilder.SetMethod ) : null,
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
                                switch ( memberSpec.OverrideStrategy )
                                {
                                    case InterfaceMemberOverrideStrategy.Fail:
                                        diagnostics.Report(
                                            AdviceDiagnosticDescriptors.ImplicitInterfaceMemberAlreadyExists.CreateRoslynDiagnostic(
                                                targetType.GetDiagnosticLocation(),
                                                (this.Aspect.AspectClass.ShortName, interfaceEvent, targetType, existingEvent) ) );

                                        continue;

                                    case InterfaceMemberOverrideStrategy.MakeExplicit:
                                        isExplicit = true;

                                        break;

                                    default:
                                        throw new NotImplementedException( $"The strategy OverrideStrategy.{this.OverrideStrategy} is not implemented." );
                                }
                            }
                            else
                            {
                                isExplicit = memberSpec.IsExplicit;
                            }

                            var aspectEvent = memberSpec.AspectInterfaceMember;
                            var isEventField = aspectEvent != null && ((IEvent) aspectEvent).IsEventField();

                            memberBuilder = this.GetImplEventBuilder( targetType, interfaceEvent, isEventField, isExplicit, mergedTags );
                            interfaceMemberMap.Add( interfaceEvent, memberBuilder );

                            if ( !isEventField )
                            {
                                addTransformation(
                                    memberSpec.AspectInterfaceMember != null
                                        ? new OverrideEventTransformation(
                                            this,
                                            (IEvent) memberBuilder,
                                            TemplateMemberFactory.Create(
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

                    addTransformation( memberBuilder );
                }

                addTransformation( new IntroduceInterfaceTransformation( this, targetType, interfaceSpecification.InterfaceType, interfaceMemberMap ) );
            }

            if ( diagnostics.HasErrors() )
            {
                return AdviceImplementationResult.Failed( diagnostics.ToImmutableArray() );
            }

            return AdviceImplementationResult.Success(
                AdviceOutcome.Default,
                this.TargetDeclaration.As<IDeclaration>(),
                diagnostics.Count > 0 ? diagnostics.ToImmutableArray() : null );
        }

        private MemberBuilder GetImplMethodBuilder(
            INamedType declaringType,
            IMethod interfaceMethod,
            bool isExplicit )
        {
            var name = GetInterfaceMemberName( interfaceMethod, isExplicit );

            var methodBuilder = new MethodBuilder( this, declaringType, name )
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
            var name = GetInterfaceMemberName( interfaceProperty, isExplicit );

            var propertyBuilder = new PropertyBuilder(
                this,
                declaringType,
                name,
                interfaceProperty.GetMethod != null || (!isExplicit && targetProperty.GetMethod != null),
                interfaceProperty.SetMethod != null || (!isExplicit && targetProperty.SetMethod != null),
                isAutoProperty,
                interfaceProperty.Writeability == Writeability.InitOnly,
                false,
                hasImplicitSetter,
                tags ) { Type = interfaceProperty.Type };

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

        private Location? GetDiagnosticLocation()
        {
            return this.TargetDeclaration.GetTarget( this.SourceCompilation ).GetDiagnosticLocation();
        }

        private MemberBuilder GetImplEventBuilder( INamedType declaringType, IEvent interfaceEvent, bool isEventField, bool isExplicit, IObjectReader tags )
        {
            var name = GetInterfaceMemberName( interfaceEvent, isExplicit );

            var eventBuilder = new EventBuilder(
                this,
                declaringType,
                name,
                isEventField,
                tags ) { Type = interfaceEvent.Type };

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

        private static string GetInterfaceMemberName( IMember interfaceMember, bool isExplicit )
            => isExplicit ? $"{interfaceMember.DeclaringType.FullName}.{interfaceMember.Name}" : interfaceMember.Name;
    }
}