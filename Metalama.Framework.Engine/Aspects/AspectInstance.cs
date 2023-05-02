﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Represents an instance of an aspect and its target declaration.
    /// </summary>
    internal sealed class AspectInstance : IAspectInstanceInternal, IComparable<AspectInstance>
    {
        /// <summary>
        /// Gets the aspect instance.
        /// </summary>
        public IAspect Aspect { get; }

        IAspectClass IAspectInstance.AspectClass => this.AspectClass;

        public IAspectClassImpl AspectClass { get; }

        IRef<IDeclaration> IAspectPredecessor.TargetDeclaration => this.TargetDeclaration;

        public Ref<IDeclaration> TargetDeclaration { get; }

        public bool IsSkipped { get; private set; }

        // When aspect inheritance is not licensed, the this.AspectClass.IsInheritable is set to false
        // and the ((IConditionallyInheritableAspect) this.Aspect).IsInheritable is therefore not considered.
        public bool IsInheritable => this.AspectClass.IsInheritable ?? ((IConditionallyInheritableAspect) this.Aspect).IsInheritable;

        public ImmutableArray<IAspectInstance> SecondaryInstances => ImmutableArray<IAspectInstance>.Empty;

        public void Skip() { this.IsSkipped = true; }

        public ImmutableDictionary<TemplateClass, TemplateClassInstance> TemplateInstances { get; }

        public ImmutableArray<AspectPredecessor> Predecessors { get; }

        public IAspectState? AspectState { get; set; }

        void IAspectInstanceInternal.SetState( IAspectState? value ) => this.AspectState = value;

        public int TargetDeclarationDepth { get; }

        internal AspectInstance( IAspect aspect, IDeclaration declaration, AspectClass aspectClass, in AspectPredecessor predecessor ) :
            this( aspect, declaration.ToTypedRef(), declaration.Depth, aspectClass, predecessor ) { }

        private AspectInstance(
            IAspect aspect,
            in Ref<IDeclaration> declaration,
            int declarationDepth,
            AspectClass aspectClass,
            in AspectPredecessor predecessor ) : this(
            aspect,
            declaration,
            declarationDepth,
            aspectClass,
            ImmutableArray.Create( predecessor ) ) { }

        internal AspectInstance( IAspect aspect, IDeclaration declaration, AspectClass aspectClass, ImmutableArray<AspectPredecessor> predecessors ) : this(
            aspect,
            declaration.ToTypedRef(),
            declaration.Depth,
            aspectClass,
            predecessors ) { }

        internal AspectInstance(
            IAspect aspect,
            in Ref<IDeclaration> declaration,
            int declarationDepth,
            AspectClass aspectClass,
            ImmutableArray<AspectPredecessor> predecessors )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
            this.Predecessors = predecessors;
            this.TargetDeclarationDepth = declarationDepth;

            this.TemplateInstances = ImmutableDictionary.Create<TemplateClass, TemplateClassInstance>()
                .Add( aspectClass, new TemplateClassInstance( aspect, aspectClass ) );

#if DEBUG
            if ( !predecessors.IsDefaultOrEmpty )
            {
                foreach ( var predecessor in predecessors )
                {
                    predecessor.Instance.AssertNotNull();
                }
            }
#endif
        }

        internal AspectInstance(
            IAspect aspect,
            IDeclaration declaration,
            IAspectClassImpl aspectClass,
            IEnumerable<TemplateClassInstance> templateInstances,
            ImmutableArray<AspectPredecessor> predecessors )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = declaration.ToTypedRef();
            this.AspectClass = aspectClass;
            this.Predecessors = predecessors;
            this.TargetDeclarationDepth = declaration.GetCompilationModel().GetDepth( declaration );

            this.TemplateInstances = templateInstances.ToImmutableDictionary( t => t.TemplateClass, t => t );
        }

        public EligibleScenarios ComputeEligibility( IDeclaration declaration )
        {
            var eligibility = this.AspectClass.GetEligibility( declaration, this.IsInheritable );

            if ( (eligibility & EligibleScenarios.Inheritance) != 0 && !((IDeclarationImpl) declaration).CanBeInherited )
            {
                eligibility &= ~EligibleScenarios.Inheritance;
            }

            return eligibility;
        }

        public override string ToString() => this.AspectClass.ShortName + "@" + this.TargetDeclaration;

        public FormattableString FormatPredecessor( ICompilation compilation )
            => $"aspect '{this.AspectClass.ShortName}' applied to '{this.TargetDeclaration.GetTarget( compilation )}'";

        public Location? GetDiagnosticLocation( Compilation compilation )
            => compilation.GetTypeByMetadataName( this.AspectClass.FullName )?.GetDiagnosticLocation();

        public int CompareTo( AspectInstance? other )
        {
            if ( other == null )
            {
                return 1;
            }

            // Compare by degree of predecessor. Shorter causality chains take precedence.
            var degreeComparison = this.PredecessorDegree.CompareTo( other.PredecessorDegree );

            if ( degreeComparison != 0 )
            {
                return degreeComparison;
            }

            // Compare by declaration depth of the root attribute or fabric. Higher depths takes precedence.
            int GetMaxRootDepth( IAspectPredecessor aspectInstance )
                => aspectInstance.GetRoots()
                    .Max( p => ((IAspectPredecessorImpl) p).TargetDeclarationDepth );

            var depthComparison = GetMaxRootDepth( this ).CompareTo( GetMaxRootDepth( other ) );

            if ( depthComparison != 0 )
            {
                return -1 * depthComparison;
            }

            // Order ChildAspect before RequireAspect.
            int GetKindOrder2( AspectInstance aspectInstance )
                => aspectInstance.Predecessors.IsDefaultOrEmpty
                    ? -1
                    : aspectInstance.Predecessors.Min(
                        x => x.Kind switch
                        {
                            AspectPredecessorKind.Attribute => 0,
                            AspectPredecessorKind.ChildAspect => 1,
                            AspectPredecessorKind.RequiredAspect => 2,
                            AspectPredecessorKind.Inherited => 3,
                            AspectPredecessorKind.Fabric => 4,
                            _ => throw new AssertionFailedException( $"Unexpected value: {x.Kind}" )
                        } );

            var predecessorKindComparison2 = GetKindOrder2( this ).CompareTo( GetKindOrder2( other ) );

            if ( predecessorKindComparison2 != 0 )
            {
                return predecessorKindComparison2;
            }

            // At this point, ordering is no longer deterministic. If the aspect needs better ordering, it must implement it by itself.

            return 0;
        }

        public AspectInstance CreateDerivedInstance( IDeclaration target )
            => new(
                this.Aspect,
                target,
                (AspectClass) this.AspectClass,
                new AspectPredecessor( AspectPredecessorKind.Inherited, this ) );

        public int PredecessorDegree => this.Predecessors.IsDefaultOrEmpty ? 0 : this.Predecessors.Min( p => p.Instance.PredecessorDegree ) + 1;
    }
}