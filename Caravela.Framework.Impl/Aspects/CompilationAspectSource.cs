// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Aspects
{
    /// <summary>
    /// An implementation  of <see cref="IAspectSource"/> that creates aspect instances from custom attributes
    /// found in a compilation.
    /// </summary>
    internal class CompilationAspectSource : IAspectSource
    {
        private readonly CompileTimeProjectLoader _loader;

        public CompilationAspectSource( ImmutableArray<IAspectClass> aspectTypes, CompileTimeProjectLoader loader )
        {
            this._loader = loader;
            this.AspectClasses = aspectTypes;
        }

        public ImmutableArray<IAspectClass> AspectClasses { get; }

        // TODO: implement aspect exclusion based on ExcludeAspectAttribute
        public IEnumerable<IDeclaration> GetExclusions( INamedType aspectType ) => Enumerable.Empty<IDeclaration>();

        public IEnumerable<AspectInstance> GetAspectInstances(
            CompilationModel compilation,
            IAspectClass aspectClass,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
        {
            if ( !compilation.Factory.TryGetTypeByReflectionName( aspectClass.FullName, out var aspectType ) )
            {
                // This happens at design time when the IDE sends an incomplete compilation. We cannot apply the aspects in this case,
                // but we prefer not to throw an exception since the case is expected.
                return Enumerable.Empty<AspectInstance>();
            }

            return compilation.GetAllAttributesOfType( aspectType )
                .Select(
                    attribute =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var attributeData = attribute.GetAttributeData();

                        if ( this._loader.AttributeDeserializer.TryCreateAttribute( attributeData, diagnosticAdder, out var attributeInstance ) )
                        {
                            var aspectInstance = ((AspectClass) aspectClass).CreateAspectInstanceFromAttribute(
                                (IAspect) attributeInstance,
                                attribute.ContainingDeclaration.AssertNotNull(),
                                attribute,
                                this._loader );

                            if ( aspectInstance.Eligibility == EligibleScenarios.None )
                            {
                                var reason = ((AspectClass) aspectClass).GetIneligibilityJustification( aspectInstance.TargetDeclaration, EligibleScenarios.Inheritance )!;

                                diagnosticAdder.Report(
                                    GeneralDiagnosticDescriptors.AspectNotEligibleOnAspect.CreateDiagnostic(
                                        attribute.GetDiagnosticLocation(),
                                        (aspectClass.DisplayName, aspectInstance.TargetDeclaration, reason) ) );

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
        }
    }
}