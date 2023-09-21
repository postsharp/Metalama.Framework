// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// An implementation  of <see cref="IAspectSource"/> that creates aspect instances from custom attributes
/// found in a compilation.
/// </summary>
internal sealed class CompilationAspectSource : IAspectSource
{
    private readonly IAttributeDeserializer _attributeDeserializer;
    private ImmutableDictionaryOfArray<IType, Ref<IDeclaration>>? _exclusions;

    public CompilationAspectSource( ProjectServiceProvider serviceProvider, ImmutableArray<IAspectClass> aspectTypes )
    {
        this._attributeDeserializer = serviceProvider.GetRequiredService<IUserCodeAttributeDeserializer>();
        this.AspectClasses = aspectTypes;
    }

    public ImmutableArray<IAspectClass> AspectClasses { get; }

    private ImmutableDictionaryOfArray<IType, Ref<IDeclaration>> DiscoverExclusions( CompilationModel compilation )
    {
        if ( this._exclusions == null )
        {
            var excludeAspectType = (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(ExcludeAspectAttribute) );

            this._exclusions =
                compilation.GetAllAttributesOfType( excludeAspectType )
                    .SelectMany(
                        a => ((ImmutableArray<TypedConstant>) a.ConstructorArguments[0].Value!).Select(
                            arg => (TargetDeclaration: a.ContainingDeclaration.ToTypedRef(), AspectType: (IType) arg.Value!) ) )
                    .ToMultiValueDictionary( x => x.AspectType, x => x.TargetDeclaration );
        }

        return this._exclusions;
    }

    public AspectSourceResult GetAspectInstances(
        CompilationModel compilation,
        IAspectClass aspectClass,
        IDiagnosticAdder diagnosticAdder,
        CancellationToken cancellationToken )
    {
        if ( !compilation.Factory.TryGetTypeByReflectionName( aspectClass.FullName, out var aspectType ) )
        {
            // This happens at design time when the IDE sends an incomplete compilation. We cannot apply the aspects in this case,
            // but we prefer not to throw an exception since the case is expected.
            return AspectSourceResult.Empty;
        }

        var attributes = compilation.GetAllAttributesOfType( aspectType );

        var aspectInstances = attributes
            .Select(
                attribute =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var attributeData = attribute.GetAttributeData();

                    if ( this._attributeDeserializer.TryCreateAttribute( attributeData, diagnosticAdder, out var attributeInstance ) )
                    {
                        var targetDeclaration = attribute.ContainingDeclaration;

                        var aspectInstance = ((AspectClass) aspectClass).CreateAspectInstanceFromAttribute(
                            (IAspect) attributeInstance,
                            targetDeclaration,
                            attribute );

                        var eligibility = aspectInstance.ComputeEligibility( targetDeclaration );

                        if ( eligibility == EligibleScenarios.None )
                        {
                            var requestedEligibility = aspectInstance.IsInheritable ? EligibleScenarios.Inheritance : EligibleScenarios.Default;

                            var reason = ((AspectClass) aspectClass).GetIneligibilityJustification(
                                requestedEligibility,
                                new DescribedObject<IDeclaration>( targetDeclaration ) )!;

                            diagnosticAdder.Report(
                                GeneralDiagnosticDescriptors.AspectNotEligibleOnTarget.CreateRoslynDiagnostic(
                                    attribute.GetDiagnosticLocation(),
                                    (aspectClass.ShortName, targetDeclaration.DeclarationKind, targetDeclaration, reason) ) );

                            return null;
                        }

                        return aspectInstance;
                    }
                    else
                    {
                        return null;
                    }
                } )
            .WhereNotNull();

        var exclusions = this.DiscoverExclusions( compilation )[aspectType];

        return new AspectSourceResult( aspectInstances, exclusions );
    }
}