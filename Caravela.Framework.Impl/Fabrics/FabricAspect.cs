// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// An implementation of <see cref="IAspect"/> that invokes all fabrics on the declaration.
    /// </summary>
    internal class FabricAspect<T> : IAspect<T>
        where T : class, IDeclaration
    {
        private ImmutableArray<FabricTemplateClass> _templateClasses;

        public FabricAspect( ImmutableArray<FabricTemplateClass> templateClasses )
        {
            this._templateClasses = templateClasses;
        }

        public void BuildAspect( IAspectBuilder<T> builder )
        {
            var internalBuilder = (IAspectBuilderInternal) builder;

            foreach ( var templateClass in this._templateClasses )
            {
                var fabricInstance = new FabricInstance( templateClass.Driver, builder.Target );

                if ( fabricInstance.Driver.Kind is FabricKind.Namespace or FabricKind.Type )
                {
                    // We need to freeze project data before execution of type fabrics. Only project fabrics can modify the state.
                    ((ProjectModel) builder.Project).FreezeProjectData();
                }

                using ( internalBuilder.WithPredecessor( new AspectPredecessor( AspectPredecessorKind.Fabric, fabricInstance ) ) )
                {
                    templateClass.Driver.Execute( internalBuilder, templateClass, fabricInstance );
                }
            }
            
            // Prevent further modifications of the project data.
            ((ProjectModel) builder.Project).FreezeProjectData();
        }

        void IAspect.BuildAspectClass( IAspectClassBuilder builder ) { }

        void IEligible<T>.BuildEligibility( IEligibilityBuilder<T> builder ) { }

        public IEnumerable<IFabric> Fabrics => this._templateClasses.Select( t => t.Driver.Fabric );
    }
}