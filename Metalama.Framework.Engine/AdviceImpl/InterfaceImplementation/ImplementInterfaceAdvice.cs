// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Advising;
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
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.AdviceImpl.InterfaceImplementation;

internal sealed partial class ImplementInterfaceAdvice : Advice<ImplementInterfaceAdviceResult>
{
    private readonly List<InterfaceSpecification> _interfaceSpecifications;
    private readonly INamedType _interfaceType;
    private readonly OverrideStrategy _overrideStrategy;
    private readonly IObjectReader _tags;
    private readonly IAdviceFactoryImpl _adviceFactory;

    private new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

    public ImplementInterfaceAdvice(
        AdviceConstructorParameters<INamedType> parameters,
        INamedType interfaceType,
        OverrideStrategy overrideStrategy,
        IObjectReader tags,
        IAdviceFactoryImpl adviceFactory )
        : base( parameters )
    {
        this._interfaceType = interfaceType;
        this._overrideStrategy = overrideStrategy;
        this._interfaceSpecifications = new List<InterfaceSpecification>();
        this._tags = tags;
        this._adviceFactory = adviceFactory;
    }

    public override AdviceKind AdviceKind => AdviceKind.ImplementInterface;

    protected override void Initialize( in ProjectServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
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
                        (this.AspectInstance.AspectClass.ShortName, InterfaceType: this._interfaceType,
                         this.TargetDeclaration.GetTarget( this.SourceCompilation ),
                         this._overrideStrategy),
                        this ) );

                break;
        }

        if ( this._interfaceType is { IsGeneric: true, IsCanonicalGenericInstance: true } )
        {
            diagnosticAdder.Report(
                AdviceDiagnosticDescriptors.CannotImplementCanonicalGenericInstanceOfGenericInterface.CreateRoslynDiagnostic(
                    this.GetDiagnosticLocation(),
                    (this.AspectInstance.AspectClass.ShortName, InterfaceType: this._interfaceType, this.TargetDeclaration.GetTarget( this.SourceCompilation )),
                    this ) );

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

        var templateReflectionContext =
            this.TemplateInstance.TemplateClass.GetTemplateReflectionContext( ((CompilationModel) this.SourceCompilation).CompilationContext );

        var templateClassType = templateReflectionContext.GetCompilationModel( this.SourceCompilation )
            .Factory.GetTypeByReflectionName( this.TemplateInstance.TemplateClass.FullName );

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
                    // Do nothing. Interface members can (and should) be specified using [Introduce] or [ExplicitInterfaceMember] now.
                }
                else if ( !membersMatch( memberTemplate.Declaration ) )
                {
                    diagnosticAdder.Report(
                        AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberDoesNotMatch.CreateRoslynDiagnostic(
                            this.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, this.TargetDeclaration.GetTarget( this.SourceCompilation ),
                             InterfaceType: this._interfaceType,
                             memberTemplate.Declaration,
                             interfaceMember),
                            this ) );
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
                                (this.AspectInstance.AspectClass.ShortName, this._interfaceType, memberTemplate.Declaration),
                                this ) );
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
                            interfaceMethod.ReturnParameter.Type.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedTypeComparison ),
                            templateMethod.ReturnParameter.Type.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedTypeComparison ) )
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
            var method = templateClassType.AllMethods.OfName( interfaceMethod.Name ).SingleOrDefault( m => m.SignatureEquals( interfaceMethod ) );

            if ( method != null && TryGetInterfaceMemberTemplate( method, out var classMember ) )
            {
                return TemplateMemberFactory.Create( method, classMember );
            }

            return null;
        }

        TemplateMember<IProperty>? GetAspectInterfaceProperty( IProperty interfaceProperty )
        {
            var property = templateClassType.AllProperties.OfName( interfaceProperty.Name ).SingleOrDefault( p => p.SignatureEquals( interfaceProperty ) );

            if ( property != null && TryGetInterfaceMemberTemplate( property, out var classMember ) )
            {
                return TemplateMemberFactory.Create( property, classMember );
            }

            return null;
        }

        TemplateMember<IEvent>? GetAspectInterfaceEvent( IEvent interfaceEvent )
        {
            var @event = templateClassType.AllEvents.OfName( interfaceEvent.Name ).SingleOrDefault( e => e.SignatureEquals( interfaceEvent ) );

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
                member.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedInterfaceImplementation ),
                out templateClassMember );
        }
    }

    protected override ImplementInterfaceAdviceResult Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        // Adding interfaces may run into three problems:
        //      1) Target type already implements the interface.
        //      2) Target type already implements an ancestor of the interface.

        var targetType = this.TargetDeclaration.GetTarget( compilation ).AssertNotNull();
        var diagnostics = new DiagnosticBag();
        var implementedInterfaces = new List<ImplementationResult>();
        var implementedInterfaceMembers = new List<MemberImplementationResult>();

        foreach ( var interfaceSpecification in this._interfaceSpecifications )
        {
            // Validate that the interface must be introduced to the specific target.
            bool skipInterfaceBaseList;
            var forceIgnore = false;

            if ( targetType.AllImplementedInterfaces.Any( t => compilation.Comparers.Default.Equals( t, interfaceSpecification.InterfaceType ) ) )
            {
                switch ( this._overrideStrategy )
                {
                    case OverrideStrategy.Ignore:
                        implementedInterfaces.Add( new ImplementationResult( interfaceSpecification.InterfaceType, InterfaceImplementationOutcome.Ignore ) );
                        skipInterfaceBaseList = true;

                        // The interface is implemented, so we ignore its members, as if they had InterfaceMemberOverrideStrategy.Ignore.
                        forceIgnore = true;

                        break;

                    case OverrideStrategy.Fail:
                        diagnostics.Report(
                            AdviceDiagnosticDescriptors.InterfaceIsAlreadyImplemented.CreateRoslynDiagnostic(
                                targetType.GetDiagnosticLocation(),
                                (this.AspectInstance.AspectClass.ShortName, interfaceSpecification.InterfaceType, targetType),
                                this ) );

                        continue;

                    case OverrideStrategy.Override:
                        implementedInterfaces.Add(
                            new ImplementationResult(
                                interfaceSpecification.InterfaceType,
                                InterfaceImplementationOutcome.Implement,
                                this.TargetDeclaration,
                                this._adviceFactory ) );

                        skipInterfaceBaseList = true;

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this._overrideStrategy}." );
                }
            }
            else
            {
                implementedInterfaces.Add(
                    new ImplementationResult(
                        interfaceSpecification.InterfaceType,
                        InterfaceImplementationOutcome.Implement,
                        this.TargetDeclaration,
                        this._adviceFactory ) );

                skipInterfaceBaseList = false;
            }

            var replaceDefaultConstructorAdded = false;

            void AddTransformationNoDuplicates( ITransformation transformation )
            {
                if ( transformation is IntroduceConstructorTransformation )
                {
                    if ( !replaceDefaultConstructorAdded )
                    {
                        replaceDefaultConstructorAdded = true;
                    }
                    else
                    {
                        return;
                    }
                }

                addTransformation( transformation );
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
#pragma warning disable CS0618 // Type is obsolete
                            switch ( memberSpec.OverrideStrategy )
                            {
                                case { } when forceIgnore:
                                case InterfaceMemberOverrideStrategy.Ignore:

                                    implementedInterfaceMembers.Add(
                                        new MemberImplementationResult(
                                            compilation,
                                            memberSpec.InterfaceMember,
                                            InterfaceMemberImplementationOutcome.UseExisting,
                                            existingMethod ) );

                                    continue;

                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy != OverrideStrategy.Override:
                                case InterfaceMemberOverrideStrategy.Fail:
                                    diagnostics.Report(
                                        AdviceDiagnosticDescriptors.ImplicitInterfaceMemberAlreadyExists.CreateRoslynDiagnostic(
                                            targetType.GetDiagnosticLocation(),
                                            (this.AspectInstance.AspectClass.ShortName, interfaceMethod, targetType, existingMethod),
                                            this ) );

                                    continue;

                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy == OverrideStrategy.Override:
                                    if ( existingMethod.Accessibility != Accessibility.Public )
                                    {
                                        diagnostics.Report(
                                            AdviceDiagnosticDescriptors.CannotOverrideNonPublicInterfaceMember.CreateRoslynDiagnostic(
                                                targetType.GetDiagnosticLocation(),
                                                (this.AspectInstance.AspectClass.ShortName, existingMethod),
                                                this ) );

                                        continue;
                                    }

                                    if ( existingMethod.DeclaringType.Equals( targetType ) )
                                    {
                                        AddTransformationNoDuplicates(
                                            new OverrideMethodTransformation(
                                                this,
                                                existingMethod,
                                                templateMethod.AssertNotNull().ForOverride( existingMethod ),
                                                mergedTags ) );

                                        implementedInterfaceMembers.Add(
                                            new MemberImplementationResult(
                                                compilation,
                                                memberSpec.InterfaceMember,
                                                InterfaceMemberImplementationOutcome.Override,
                                                existingMethod ) );
                                    }
                                    else
                                    {
                                        if ( !existingMethod.IsVirtual || existingMethod.IsSealed )
                                        {
                                            diagnostics.Report(
                                                AdviceDiagnosticDescriptors.CannotOverrideNonVirtualInterfaceMember.CreateRoslynDiagnostic(
                                                    targetType.GetDiagnosticLocation(),
                                                    (this.AspectInstance.AspectClass.ShortName, existingMethod),
                                                    this ) );

                                            continue;
                                        }

                                        IntroduceMethod( false, true );
                                    }

                                    break;

                                case InterfaceMemberOverrideStrategy.MakeExplicit:
                                    IntroduceMethod( true, false );

                                    break;

                                default:
                                    throw new AssertionFailedException(
                                        $"Unexpected value for InterfaceMemberOverrideStrategy: {memberSpec.OverrideStrategy}." );
                            }
#pragma warning restore CS0618
                        }
                        else
                        {
                            IntroduceMethod( memberSpec.IsExplicit, false );
                        }

                        void IntroduceMethod( bool isExplicit, bool isOverride )
                        {
                            var isIteratorMethod = templateMethod?.IsIteratorMethod ?? redirectionTargetMethod.AssertNotNull().IsIteratorMethod() == true;
                            var isVirtual = templateAttributeProperties?.IsVirtual ?? templateMethod is { Declaration.IsVirtual: true };

                            var methodBuilder = this.GetImplMethodBuilder( targetType, interfaceMethod, isIteratorMethod, isExplicit, isVirtual, isOverride );

                            CopyAttributes( interfaceMethod, methodBuilder );

                            AddTransformationNoDuplicates( methodBuilder.ToTransformation() );
                            interfaceMemberMap.Add( interfaceMethod, methodBuilder );

                            if ( templateMethod != null )
                            {
                                AddTransformationNoDuplicates(
                                    new OverrideMethodTransformation(
                                        this,
                                        methodBuilder,
                                        templateMethod.ForIntroduction( methodBuilder ),
                                        mergedTags ) );
                            }
                            else
                            {
                                AddTransformationNoDuplicates(
                                    new RedirectMethodTransformation(
                                        this,
                                        methodBuilder,
                                        (IMethod) memberSpec.TargetMember.AssertNotNull() ) );
                            }

                            implementedInterfaceMembers.Add(
                                new MemberImplementationResult(
                                    compilation,
                                    memberSpec.InterfaceMember,
                                    InterfaceMemberImplementationOutcome.Introduce,
                                    methodBuilder ) );
                        }

                        break;

                    case IProperty interfaceProperty:
                        var existingProperty = targetType.Properties.SingleOrDefault( p => p.SignatureEquals( interfaceProperty ) );
                        var templateProperty = memberSpec.Template?.Cast<IProperty>();
                        var redirectionTargetProperty = (IProperty?) memberSpec.TargetMember;

                        if ( existingProperty != null && !memberSpec.IsExplicit )
                        {
#pragma warning disable CS0618 // Type is obsolete
                            switch ( memberSpec.OverrideStrategy )
                            {
                                case { } when forceIgnore:
                                case InterfaceMemberOverrideStrategy.Ignore:

                                    implementedInterfaceMembers.Add(
                                        new MemberImplementationResult(
                                            compilation,
                                            memberSpec.InterfaceMember,
                                            InterfaceMemberImplementationOutcome.UseExisting,
                                            existingProperty ) );

                                    continue;

                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy != OverrideStrategy.Override:
                                case InterfaceMemberOverrideStrategy.Fail:
                                    diagnostics.Report(
                                        AdviceDiagnosticDescriptors.ImplicitInterfaceMemberAlreadyExists.CreateRoslynDiagnostic(
                                            targetType.GetDiagnosticLocation(),
                                            (this.AspectInstance.AspectClass.ShortName, interfaceProperty, targetType, existingProperty),
                                            this ) );

                                    continue;

                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy == OverrideStrategy.Override:
                                    if ( existingProperty.Accessibility != Accessibility.Public )
                                    {
                                        diagnostics.Report(
                                            AdviceDiagnosticDescriptors.CannotOverrideNonPublicInterfaceMember.CreateRoslynDiagnostic(
                                                targetType.GetDiagnosticLocation(),
                                                (this.AspectInstance.AspectClass.ShortName, existingProperty),
                                                this ) );

                                        continue;
                                    }

                                    if ( existingProperty.DeclaringType.Equals( targetType ) )
                                    {
                                        var accessorTemplates = templateProperty.GetAccessorTemplates();

                                        AddTransformationNoDuplicates(
                                            new OverridePropertyTransformation(
                                                this,
                                                existingProperty,
                                                existingProperty.GetMethod != null ? accessorTemplates.Get?.ForOverride( existingProperty.GetMethod ) : null,
                                                existingProperty.SetMethod != null ? accessorTemplates.Set?.ForOverride( existingProperty.SetMethod ) : null,
                                                mergedTags ) );

                                        implementedInterfaceMembers.Add(
                                            new MemberImplementationResult(
                                                compilation,
                                                memberSpec.InterfaceMember,
                                                InterfaceMemberImplementationOutcome.Override,
                                                existingProperty ) );
                                    }
                                    else
                                    {
                                        if ( !existingProperty.IsVirtual || existingProperty.IsSealed )
                                        {
                                            diagnostics.Report(
                                                AdviceDiagnosticDescriptors.CannotOverrideNonVirtualInterfaceMember.CreateRoslynDiagnostic(
                                                    targetType.GetDiagnosticLocation(),
                                                    (this.AspectInstance.AspectClass.ShortName, existingProperty),
                                                    this ) );

                                            continue;
                                        }

                                        IntroduceProperty( false, true );
                                    }

                                    break;

                                case InterfaceMemberOverrideStrategy.MakeExplicit:
                                    IntroduceProperty( true, false );

                                    break;

                                default:
                                    throw new NotImplementedException( $"The strategy OverrideStrategy.{this._overrideStrategy} is not implemented." );
                            }
#pragma warning restore CS0618
                        }
                        else
                        {
                            IntroduceProperty( memberSpec.IsExplicit, false );
                        }

                        void IntroduceProperty( bool isExplicit, bool isOverride )
                        {
                            var isAutoProperty = templateProperty?.Declaration.IsAutoPropertyOrField == true;
                            var isVirtual = templateAttributeProperties?.IsVirtual ?? templateProperty is { Declaration.IsVirtual: true };

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
                                        (true, _, _, _) => "get",              // Missing getter.
                                        (false, true, _, false) => "set",      // Missing setter.
                                        (false, true, _, true) => "init",      // Missing init-only setter.
                                        (false, false, true, false) => "set",  // Interface has setter, template has init-only setter.
                                        (false, false, false, true) => "init", // Interface has init-only setter, template has setter.
                                        _ => null
                                    };

                                if ( missingAccessor != null )
                                {
                                    diagnostics.Report(
                                        AdviceDiagnosticDescriptors.InterfacePropertyIsMissingAccessor.CreateRoslynDiagnostic(
                                            targetType.GetDiagnosticLocation(),
                                            (this.AspectInstance.AspectClass.ShortName, interfaceProperty, targetType, templateProperty.Declaration,
                                             missingAccessor),
                                            this ) );

                                    return;
                                }

                                var unexpectedAccessor =
                                    (isExplicit, getMethodUnexpectedInTemplate, setMethodUnexpectedInTemplate, setInitOnlyInTemplate) switch
                                    {
                                        (true, true, _, _) => "get",         // Unexpected getter.
                                        (true, false, true, false) => "set", // Unexpected setter.
                                        (true, false, true, true) => "init", // Unexpected init-only setter.
                                        _ => null
                                    };

                                if ( unexpectedAccessor != null )
                                {
                                    diagnostics.Report(
                                        AdviceDiagnosticDescriptors.ExplicitInterfacePropertyHasSuperficialAccessor.CreateRoslynDiagnostic(
                                            targetType.GetDiagnosticLocation(),
                                            (this.AspectInstance.AspectClass.ShortName, interfaceProperty, targetType, templateProperty.Declaration,
                                             unexpectedAccessor),
                                            this ) );

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
                                isVirtual,
                                isOverride,
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

                            AddTransformationNoDuplicates( propertyBuilder.ToTransformation() );
                            interfaceMemberMap.Add( interfaceProperty, propertyBuilder );

                            if ( templateProperty != null )
                            {
                                if ( isAutoProperty != true )
                                {
                                    var accessorTemplates = templateProperty.GetAccessorTemplates();

                                    AddTransformationNoDuplicates(
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

                                    OverrideHelper.AddTransformationsForStructField(
                                        targetType.ForCompilation( compilation ),
                                        this,
                                        AddTransformationNoDuplicates );
                                }
                            }
                            else
                            {
                                AddTransformationNoDuplicates(
                                    new RedirectPropertyTransformation(
                                        this,
                                        propertyBuilder,
                                        redirectionTargetProperty.AssertNotNull() ) );
                            }

                            implementedInterfaceMembers.Add(
                                new MemberImplementationResult(
                                    compilation,
                                    memberSpec.InterfaceMember,
                                    InterfaceMemberImplementationOutcome.Introduce,
                                    propertyBuilder ) );
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
#pragma warning disable CS0618 // Type is obsolete
                            switch ( memberSpec.OverrideStrategy )
                            {
                                case { } when forceIgnore:
                                case InterfaceMemberOverrideStrategy.Ignore:

                                    implementedInterfaceMembers.Add(
                                        new MemberImplementationResult(
                                            compilation,
                                            memberSpec.InterfaceMember,
                                            InterfaceMemberImplementationOutcome.UseExisting,
                                            existingEvent ) );

                                    continue;

                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy != OverrideStrategy.Override:
                                case InterfaceMemberOverrideStrategy.Fail:
                                    diagnostics.Report(
                                        AdviceDiagnosticDescriptors.ImplicitInterfaceMemberAlreadyExists.CreateRoslynDiagnostic(
                                            targetType.GetDiagnosticLocation(),
                                            (this.AspectInstance.AspectClass.ShortName, interfaceEvent, targetType, existingEvent),
                                            this ) );

                                    continue;

                                case InterfaceMemberOverrideStrategy.Default when this._overrideStrategy == OverrideStrategy.Override:
                                    if ( existingEvent.Accessibility != Accessibility.Public )
                                    {
                                        diagnostics.Report(
                                            AdviceDiagnosticDescriptors.CannotOverrideNonPublicInterfaceMember.CreateRoslynDiagnostic(
                                                targetType.GetDiagnosticLocation(),
                                                (this.AspectInstance.AspectClass.ShortName, existingEvent),
                                                this ) );

                                        continue;
                                    }

                                    if ( existingEvent.DeclaringType.Equals( targetType ) )
                                    {
                                        var accessorTemplates = templateEvent.GetAccessorTemplates();

                                        AddTransformationNoDuplicates(
                                            new OverrideEventTransformation(
                                                this,
                                                existingEvent,
                                                accessorTemplates.Add?.ForOverride( existingEvent.AddMethod ),
                                                accessorTemplates.Remove?.ForOverride( existingEvent.RemoveMethod ),
                                                mergedTags ) );

                                        implementedInterfaceMembers.Add(
                                            new MemberImplementationResult(
                                                compilation,
                                                memberSpec.InterfaceMember,
                                                InterfaceMemberImplementationOutcome.Override,
                                                existingEvent ) );
                                    }
                                    else
                                    {
                                        if ( !existingEvent.IsVirtual || existingEvent.IsSealed )
                                        {
                                            diagnostics.Report(
                                                AdviceDiagnosticDescriptors.CannotOverrideNonVirtualInterfaceMember.CreateRoslynDiagnostic(
                                                    targetType.GetDiagnosticLocation(),
                                                    (this.AspectInstance.AspectClass.ShortName, existingEvent),
                                                    this ) );

                                            continue;
                                        }

                                        IntroduceEvent( false, true );
                                    }

                                    break;

                                case InterfaceMemberOverrideStrategy.MakeExplicit:
                                    IntroduceEvent( true, false );

                                    break;

                                default:
                                    throw new NotImplementedException( $"The strategy OverrideStrategy.{this._overrideStrategy} is not implemented." );
                            }
#pragma warning restore CS0618
                        }
                        else
                        {
                            IntroduceEvent( memberSpec.IsExplicit, false );
                        }

                        void IntroduceEvent( bool isExplicit, bool isOverride )
                        {
                            var isEventField = templateEvent?.Declaration.IsEventField() ?? false;
                            var isVirtual = templateAttributeProperties?.IsVirtual ?? templateEvent is { Declaration.IsVirtual: true };

                            var eventBuilder = this.GetImplEventBuilder(
                                targetType,
                                interfaceEvent,
                                isEventField,
                                isExplicit,
                                isVirtual,
                                isOverride,
                                mergedTags );

                            if ( templateEvent != null )
                            {
                                CopyAttributes( templateEvent.Declaration, eventBuilder );
                                CopyAttributes( templateEvent.Declaration.AssertNotNull(), (DeclarationBuilder) eventBuilder.AddMethod.AssertNotNull() );
                                CopyAttributes( templateEvent.Declaration.AssertNotNull(), (DeclarationBuilder) eventBuilder.RemoveMethod.AssertNotNull() );
                            }

                            AddTransformationNoDuplicates( eventBuilder.ToTransformation() );
                            interfaceMemberMap.Add( interfaceEvent, eventBuilder );

                            if ( templateEvent != null )
                            {
                                if ( !isEventField )
                                {
                                    var accessorTemplates = templateEvent.GetAccessorTemplates();

                                    AddTransformationNoDuplicates(
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

                                    OverrideHelper.AddTransformationsForStructField(
                                        targetType.ForCompilation( compilation ),
                                        this,
                                        AddTransformationNoDuplicates );
                                }
                            }
                            else
                            {
                                AddTransformationNoDuplicates(
                                    new RedirectEventTransformation(
                                        this,
                                        eventBuilder,
                                        redirectionTargetEvent.AssertNotNull() ) );
                            }

                            implementedInterfaceMembers.Add(
                                new MemberImplementationResult(
                                    compilation,
                                    memberSpec.InterfaceMember,
                                    InterfaceMemberImplementationOutcome.Introduce,
                                    eventBuilder ) );
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
                AddTransformationNoDuplicates(
                    new IntroduceInterfaceTransformation( this, targetType, interfaceSpecification.InterfaceType, interfaceMemberMap ) );
            }
        }

        if ( diagnostics.HasError() )
        {
            return this.CreateFailedResult( diagnostics.ToImmutableArray() );
        }

        return new ImplementInterfaceAdviceResult(
            implementedInterfaces.All( i => i.Outcome == InterfaceImplementationOutcome.Ignore ) ? AdviceOutcome.Ignore : AdviceOutcome.Default,
            diagnostics.Count > 0 ? diagnostics.ToImmutableArray() : ImmutableArray<Diagnostic>.Empty,
            implementedInterfaces,
            implementedInterfaceMembers );
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
        bool isVirtual,
        bool isOverride,
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

        if ( isOverride )
        {
            propertyBuilder.IsOverride = true;
        }
        else if ( isVirtual )
        {
            propertyBuilder.IsVirtual = true;
        }

        return propertyBuilder;
    }

    private Location? GetDiagnosticLocation() => this.TargetDeclaration.GetTarget( this.SourceCompilation ).GetDiagnosticLocation();

    private EventBuilder GetImplEventBuilder(
        INamedType declaringType,
        IEvent interfaceEvent,
        bool isEventField,
        bool isExplicit,
        bool isVirtual,
        bool isOverride,
        IObjectReader tags )
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

        if ( isOverride )
        {
            eventBuilder.IsOverride = true;
        }
        else if ( isVirtual )
        {
            eventBuilder.IsVirtual = true;
        }

        return eventBuilder;
    }

    private static string GetInterfaceMemberName( IMember interfaceMember, bool isExplicit )
        => isExplicit ? $"{interfaceMember.DeclaringType.FullName}.{interfaceMember.Name}" : interfaceMember.Name;
}