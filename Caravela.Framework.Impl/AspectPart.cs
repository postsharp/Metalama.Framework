using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal class AspectPart : AspectPartId
    {
        


        public AspectPart( AspectType aspectType, string? partName = null ) : base( aspectType.Type, partName )
        {
            this.AspectType = aspectType;
        }

        public AspectType AspectType { get; }
        

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="prototype"></param>
        protected AspectPart( AspectPart prototype ) : this( prototype.AspectType, prototype.PartName )
        {
            
        }

        internal AspectPartResult Execute( AspectPartResult input )
        {
            var aspectDriver = (AspectDriver) this.AspectType.AspectDriver;

            // Run the aspect initializers.
            IEnumerable<Advice> addedAdvices;
            IEnumerable<Diagnostic> aspectInitializerDiagnostics;
            IEnumerable<IAspectSource> addedAspectSources;
            if ( this.IsDefault )
            {
                // If we are in the default aspect part, we have to execute the aspect initializer.
                
                // TODO: order declarations by depth of inheritance. 

                var aspectInstances = input.AspectSources.SelectMany( s => s.GetAspectInstances( this.AspectType.Type ) );

                var instanceResults = aspectInstances.Select( ai => aspectDriver.EvaluateAspect( ai ) ).ToImmutableArray();

                aspectInitializerDiagnostics = instanceResults.SelectMany( air => air.Diagnostics );

                addedAspectSources = instanceResults.SelectMany( air => air.AspectSources );

                addedAdvices = instanceResults.SelectMany( air => air.Advices ).Cast<Advice>();
            }
            else
            {
                addedAdvices = Enumerable.Empty<Advice>();
                aspectInitializerDiagnostics = Enumerable.Empty<Diagnostic>();
                addedAspectSources = Enumerable.Empty<IAspectSource>();
            }

            var advicesInCurrentAspectParts =
                input.Advices.Concat( addedAdvices ).Where( a => a.AspectPartId.Equals( this ) );

            var adviceResults = advicesInCurrentAspectParts
                .Select( ai => ai.ToResult( input.Compilation ) ).ToList();

            var addedObservableIntroductions = adviceResults.SelectMany( ar => ar.ObservableTransformations );
            var addedNonObservableTransformations = adviceResults.SelectMany( ar => ar.NonObservableTransformations );

            var newCompilation = CompilationModel.CreateRevisedInstance(input.Compilation, addedObservableIntroductions);

            return input.WithNewResults( 
                newCompilation,
                aspectInitializerDiagnostics.Concat( adviceResults.SelectMany( ar => ar.Diagnostics ) ).ToList(),
                addedAspectSources.ToList(),
                addedAdvices.ToList(),
                addedNonObservableTransformations.ToList() );
        }
    }
}
