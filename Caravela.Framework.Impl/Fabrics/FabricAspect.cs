// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.Aspects;
using System.Collections.Immutable;

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
                templateClass.Driver.Execute( internalBuilder, templateClass );
            }
        }

        void IAspect.BuildAspectClass( IAspectClassBuilder builder ) { }

        void IEligible<T>.BuildEligibility( IEligibilityBuilder<T> builder ) { }
    }
}