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
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.Advising;

internal sealed partial class ImplementInterfaceAdvice : Advice
{
    private readonly List<InterfaceSpecification> _interfaceSpecifications;
    private readonly INamedType _interfaceType;
    private readonly OverrideStrategy _overrideStrategy;
    private readonly IObjectReader _tags;

    private new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

    public ImplementInterfaceAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance template,
        INamedType targetType,
        ICompilation sourceCompilation,
        INamedType interfaceType,
        OverrideStrategy overrideStrategy,
        string? layerName,
        IObjectReader tags ) : base( aspect, template, targetType, sourceCompilation, layerName )
    {
        this._interfaceType = interfaceType;
        this._overrideStrategy = overrideStrategy;
        this._interfaceSpecifications = new List<InterfaceSpecification>();
        this._tags = tags;
    }

    public override AdviceKind AdviceKind => AdviceKind.ImplementInterface;

    public override void Initialize( ProjectServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
    {
        base.Initialize( serviceProvider, diagnosticAdder );

        switch ( this._overrideStrategy )
        {
            case OverrideStrategy.Fail:
            case OverrideStrategy.Ignore:
                break;

            default:
                diagnosticAdder.Report(
                    AdviceDiagnosticDescriptors.InterfaceUnsupportedOverrideStrategy.CreateRoslynDiagnostic(
                        this.GetDiagnosticLocation(),
                        (this.Aspect.AspectClass.ShortName, InterfaceType: this._interfaceType, this.TargetDeclaration.GetTarget( this.SourceCompilation ),
                         this._overrideStrategy) ) );

                break;
        }

        if ( this._interfaceType is { IsGeneric: true, IsCanonicalGenericInstance: true } )
        {
            diagnosticAdder.Report(
                AdviceDiagnosticDescriptors.CannotImplementCanonicalGenericInstanceOfGenericInterface.CreateRoslynDiagnostic(
                    this.GetDiagnosticLocation(),
                    (this.Aspect.AspectClass.ShortName, InterfaceType: this._interfaceType, this.TargetDeclaration.GetTarget( this.SourceCompilation )) ) );

            // No other diagnostics should be reported after this.
            return;
        }

        if ( !this._interfaceType.IsFullyBound() )
        {
            // Temporary limitation.
            throw new NotImplementedException( "Overriding unbound generic interfaces is not yet supported." );
        }

        // When initializing, it is not known which types the target type is implementing.
        // Therefore, a specification for all interfaces should be prepared and only diagnostics related advice parameters and aspect class
        // should be reported.            

        var aspectTypeName = this.Aspect.AspectClass.FullName.AssertNotNull();
        var aspectType = this.SourceCompilation.GetCompilationModel().Factory.GetTypeByReflectionName( aspectTypeName );

        // Prepare all interface types that need to be introduced.
        var interfacesToIntroduce =
            new[] { (InterfaceType: this._interfaceType, IsTopLevel: true) }
                .Concat( this._interfaceType.AllImplementedInterfaces.SelectAsImmutableArray( i => (InterfaceType: i, IsTopLevel: false) ) )
                .ToDictionary( x => x.InterfaceType, x => x.IsTopLevel, this.SourceCompilation.Comparers.Default );

        // No explicit member specification was given, we have to detect introduced members corresponding to all interface members.
        foreach ( var pair in interfacesToIntroduce )
        {
            var introducedInterface = pair.Key;
            List<MemberSpecification> memberSpecifications = new();

            foreach ( var interfaceMethod in introducedInterface.Methods )
            {
                if ( !TryGetAspectInterfaceMethod( interfaceMethod, out var methodTemplate ) )
                {
                    diagnosticAdder.Report(
                        AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateRoslynDiagnostic(
                            this.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), InterfaceType: this._interfaceType,
                             interfaceMethod) ) );
                }
                else if (
                    !SignatureTypeSymbolComparer.Instance.Equals(
                        interfaceMethod.ReturnParameter.Type.GetSymbol().AssertNotNull(),
                        methodTemplate.Declaration.ReturnParameter.Type.GetSymbol().AssertNotNull() )
                    || interfaceMethod.ReturnParameter.RefKind != methodTemplate.Declaration.ReturnParameter.RefKind )
                {
                    diagnosticAdder.Report(
                        AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateRoslynDiagnostic(
                            this.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), InterfaceType: this._interfaceType,
                             methodTemplate.Declaration,
                             interfaceMethod) ) );
                }
                else
                {
                    memberSpecifications.Add( new MemberSpecification( interfaceMethod, null, methodTemplate.Cast<IMember>(), null ) );
                }
            }

            foreach ( var interfaceIndexer in introducedInterface.Indexers )
            {
                _ = interfaceIndexer;

                throw new NotImplementedException();
            }

            foreach ( var interfaceProperty in introducedInterface.Properties )
            {
                if ( !TryGetAspectInterfaceProperty( interfaceProperty, out var propertyTemplate ) )
                {
                    diagnosticAdder.Report(
                        AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateRoslynDiagnostic(
                            this.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), InterfaceType: this._interfaceType,
                             interfaceProperty) ) );
                }
                else if (
                    !this.SourceCompilation.Comparers.Default.Equals( interfaceProperty.Type, propertyTemplate.Declaration.Type )
                    || interfaceProperty.RefKind != propertyTemplate.Declaration.RefKind )
                {
                    diagnosticAdder.Report(
                        AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateRoslynDiagnostic(
                            this.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), InterfaceType: this._interfaceType,
                             propertyTemplate.Declaration,
                             interfaceProperty) ) );
                }
                else
                {
                    memberSpecifications.Add( new MemberSpecification( interfaceProperty, null, propertyTemplate.Cast<IMember>(), null ) );
                }
            }

            foreach ( var interfaceEvent in introducedInterface.Events )
            {
                if ( !TryGetAspectInterfaceEvent( interfaceEvent, out var eventTemplate ) )
                {
                    diagnosticAdder.Report(
                        AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateRoslynDiagnostic(
                            this.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), InterfaceType: this._interfaceType,
                             interfaceEvent) ) );
                }
                else if ( !this.SourceCompilation.Comparers.Default.Equals( interfaceEvent.Type, eventTemplate.Declaration.Type ) )
                {
                    diagnosticAdder.Report(
                        AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateRoslynDiagnostic(
                            this.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), InterfaceType: this._interfaceType,
                             eventTemplate.Declaration,
                             interfaceEvent) ) );
                }
                else
                {
                    memberSpecifications.Add( new MemberSpecification( interfaceEvent, null, eventTemplate.Cast<IMember>(), null ) );
                }
            }

            this._interfaceSpecifications.Add( new InterfaceSpecification( introducedInterface, memberSpecifications ) );
        }

        bool TryGetAspectInterfaceMethod(
            IMethod interfaceMethod,
            [NotNullWhen( true )] out TemplateMember<IMethod>? aspectMethod )
        {
            var method = aspectType.AllMethods.SingleOrDefault( m => m.SignatureEquals( interfaceMethod ) );

            if ( method != null && TryGetInterfaceMemberTemplate( method, out var classMember ) )
            {
                aspectMethod = TemplateMemberFactory.Create( method, classMember );

                return true;
            }

            aspectMethod = null;

            return false;
        }

        bool TryGetAspectInterfaceProperty(
            IProperty interfaceProperty,
            [NotNullWhen( true )] out TemplateMember<IProperty>? aspectProperty )
        {
            var property = aspectType.AllProperties.SingleOrDefault( p => p.SignatureEquals( interfaceProperty ) );

            if ( property != null && TryGetInterfaceMemberTemplate( property, out var classMember ) )
            {
                aspectProperty = TemplateMemberFactory.Create( property, classMember );

                return true;
            }

            aspectProperty = null;

            return false;
        }

        bool TryGetAspectInterfaceEvent(
            IEvent interfaceEvent,
            [NotNullWhen( true )] out TemplateMember<IEvent>? aspectEvent )
        {
            var @event = aspectType.AllEvents.SingleOrDefault( e => e.SignatureEquals( interfaceEvent ) );

            if ( @event != null && TryGetInterfaceMemberTemplate( @event, out var classMember ) )
            {
                aspectEvent = TemplateMemberFactory.Create( @event, classMember );

                return true;
            }

            aspectEvent = null;

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
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        // Adding interfaces may run into three problems:
        //      1) Target type already implements the interface.
        //      2) Target type already implements an ancestor of the interface.

        var targetType = this.TargetDeclaration.GetTarget( compilation ).AssertNotNull();
        var diagnostics = new DiagnosticBag();

        foreach ( var interfaceSpecification in this._interfaceSpecifications )
        {
            // Validate that the interface must be introduced to the specific target.
            if ( targetType.AllImplementedInterfaces.Any( t => compilation.Comparers.Default.Equals( t, interfaceSpecification.InterfaceType ) ) )
            {
                if ( this._overrideStrategy == OverrideStrategy.Fail )
                {
                    diagnostics.Report(
                        AdviceDiagnosticDescriptors.InterfaceIsAlreadyImplemented.CreateRoslynDiagnostic(
                            targetType.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, interfaceSpecification.InterfaceType, targetType) ) );
                }

                continue;
            }

            var interfaceMemberMap = new Dictionary<IMember, IMember>( compilation.Comparers.Default );

            foreach ( var memberSpec in interfaceSpecification.MemberSpecifications )
            {
                // Collect implemented interface members and add non-observable transformations.
                MemberBuilder memberBuilder;

                var mergedTags = ObjectReader.Merge( this._tags, memberSpec.Tags );

                bool isExplicit;

                switch ( memberSpec.InterfaceMember )
                {
                    case IMethod interfaceMethod:
                        var existingMethod = targetType.Methods.SingleOrDefault( m => m.SignatureEquals( interfaceMethod ) );
                        var templateMethod = memberSpec.Template?.Cast<IMethod>();
                        var redirectionTargetMethod = (IMethod?) memberSpec.TargetMember;

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
                                    throw new AssertionFailedException(
                                        $"Unexpected value for InterfaceMemberOverrideStrategy: {memberSpec.OverrideStrategy}." );
                            }
                        }
                        else
                        {
                            isExplicit = memberSpec.IsExplicit;
                        }

                        var isIteratorMethod = templateMethod?.IsIteratorMethod ?? redirectionTargetMethod.AssertNotNull().IsIteratorMethod() == true;

                        memberBuilder = this.GetImplMethodBuilder( targetType, interfaceMethod, isIteratorMethod, isExplicit );
                        interfaceMemberMap.Add( interfaceMethod, memberBuilder );

                        if ( templateMethod != null )
                        {
                            addTransformation(
                                new OverrideMethodTransformation(
                                    this,
                                    (IMethod) memberBuilder,
                                    templateMethod.ForIntroduction(),
                                    mergedTags ) );
                        }
                        else
                        {
                            addTransformation(
                                new RedirectMethodTransformation(
                                    this,
                                    (IMethod) memberBuilder,
                                    (IMethod) memberSpec.TargetMember.AssertNotNull() ) );
                        }

                        break;

                    case IProperty interfaceProperty:
                        var existingProperty = targetType.Properties.SingleOrDefault( p => p.SignatureEquals( interfaceProperty ) );
                        var templateProperty = memberSpec.Template?.Cast<IProperty>();
                        var redirectionTargetProperty = (IProperty?) memberSpec.TargetMember;

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
                                    throw new NotImplementedException( $"The strategy OverrideStrategy.{this._overrideStrategy} is not implemented." );
                            }
                        }
                        else
                        {
                            isExplicit = memberSpec.IsExplicit;
                        }

                        var isAutoProperty = templateProperty?.Declaration.IsAutoPropertyOrField == true;

                        var getAccessibility =
                            templateProperty?.Declaration.GetMethod?.Accessibility ?? Accessibility.Public;

                        var setAccessibility =
                            templateProperty?.Declaration.SetMethod?.Accessibility ?? Accessibility.Public;

                        var hasGetter =
                            interfaceProperty.GetMethod != null || (!isExplicit && templateProperty?.Declaration.GetMethod != null);

                        var hasSetter =
                            interfaceProperty.GetMethod != null || (!isExplicit && templateProperty?.Declaration.GetMethod != null);

                        var hasImplicitSetter = templateProperty?.Declaration.SetMethod?.IsImplicitlyDeclared ?? false;

                        var propertyBuilder = this.GetImplPropertyBuilder(
                            targetType,
                            interfaceProperty,
                            getAccessibility,
                            setAccessibility,
                            hasGetter,
                            hasSetter,
                            isAutoProperty,
                            isExplicit,
                            hasImplicitSetter,
                            mergedTags );

                        memberBuilder = propertyBuilder;
                        interfaceMemberMap.Add( interfaceProperty, memberBuilder );

                        if ( templateProperty != null )
                        {
                            if ( isAutoProperty != true )
                            {
                                var accessorTemplates = templateProperty.GetAccessorTemplates();

                                addTransformation(
                                    new OverridePropertyTransformation(
                                        this,
                                        propertyBuilder,
                                        propertyBuilder.GetMethod != null ? accessorTemplates.Get?.ForOverride( propertyBuilder.GetMethod ) : null,
                                        propertyBuilder.SetMethod != null ? accessorTemplates.Set?.ForOverride( propertyBuilder.SetMethod ) : null,
                                        mergedTags ) );
                            }
                            else
                            {
                                propertyBuilder.InitializerTemplate = templateProperty.GetInitializerTemplate();

                                OverrideHelper.AddTransformationsForStructField( targetType, this, addTransformation );
                            }
                        }
                        else
                        {
                            addTransformation(
                                new RedirectPropertyTransformation(
                                    this,
                                    propertyBuilder,
                                    redirectionTargetProperty.AssertNotNull() ) );
                        }

                        break;

                    case IIndexer:
                        throw new NotImplementedException();

                    case IEvent interfaceEvent:
                        var existingEvent = targetType.Events.SingleOrDefault( p => p.SignatureEquals( interfaceEvent ) );
                        var templateEvent = memberSpec.Template?.Cast<IEvent>();
                        var redirectionTargetEvent = (IEvent?) memberSpec.TargetMember;

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
                                    throw new NotImplementedException( $"The strategy OverrideStrategy.{this._overrideStrategy} is not implemented." );
                            }
                        }
                        else
                        {
                            isExplicit = memberSpec.IsExplicit;
                        }

                        var isEventField = templateEvent?.Declaration.IsEventField() ?? false;

                        var eventBuilder = this.GetImplEventBuilder( targetType, interfaceEvent, isEventField, isExplicit, mergedTags );
                        memberBuilder = eventBuilder;
                        interfaceMemberMap.Add( interfaceEvent, memberBuilder );

                        if ( templateEvent != null )
                        {
                            if ( !isEventField )
                            {
                                addTransformation(
                                    new OverrideEventTransformation(
                                        this,
                                        eventBuilder,
                                        templateEvent,
                                        default,
                                        default,
                                        mergedTags,
                                        null ) );
                            }
                            else
                            {
                                eventBuilder.InitializerTemplate = templateEvent.GetInitializerTemplate();

                                OverrideHelper.AddTransformationsForStructField( targetType, this, addTransformation );
                            }
                        }
                        else
                        {
                            addTransformation(
                                new RedirectEventTransformation(
                                    this,
                                    eventBuilder,
                                    redirectionTargetEvent.AssertNotNull() ) );
                        }

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected kind of declaration: '{memberSpec.InterfaceMember}'." );
                }

                addTransformation( memberBuilder.ToTransformation() );
            }

            addTransformation( new IntroduceInterfaceTransformation( this, targetType, interfaceSpecification.InterfaceType, interfaceMemberMap ) );
        }

        if ( diagnostics.HasError() )
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
        bool isIteratorMethod,
        bool isExplicit )
    {
        var name = GetInterfaceMemberName( interfaceMethod, isExplicit );

        var methodBuilder = new MethodBuilder( this, declaringType, name )
        {
            ReturnParameter = { Type = interfaceMethod.ReturnParameter.Type, RefKind = interfaceMethod.ReturnParameter.RefKind }
        };

        methodBuilder.SetIsIteratorMethod( isIteratorMethod );

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
        Accessibility getAccessibility,
        Accessibility setAccessibility,
        bool hasGetter,
        bool hasSetter,
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
            hasGetter,
            hasSetter,
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
                    propertyBuilder.GetMethod.Accessibility = getAccessibility;
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
                    propertyBuilder.SetMethod.Accessibility = setAccessibility;
                }
            }
        }

        return propertyBuilder;
    }

    private Location? GetDiagnosticLocation() => this.TargetDeclaration.GetTarget( this.SourceCompilation ).GetDiagnosticLocation();

    private EventBuilder GetImplEventBuilder( INamedType declaringType, IEvent interfaceEvent, bool isEventField, bool isExplicit, IObjectReader tags )
    {
        var name = GetInterfaceMemberName( interfaceEvent, isExplicit );

        var eventBuilder = new EventBuilder( this, declaringType, name, isEventField, tags ) { Type = interfaceEvent.Type };

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