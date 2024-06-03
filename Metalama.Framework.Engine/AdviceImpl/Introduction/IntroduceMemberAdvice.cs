// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal abstract class IntroduceMemberAdvice<TTemplate, TIntroduced, TBuilder> : IntroduceDeclarationAdvice<TIntroduced, TBuilder>
    where TTemplate : class, IMember
    where TIntroduced : class, IMember
    where TBuilder : MemberBuilder, TIntroduced
{
    private readonly IntroductionScope _scope;

    private readonly INamedType? _explicitlyImplementedInterfaceType;

    protected new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

    protected string MemberName { get; }

    protected TemplateMember<TTemplate>? Template { get; }

    protected OverrideStrategy OverrideStrategy { get; }

    protected IObjectReader Tags { get; }

    protected IntroduceMemberAdvice(
        AdviceConstructorParameters<INamedType> parameters,
        string? explicitName,
        TemplateMember<TTemplate>? template,
        IntroductionScope scope,
        OverrideStrategy overrideStrategy,
        Action<TBuilder>? buildAction,
        IObjectReader tags,
        INamedType? explicitlyImplementedInterfaceType )
        : base( parameters, buildAction )
    {
        var templateAttribute = (ITemplateAttribute?) template?.AdviceAttribute;
        var templateAttributeProperties = templateAttribute?.Properties;

        this.MemberName = explicitName ?? templateAttributeProperties?.Name
            ?? template?.Declaration.Name ?? throw new ArgumentNullException( nameof(explicitName) );

        this.Template = template;

        if ( scope != IntroductionScope.Default )
        {
            this._scope = scope;
        }
        else if ( templateAttribute is IntroduceAttribute introduceAttribute )
        {
            this._scope = introduceAttribute.Scope;
        }

        if ( this._scope == IntroductionScope.Target )
        {
            this._scope = parameters.AspectInstance.TargetDeclaration.GetTarget( parameters.SourceCompilation ).GetClosestMemberOrNamedType()?.IsStatic == false
                ? IntroductionScope.Instance
                : IntroductionScope.Static;
        }

        this.OverrideStrategy = overrideStrategy;
        this.Tags = tags;
        this._explicitlyImplementedInterfaceType = explicitlyImplementedInterfaceType;
    }

    protected virtual void InitializeCore(
        ProjectServiceProvider serviceProvider,
        IDiagnosticAdder diagnosticAdder,
        TemplateAttributeProperties? templateAttributeProperties ) { }

    protected sealed override void Initialize( in ProjectServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
    {
        base.Initialize( serviceProvider, diagnosticAdder );

        var templateAttribute = (ITemplateAttribute?) this.Template?.AdviceAttribute;
        var templateAttributeProperties = templateAttribute?.Properties;

        this.Builder.Accessibility = this.Template?.Accessibility ?? Accessibility.Private;
        this.Builder.IsSealed = templateAttributeProperties?.IsSealed ?? this.Template?.Declaration.IsSealed ?? false;
        this.Builder.IsVirtual = templateAttributeProperties?.IsVirtual ?? this.Template?.Declaration.IsVirtual ?? false;

        // Handle the introduction scope.

        switch ( this._scope )
        {
            case IntroductionScope.Default:
                if ( this.Template?.Declaration is { IsStatic: true } )
                {
                    this.Builder.IsStatic = true;
                }
                else
                {
                    this.Builder.IsStatic = false;
                }

                break;

            case IntroductionScope.Instance:
                this.Builder.IsStatic = false;

                break;

            case IntroductionScope.Static:
                this.Builder.IsStatic = true;

                break;

            default:
                throw new AssertionFailedException( $"Unexpected IntroductionScope: {this._scope}." );
        }

        if ( this.Template != null )
        {
            CopyTemplateAttributes( this.Template.Declaration!, this.Builder, serviceProvider );
        }

        this.InitializeCore( serviceProvider, diagnosticAdder, templateAttributeProperties );

        this.BuildAction?.Invoke( this.Builder );

        SetBuilderExplicitInterfaceImplementation( this.Builder, this._explicitlyImplementedInterfaceType );
    }

    protected override void Validate( in ProjectServiceProvider serviceProvider, CompilationModel compilation, IDiagnosticAdder diagnosticAdder )
    {
        this.ValidateBuilder( this.TargetDeclaration.GetTarget( compilation ), diagnosticAdder );
    }

    protected virtual void ValidateBuilder( INamedType targetDeclaration, IDiagnosticAdder diagnosticAdder )
    {
        // Check that static member is not virtual.
        if ( this.Builder is { IsStatic: true, IsVirtual: true } )
        {
            diagnosticAdder.Report(
                AdviceDiagnosticDescriptors.CannotIntroduceStaticVirtualMember.CreateRoslynDiagnostic(
                    targetDeclaration.GetDiagnosticLocation(),
                    (this.AspectInstance.AspectClass.ShortName, this.Builder),
                    this ) );
        }

        // Check that static member is not sealed.
        if ( this.Builder is { IsStatic: true, IsSealed: true } )
        {
            diagnosticAdder.Report(
                AdviceDiagnosticDescriptors.CannotIntroduceStaticSealedMember.CreateRoslynDiagnostic(
                    targetDeclaration.GetDiagnosticLocation(),
                    (this.AspectInstance.AspectClass.ShortName, this.Builder),
                    this ) );
        }

        // Check that instance member is not introduced to a static type.
        if ( targetDeclaration.IsStatic && !this.Builder.IsStatic )
        {
            diagnosticAdder.Report(
                AdviceDiagnosticDescriptors.CannotIntroduceInstanceMember.CreateRoslynDiagnostic(
                    targetDeclaration.GetDiagnosticLocation(),
                    (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration),
                    this ) );
        }

        // Check that virtual member is not introduced to a sealed type or a struct.
        if ( targetDeclaration is { IsSealed: true } or { TypeKind: TypeKind.Struct or TypeKind.RecordStruct }
             && this.Builder.IsVirtual )
        {
            diagnosticAdder.Report(
                AdviceDiagnosticDescriptors.CannotIntroduceVirtualToTargetType.CreateRoslynDiagnostic(
                    targetDeclaration.GetDiagnosticLocation(),
                    (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration),
                    this ) );
        }
    }

    protected static void CopyTemplateAttributes( IDeclaration declaration, IDeclarationBuilder builder, in ProjectServiceProvider serviceProvider )
    {
        var classificationService = serviceProvider.Global.GetRequiredService<AttributeClassificationService>();

        foreach ( var codeElementAttribute in declaration.Attributes )
        {
            if ( classificationService.MustCopyTemplateAttribute( codeElementAttribute ) )
            {
                builder.AddAttribute( codeElementAttribute.ToAttributeConstruction() );
            }
        }
    }

    private static void SetBuilderExplicitInterfaceImplementation( TBuilder builder, INamedType? explicitlyImplementedInterfaceType )
    {
        if ( explicitlyImplementedInterfaceType == null )
        {
            return;
        }

        switch ( builder )
        {
            case MethodBuilder methodBuilder:
                if ( explicitlyImplementedInterfaceType.Methods.OfExactSignature( methodBuilder ) is { } interfaceMethod )
                {
                    methodBuilder.SetExplicitInterfaceImplementation( interfaceMethod );

                    return;
                }

                break;

            case PropertyBuilder propertyBuilder:
                if ( explicitlyImplementedInterfaceType.Properties.OfName( propertyBuilder.Name ).SingleOrDefault() is { } interfaceProperty )
                {
                    propertyBuilder.SetExplicitInterfaceImplementation( interfaceProperty );

                    return;
                }

                break;

            case EventBuilder eventBuilder:
                if ( explicitlyImplementedInterfaceType.Events.OfName( eventBuilder.Name ).SingleOrDefault() is { } interfaceEvent )
                {
                    eventBuilder.SetExplicitInterfaceImplementation( interfaceEvent );

                    return;
                }

                break;

            case IndexerBuilder indexerBuilder:
                if ( explicitlyImplementedInterfaceType.Indexers.OfExactSignature( indexerBuilder ) is { } interfaceIndexer )
                {
                    indexerBuilder.SetExplicitInterfaceImplementation( interfaceIndexer );

                    return;
                }

                break;
        }

        throw new InvalidOperationException(
            MetalamaStringFormatter.Format(
                $"The member '{builder}' can't be used to explicitly implement the interface '{explicitlyImplementedInterfaceType}', because it doesn't match any member of the interface." ) );
    }

    public override string ToString() => $"Introduce {this.Builder}";
}