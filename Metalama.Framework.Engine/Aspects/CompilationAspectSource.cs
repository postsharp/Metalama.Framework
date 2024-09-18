// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// An implementation  of <see cref="IAspectSource"/> that creates aspect instances from custom attributes
/// found in a compilation.
/// </summary>
internal sealed class CompilationAspectSource : IAspectSource
{
    private readonly IAttributeDeserializer _attributeDeserializer;
    private readonly IConcurrentTaskRunner _concurrentTaskRunner;
    private ImmutableDictionaryOfArray<IType, IRef<IDeclaration>>? _exclusions;

    public CompilationAspectSource( in ProjectServiceProvider serviceProvider, ImmutableArray<IAspectClass> aspectTypes )
    {
        this._attributeDeserializer = serviceProvider.GetRequiredService<IUserCodeAttributeDeserializer>();
        this._concurrentTaskRunner = serviceProvider.GetRequiredService<IConcurrentTaskRunner>();
        this.AspectClasses = aspectTypes;
    }

    public ImmutableArray<IAspectClass> AspectClasses { get; }

    private ImmutableDictionaryOfArray<IType, IRef<IDeclaration>> DiscoverExclusions( CompilationModel compilation )
    {
        if ( this._exclusions == null )
        {
            var excludeAspectType = (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(ExcludeAspectAttribute) );

            this._exclusions =
                compilation.GetAllAttributesOfType( excludeAspectType )
                    .SelectMany(
                        a => a.ConstructorArguments[0]
                            .Values.Select( arg => (TargetDeclaration: a.ContainingDeclaration.ToRef(), AspectType: (IType) arg.Value!) ) )
                    .ToMultiValueDictionary( x => x.AspectType, x => x.TargetDeclaration );
        }

        return this._exclusions;
    }

    public Task CollectAspectInstancesAsync(
        IAspectClass aspectClass,
        OutboundActionCollectionContext context )
    {
        var compilation = context.Compilation;
        var cancellationToken = context.CancellationToken;

        if ( !compilation.Factory.TryGetTypeByReflectionName( aspectClass.FullName, out var aspectType ) )
        {
            // This happens at design time when the IDE sends an incomplete compilation. We cannot apply the aspects in this case,
            // but we prefer not to throw an exception since the case is expected.
            return Task.CompletedTask;
        }

        // Process exclusions.
        var exclusions = this.DiscoverExclusions( compilation )[aspectType];

        foreach ( var exclusion in exclusions )
        {
            context.Collector.AddExclusion( exclusion );
        }

        // Process attributes in parallel.
        var attributes = compilation.GetAllAttributesOfType( aspectType );

        return this._concurrentTaskRunner.RunConcurrentlyAsync( attributes, ProcessAttribute, cancellationToken );

        void ProcessAttribute( IAttribute attribute )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var attributeData = attribute.GetAttributeData();

            if ( this._attributeDeserializer.TryCreateAttribute( attributeData, context.Collector, out var attributeInstance ) )
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

                    context.Collector.Report(
                        GeneralDiagnosticDescriptors.AspectNotEligibleOnTarget.CreateRoslynDiagnostic(
                            attribute.GetDiagnosticLocation(),
                            (aspectClass.ShortName, targetDeclaration.DeclarationKind, targetDeclaration, reason) ) );
                }

                context.Collector.AddAspectInstance( aspectInstance );
            }
        }
    }
}