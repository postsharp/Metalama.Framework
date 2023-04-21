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
            case OverrideStrategy.Override:
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

        var templateReflectionContext = this.Aspect.AspectClass.GetTemplateReflectionContext( ((CompilationModel) this.SourceCompilation).CompilationContext );

        var aspectType = templateReflectionContext.GetCompilationModel( this.SourceCompilation )
            .Factory.GetTypeByReflectionName( this.Aspect.AspectClass.FullName );

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

            void TryAddMember<T>( T interfaceMember, Func<T, TemplateMember<T>?> getAspectInterfaceMember, Func<T, bool> membersMatch )
                where T : class, IMember
            {
                var memberTemplate = getAspectInterfaceMember( interfaceMember );

                if ( memberTemplate == null )
                {
                    diagnosticAdder.Report(
                        AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMember.CreateRoslynDiagnostic(
                            this.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), InterfaceType: this._interfaceType,
                             interfaceMember) ) );
                }
                else if ( !membersMatch( memberTemplate.Declaration ) )
                {
                    diagnosticAdder.Report(
                        AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateRoslynDiagnostic(
                            this.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ), InterfaceType: this._interfaceType,
                             memberTemplate.Declaration,
                             interfaceMember) ) );
                }
                else
                {
                    var memberSpecification = new MemberSpecification( interfaceMember, null, memberTemplate.Cast<IMember>(), null );

                    var isPublic = memberTemplate.Accessibility == Accessibility.Public;

                    if ( interfaceMember is IPropertyOrIndexer property )
                    {
                        if ( property.GetMethod != null )
                        {
                            isPublic &= memberTemplate.GetAccessorAccessibility == Accessibility.Public;
                        }

                        if ( property.SetMethod != null )
                        {
                            isPublic &= memberTemplate.SetAccessorAccessibility == Accessibility.Public;
                        }
                    }

                    if ( !memberSpecification.IsExplicit && !isPublic )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.ImplicitInterfaceImplementationHasToBePublic.CreateRoslynDiagnostic(
                                this.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this._interfaceType, memberTemplate.Declaration) ) );
                    }
                    else
                    {
                        memberSpecifications.Add( memberSpecification );
                    }
                }
            }

            foreach ( var interfaceMethod in introducedInterface.Methods )
            {
                TryAddMember(
                    interfaceMethod,
                    GetAspectInterfaceMethod,
                    templateMethod =>
                        SignatureTypeSymbolComparer.Instance.Equals(
                            interfaceMethod.ReturnParameter.Type.GetSymbol().AssertNotNull(),
                            templateMethod.ReturnParameter.Type.GetSymbol().AssertNotNull() )
                        && interfaceMethod.ReturnParameter.RefKind == templateMethod.ReturnParameter.RefKind );
            }

            foreach ( var interfaceIndexer in introducedInterface.Indexers )
            {
                _ = interfaceIndexer;

                throw new NotImplementedException( $"Cannot introduce indexer '{interfaceIndexer}' because indexers are not supported." );
            }

            foreach ( var interfaceProperty in introducedInterface.Properties )
            {
                TryAddMember(
                    interfaceProperty,
                    GetAspectInterfaceProperty,
                    templateProperty =>
                        this.SourceCompilation.Comparers.Default.Equals( interfaceProperty.Type, templateProperty.Type )
                        && interfaceProperty.RefKind == templateProperty.RefKind );
            }

            foreach ( var interfaceEvent in introducedInterface.Events )
            {
                TryAddMember(
                    interfaceEvent,
                    GetAspectInterfaceEvent,
                    templateEvent =>
                        this.SourceCompilation.Comparers.Default.Equals( interfaceEvent.Type, templateEvent.Type ) );
            }

            this._interfaceSpecifications.Add( new InterfaceSpecification( introducedInterface, memberSpecifications ) );
        }

        TemplateMember<IMethod>? GetAspectInterfaceMethod( IMethod interfaceMethod )
        {
            var method = aspectType.AllMethods.OfName( interfaceMethod.Name ).SingleOrDefault( m => m.SignatureEquals( interfaceMethod ) );

            if ( method != null && TryGetInterfaceMemberTemplate( method, out var classMember ) )
            {
                return TemplateMemberFactory.Create( method, classMember );
            }

            return null;
        }

        TemplateMember<IProperty>? GetAspectInterfaceProperty( IProperty interfaceProperty )
        {
            var property = aspectType.AllProperties.OfName( interfaceProperty.Name ).SingleOrDefault( p => p.SignatureEquals( interfaceProperty ) );

            if ( property != null && TryGetInterfaceMemberTemplate( property, out var classMember ) )
            {
                return TemplateMemberFactory.Create( property, classMember );
            }

            return null;
        }

        TemplateMember<IEvent>? GetAspectInterfaceEvent( IEvent interfaceEvent )
        {
            var @event = aspectType.AllEvents.OfName( interfaceEvent.Name ).SingleOrDefault( e => e.SignatureEquals( interfaceEvent ) );

            if ( @event != null && TryGetInterfaceMemberTemplate( @event, out var classMember ) )
            {
                return TemplateMemberFactory.Create( @event, classMember );
            }

            return null;
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
            bool skipInterfaceBaseList;

            if ( targetType.AllImplementedInterfaces.Any( t => compilation.Comparers.Default.Equals( t, interfaceSpecification.InterfaceType ) ) )
            {
                switch ( this._overrideStrategy )
                {
                    case OverrideStrategy.Ignore when interfaceSpecification.InterfaceType.Equals( this._interfaceType ):
                        return AdviceImplementationResult.Ignored;

                    case OverrideStrategy.Ignore:
                        continue;

                    case OverrideStrategy.Fail:
                        diagnostics.Report(
                            AdviceDiagnosticDescriptors.InterfaceIsAlreadyImplemented.CreateRoslynDiagnostic(
                                targetType.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, interfaceSpecification.InterfaceType, targetType) ) );

                        continue;

                    case OverrideStrategy.Override:
                        skipInterfaceBaseList = true;

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this._overrideStrategy}." );
                }
            }
            else
            {
                skipInterfaceBaseList = false;
            }

            var interfaceMemberMap = new Dictionary<IMember, IMember>( compilation.Comparers.Default );

            foreach ( var memberSpec in interfaceSpecification.MemberSpecifications )
            {
                // Collect implemented interface members and add non-observable transformations.
                var mergedTags = ObjectReader.Merge( this._tags, memberSpec.Tags );
                var templateAttributeProperties = (memberSpec.Template?.AdviceAttribute as ITemplateAttribute)?.Properties;

                switch ( memberSpec.InterfaceMember )
                {
                    case IMethod interfaceMethod:
                        var existingMethod = targetType.AllMethods.OfName( interfaceMethod.Name ).SingleOrDefault( m => m.SignatureEquals( interfaceMethod ) );
                        var templateMethod = memberSpec.Template?.Cast<IMethod>();
                        var redirectionTargetMethod = (IMethod?) memberSpec.TargetMember;

                        if ( existingMethod != null && !memberSpec.IsExplicit )
                        {
                            switch ( memberSpec.OverrideStrategy )
                            {
                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy != OverrideStrategy.Override:
                                case InterfaceMemberOverrideStrategy.Fail:
                                    diagnostics.Report(
                                        AdviceDiagnosticDescriptors.ImplicitInterfaceMemberAlreadyExists.CreateRoslynDiagnostic(
                                            targetType.GetDiagnosticLocation(),
                                            (this.Aspect.AspectClass.ShortName, interfaceMethod, targetType, existingMethod) ) );

                                    continue;

                                case InterfaceMemberOverrideStrategy.Ignore:
                                    continue;

                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy == OverrideStrategy.Override:

                                    if ( existingMethod.Accessibility != Accessibility.Public )
                                    {
                                        diagnostics.Report(
                                            AdviceDiagnosticDescriptors.CannotOverrideNonPublicInterfaceMethod.CreateRoslynDiagnostic(
                                                targetType.GetDiagnosticLocation(),
                                                (this.Aspect.AspectClass.ShortName, existingMethod) ) );

                                        continue;
                                    }

                                    if ( existingMethod.DeclaringType.Equals( targetType ) )
                                    {
                                        addTransformation(
                                            new OverrideMethodTransformation(
                                                this,
                                                existingMethod,
                                                templateMethod.AssertNotNull().ForOverride( existingMethod ),
                                                mergedTags ) );
                                    }
                                    else
                                    {
                                        if ( !existingMethod.IsVirtual || existingMethod.IsSealed )
                                        {
                                            diagnostics.Report(
                                                AdviceDiagnosticDescriptors.CannotOverrideNonVirtualInterfaceMethod.CreateRoslynDiagnostic(
                                                    targetType.GetDiagnosticLocation(),
                                                    (this.Aspect.AspectClass.ShortName, existingMethod) ) );

                                            continue;
                                        }

                                        IntroduceMethod( false, true );
                                    }

                                    break;

                                case InterfaceMemberOverrideStrategy.MakeExplicit:
                                    IntroduceMethod( true );

                                    break;

                                default:
                                    throw new AssertionFailedException(
                                        $"Unexpected value for InterfaceMemberOverrideStrategy: {memberSpec.OverrideStrategy}." );
                            }
                        }
                        else
                        {
                            IntroduceMethod( memberSpec.IsExplicit );
                        }

                        void IntroduceMethod( bool isExplicit, bool isOverride = false )
                        {
                            var isIteratorMethod = templateMethod?.IsIteratorMethod ?? redirectionTargetMethod.AssertNotNull().IsIteratorMethod() == true;
                            var isVirtual = templateAttributeProperties?.IsVirtual ?? templateMethod is { Declaration.IsVirtual: true };

                            var methodBuilder = this.GetImplMethodBuilder( targetType, interfaceMethod, isIteratorMethod, isExplicit, isVirtual, isOverride);

                        CopyAttributes( interfaceMethod, methodBuilder );

                        addTransformation( methodBuilder.ToTransformation() );
                        interfaceMemberMap.Add( interfaceMethod, methodBuilder );

                        if ( templateMethod != null )
                        {
                            addTransformation(
                                new OverrideMethodTransformation(
                                    this,
                                    methodBuilder,
                                    templateMethod.ForIntroduction( methodBuilder ),
                                    mergedTags ) );
                        }
                        else
                        {
                            addTransformation(
                                new RedirectMethodTransformation(
                                    this,
                                    methodBuilder,
                                    (IMethod) memberSpec.TargetMember.AssertNotNull() ) );
                        }

                            addTransformation( memberBuilder.ToTransformation() );
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
                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy != OverrideStrategy.Override:
                                case InterfaceMemberOverrideStrategy.Fail:
                                    diagnostics.Report(
                                        AdviceDiagnosticDescriptors.ImplicitInterfaceMemberAlreadyExists.CreateRoslynDiagnostic(
                                            targetType.GetDiagnosticLocation(),
                                            (this.Aspect.AspectClass.ShortName, interfaceProperty, targetType, existingProperty) ) );

                                    continue;

                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy == OverrideStrategy.Override:
                                    throw new NotImplementedException( "Overriding interface properties is not yet implemented." );

                                case InterfaceMemberOverrideStrategy.MakeExplicit:
                                    IntroduceProperty( true );

                                    break;

                                case InterfaceMemberOverrideStrategy.Ignore:
                                    continue;

                                default:
                                    throw new NotImplementedException( $"The strategy OverrideStrategy.{this._overrideStrategy} is not implemented." );
                            }
                        }
                        else
                        {
                            IntroduceProperty( memberSpec.IsExplicit );
                        }

                        void IntroduceProperty( bool isExplicit )
                        {
                            var isAutoProperty = templateProperty?.Declaration.IsAutoPropertyOrField == true;

                            var getAccessibility =
                                templateProperty?.GetAccessorAccessibility ?? Accessibility.Public;

                            var setAccessibility =
                                templateProperty?.SetAccessorAccessibility ?? Accessibility.Public;

                            var hasGetter =
                                interfaceProperty.GetMethod != null || (!isExplicit && templateProperty?.Declaration.GetMethod != null);

                            var hasSetter =
                                interfaceProperty.SetMethod != null || (!isExplicit && templateProperty?.Declaration.SetMethod != null);

                            // Check that there are no accessors required by interface that are missing from the template.

                            if ( templateProperty != null )
                            {
                                var getMethodMissingFromTemplate = interfaceProperty.GetMethod != null && templateProperty.Declaration.GetMethod == null;
                                var setMethodMissingFromTemplate = interfaceProperty.SetMethod != null && templateProperty.Declaration.SetMethod == null;
                                var getMethodUnexpectedInTemplate = interfaceProperty.GetMethod == null && templateProperty.Declaration.GetMethod != null;
                                var setMethodUnexpectedInTemplate = interfaceProperty.SetMethod == null && templateProperty.Declaration.SetMethod != null;
                                var setInitOnlyInTemplate = templateProperty.Declaration.Writeability is Writeability.InitOnly;
                                var setInitOnlyInInterface = interfaceProperty.Writeability is Writeability.InitOnly;

                            var missingAccessor =
                                (getMethodMissingFromTemplate, setMethodMissingFromTemplate, setInitOnlyInTemplate, setInitOnlyInInterface) switch
                                {
                                    (true, _, _, _ ) => "get",              // Missing getter.
                                    (false, true, _, false ) => "set",      // Missing setter.
                                    (false, true, _, true ) => "init",      // Missing init-only setter.
                                    (false, false, true, false ) => "set",  // Interface has setter, template has init-only setter.
                                    (false, false, false, true ) => "init", // Interface has init-only setter, template has setter.
                                    _ => null
                                };

                                if ( missingAccessor != null )
                                {
                                    diagnostics.Report(
                                        AdviceDiagnosticDescriptors.InterfacePropertyIsMissingAccessor.CreateRoslynDiagnostic(
                                            targetType.GetDiagnosticLocation(),
                                            (this.Aspect.AspectClass.ShortName, interfaceProperty, targetType, templateProperty.Declaration,
                                             missingAccessor) ) );

                                    return;
                                }

                            var unexpectedAccessor =
                                (isExplicit, getMethodUnexpectedInTemplate, setMethodUnexpectedInTemplate, setInitOnlyInTemplate) switch
                                {
                                    (true, true, _, _ ) => "get",         // Unexpected getter.
                                    (true, false, true, false ) => "set", // Unexpected setter.
                                    (true, false, true, true ) => "init", // Unexpected init-only setter.
                                    _ => null
                                };

                                if ( unexpectedAccessor != null )
                                {
                                    diagnostics.Report(
                                        AdviceDiagnosticDescriptors.ExplicitInterfacePropertyHasSuperficialAccessor.CreateRoslynDiagnostic(
                                            targetType.GetDiagnosticLocation(),
                                            (this.Aspect.AspectClass.ShortName, interfaceProperty, targetType, templateProperty.Declaration,
                                             unexpectedAccessor) ) );

                                    return;
                                }
                            }

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

                            if ( templateProperty != null )
                            {
                                CopyAttributes( templateProperty.Declaration, propertyBuilder );

                                if ( hasGetter )
                                {
                                    CopyAttributes(
                                        templateProperty.Declaration.GetMethod.AssertNotNull(),
                                        (DeclarationBuilder) propertyBuilder.GetMethod.AssertNotNull() );
                                }

                                if ( hasSetter )
                                {
                                    CopyAttributes(
                                        templateProperty.Declaration.SetMethod.AssertNotNull(),
                                        (DeclarationBuilder) propertyBuilder.SetMethod.AssertNotNull() );
                                }
                            }

                        addTransformation( propertyBuilder.ToTransformation() );
                        interfaceMemberMap.Add( interfaceProperty, propertyBuilder );

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

                            addTransformation( propertyBuilder.ToTransformation() );
                        }

                        break;

                    case IIndexer:
                        throw new NotImplementedException( "Implementing interface indexers is not yet supported." );

                    case IEvent interfaceEvent:
                        var existingEvent = targetType.Events.SingleOrDefault( p => p.SignatureEquals( interfaceEvent ) );
                        var templateEvent = memberSpec.Template?.Cast<IEvent>();
                        var redirectionTargetEvent = (IEvent?) memberSpec.TargetMember;

                        if ( existingEvent != null && !memberSpec.IsExplicit )
                        {
                            switch ( memberSpec.OverrideStrategy )
                            {
                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy != OverrideStrategy.Override:

                                case InterfaceMemberOverrideStrategy.Fail:
                                    diagnostics.Report(
                                        AdviceDiagnosticDescriptors.ImplicitInterfaceMemberAlreadyExists.CreateRoslynDiagnostic(
                                            targetType.GetDiagnosticLocation(),
                                            (this.Aspect.AspectClass.ShortName, interfaceEvent, targetType, existingEvent) ) );

                                    continue;

                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy == OverrideStrategy.Override:
                                    throw new NotImplementedException( "Overriding interface events is not yet implemented." );

                                case InterfaceMemberOverrideStrategy.MakeExplicit:
                                    IntroduceEvent( true );

                                    break;

                                case InterfaceMemberOverrideStrategy.Ignore:
                                    continue;

                                default:
                                    throw new NotImplementedException( $"The strategy OverrideStrategy.{this._overrideStrategy} is not implemented." );
                            }
                        }
                        else
                        {
                            IntroduceEvent( memberSpec.IsExplicit );
                        }

                        void IntroduceEvent( bool isExplicit )
                        {
                            var isEventField = templateEvent?.Declaration.IsEventField() ?? false;

                            var eventBuilder = this.GetImplEventBuilder( targetType, interfaceEvent, isEventField, isExplicit, mergedTags );

                            if ( templateEvent != null )
                            {
                                CopyAttributes( templateEvent.Declaration, eventBuilder );
                                CopyAttributes( templateEvent.Declaration.AssertNotNull(), (DeclarationBuilder) eventBuilder.AddMethod.AssertNotNull() );
                                CopyAttributes( templateEvent.Declaration.AssertNotNull(), (DeclarationBuilder) eventBuilder.RemoveMethod.AssertNotNull() );
                            }

                        addTransformation( eventBuilder.ToTransformation() );
                        interfaceMemberMap.Add( interfaceEvent, eventBuilder );

                            if ( templateEvent != null )
                            {
                                if ( !isEventField )
                                {
                                    var accessorTemplates = templateEvent.GetAccessorTemplates();

                                    addTransformation(
                                        new OverrideEventTransformation(
                                            this,
                                            eventBuilder,
                                            accessorTemplates.Add?.ForOverride( eventBuilder.AddMethod ),
                                            accessorTemplates.Remove?.ForOverride( eventBuilder.RemoveMethod ),
                                            mergedTags ) );
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

                            addTransformation( eventBuilder.ToTransformation() );
                        }

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected kind of declaration: '{memberSpec.InterfaceMember}'." );
                }

                void CopyAttributes( IDeclaration interfaceMember, DeclarationBuilder builder )
                {
                    var classificationService = serviceProvider.Global.GetRequiredService<AttributeClassificationService>();

                    foreach ( var codeElementAttribute in interfaceMember.Attributes )
                    {
                        if ( classificationService.MustCopyTemplateAttribute( codeElementAttribute ) )
                        {
                            builder.AddAttribute( codeElementAttribute.ToAttributeConstruction() );
                        }
                    }
                }
            }

            if ( !skipInterfaceBaseList )
            {
                addTransformation( new IntroduceInterfaceTransformation( this, targetType, interfaceSpecification.InterfaceType, interfaceMemberMap ) );
            }
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

    private MethodBuilder GetImplMethodBuilder(
        INamedType declaringType,
        IMethod interfaceMethod,
        bool isIteratorMethod,
        bool isExplicit,
        bool isVirtual,
        bool isOverride )
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

        if ( isOverride )
        {
            methodBuilder.IsOverride = true;
        }
        else if ( isVirtual )
        {
            methodBuilder.IsVirtual = true;
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
            tags )
        { Type = interfaceProperty.Type };

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