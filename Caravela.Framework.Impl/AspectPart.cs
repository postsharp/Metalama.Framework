﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal class AspectPart
    {
        public AspectType AspectType { get; }

        /// <summary>
        /// Gets the name of the part, or <c>null</c> for the default part.
        /// </summary>
        public string? PartName { get; }

        public AspectPart( AspectType aspectType, string? partName = null )
        {
            this.AspectType = aspectType;
            this.PartName = partName;
        }

        internal AspectPartResult ToResult( AspectPartResult input )
        {
            var aspectDriver = (AspectDriver) this.AspectType.AspectDriver;

            // Takes our aspects.
            var aspectInstances = input.Aspects.Where( a => a.AspectType.Name == this.AspectType.Name );

            // Run the aspect initializers.
            IEnumerable<Advice> addedAdvices;
            IEnumerable<Diagnostic> aspectInitializerDiagnostics;
            IEnumerable<AspectInstance> addedAspects;
            if ( this.PartName == null )
            {
                // If we are in the default aspect part, we have to execute the aspect initializer.
                
                var instanceResults = aspectInstances.Select( ai => aspectDriver.EvaluateAspect( ai ) ).ToImmutableArray();

                aspectInitializerDiagnostics = instanceResults.SelectMany( air => air.Diagnostics );
                
                addedAspects = instanceResults.SelectMany( air => air.Aspects );

                addedAdvices = instanceResults.SelectMany( air => air.Advices ).Cast<Advice>();
            }
            else
            {
                addedAdvices = Enumerable.Empty<Advice>();
                aspectInitializerDiagnostics = Enumerable.Empty<Diagnostic>();
                addedAspects = Enumerable.Empty<AspectInstance>();
            }

            var advicesInCurrentAspectParts = 
                input.Advices.Concat(  addedAdvices ).Where( a => a.Aspect.AspectType.Name == this.AspectType.Name && a.PartName == this.PartName );

            var adviceResults = advicesInCurrentAspectParts
                .Select( ai => ai.ToResult( input.Compilation ) ).ToList();


            var addedObservableIntroductions = adviceResults.SelectMany( ar => ar.ObservableTransformations );
            var addedNonObservableTransformations = adviceResults.SelectMany( ar => ar.NonObservableTransformations );
            
            var newCompilation = new RoslynBasedCompilationModel( (RoslynBasedCompilationModel) input.Compilation, addedObservableIntroductions );




            return input.WithNewResults( newCompilation,
                aspectInitializerDiagnostics.Concat( adviceResults.SelectMany( ar => ar.Diagnostics ) ).ToList(),
                addedAspects.ToList(),
                addedAdvices.ToList(),
                addedNonObservableTransformations.ToList());




        }
    }
}
