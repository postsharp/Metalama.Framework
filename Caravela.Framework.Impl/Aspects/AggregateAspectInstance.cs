// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Aspects
{
    internal sealed class AggregateAspectInstance : IAspectInstanceInternal
    {
        private readonly AspectInstance _primaryInstance;
        private readonly IReadOnlyList<AspectInstance> _otherInstances;

        private AggregateAspectInstance( AspectInstance firstInstance, List<AspectInstance> otherInstances )
        {
            this._primaryInstance = firstInstance;
            this._otherInstances = otherInstances;
        }

        public static IAspectInstanceInternal GetInstance( IEnumerable<AspectInstance> aspectInstances )
        {
            var instancesList = aspectInstances.ToList();

            if ( instancesList.Count == 1 )
            {
                return instancesList[0];
            }
            else
            {
                instancesList.Sort();

                var firstInstance = instancesList[0];
                instancesList.RemoveAt( 0 );

                return new AggregateAspectInstance( firstInstance, instancesList );
            }
        }

        public IAspect Aspect => this._primaryInstance.Aspect;

        public IDeclaration TargetDeclaration => this._primaryInstance.TargetDeclaration;

        public IAspectClass AspectClass => this._primaryInstance.AspectClass;

        public bool IsSkipped => this._primaryInstance.IsSkipped;

        public ImmutableArray<IAspectInstance> OtherInstances => this._otherInstances.Cast<IAspectInstance>().ToImmutableArray();

        public ImmutableArray<AspectPredecessor> Predecessors => ImmutableArray.Create( this._primaryInstance.Predecessor );

        public void Skip() => this._primaryInstance.Skip();

        public ImmutableDictionary<TemplateClass, TemplateClassInstance> TemplateInstances => this._primaryInstance.TemplateInstances;

        public FormattableString FormatPredecessor() => this._primaryInstance.FormatPredecessor();

        public Location? GetDiagnosticLocation( Compilation compilation ) => this._primaryInstance.GetDiagnosticLocation( compilation );
    }
}