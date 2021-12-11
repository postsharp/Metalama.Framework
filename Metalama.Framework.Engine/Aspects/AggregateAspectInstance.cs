// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects
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

        IRef<IDeclaration> IAspectInstance.TargetDeclaration => this.TargetDeclaration;

        public Ref<IDeclaration> TargetDeclaration => this._primaryInstance.TargetDeclaration;

        public IAspectClass AspectClass => this._primaryInstance.AspectClass;

        public bool IsSkipped => this._primaryInstance.IsSkipped;

        public ImmutableArray<IAspectInstance> SecondaryInstances => this._otherInstances.Cast<IAspectInstance>().ToImmutableArray();

        public ImmutableArray<AspectPredecessor> Predecessors => ImmutableArray.Create( this._primaryInstance.Predecessor );

        public object? TargetTag => this._primaryInstance.TargetTag;

        public void SetTargetTag( object? value ) => this._primaryInstance.TargetTag = value;

        public void Skip() => this._primaryInstance.Skip();

        public ImmutableDictionary<TemplateClass, TemplateClassInstance> TemplateInstances => this._primaryInstance.TemplateInstances;

        public FormattableString FormatPredecessor( ICompilation compilation ) => this._primaryInstance.FormatPredecessor( compilation );

        public Location? GetDiagnosticLocation( Compilation compilation ) => this._primaryInstance.GetDiagnosticLocation( compilation );
    }
}