// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Represents an instance of an aspect and its target declaration.
    /// </summary>
    internal class AspectInstance : IAspectInstanceInternal, IComparable<AspectInstance>
    {
        /// <summary>
        /// Gets the aspect instance.
        /// </summary>
        public IAspect Aspect { get; }

        IAspectClass IAspectInstance.AspectClass => this.AspectClass;

        public IAspectClassImpl AspectClass { get; }

        IRef<IDeclaration> IAspectInstance.TargetDeclaration => this.TargetDeclaration;

        public Ref<IDeclaration> TargetDeclaration { get; }

        public bool IsSkipped { get; private set; }

        public ImmutableArray<IAspectInstance> SecondaryInstances => ImmutableArray<IAspectInstance>.Empty;

        public void Skip() { this.IsSkipped = true; }

        public ImmutableDictionary<TemplateClass, TemplateClassInstance> TemplateInstances { get; }

        public ImmutableArray<AspectPredecessor> Predecessors { get; }

        private AspectPredecessor FirstPredecessor => this.Predecessors[0];

        public IAspectState? State { get; set; }

        void IAspectInstanceInternal.SetState( IAspectState? value ) => this.State = value;

        internal AspectInstance( IAspect aspect, in Ref<IDeclaration> declaration, AspectClass aspectClass, in AspectPredecessor predecessor ) : this(
            aspect,
            declaration,
            aspectClass,
            ImmutableArray.Create( predecessor ) ) { }

        internal AspectInstance( IAspect aspect, in Ref<IDeclaration> declaration, AspectClass aspectClass, ImmutableArray<AspectPredecessor> predecessors )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
            this.Predecessors = predecessors;

            this.TemplateInstances = ImmutableDictionary.Create<TemplateClass, TemplateClassInstance>()
                .Add( aspectClass, new TemplateClassInstance( aspect, aspectClass ) );
        }

        internal AspectInstance(
            IAspect aspect,
            in Ref<IDeclaration> declaration,
            IAspectClassImpl aspectClass,
            IEnumerable<TemplateClassInstance> templateInstances,
            ImmutableArray<AspectPredecessor> predecessors )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
            this.Predecessors = predecessors;

            this.TemplateInstances = templateInstances.ToImmutableDictionary( t => t.TemplateClass, t => t );
        }

        public static bool TryCreateInstance(
            IServiceProvider serviceProvider,
            IDiagnosticAdder diagnosticAdder,
            Expression<Func<IAspect>> aspectExpression,
            in Ref<IDeclaration> declaration,
            AspectClass aspectClass,
            in AspectPredecessor predecessor,
            [NotNullWhen( true )] out AspectInstance? aspectInstance )
        {
            var userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
            var aspectFunc = aspectExpression.Compile();

            var executionContext = new UserCodeExecutionContext( serviceProvider, diagnosticAdder, UserCodeMemberInfo.FromExpression( aspectExpression ) );

            if ( !userCodeInvoker.TryInvoke( () => aspectFunc(), executionContext, out var aspect ) )
            {
                aspectInstance = null;

                return false;
            }

            aspectInstance = new AspectInstance( aspect!, in declaration, aspectClass, in predecessor );

            return true;
        }

        public EligibleScenarios ComputeEligibility( IDeclaration declaration )
        {
            var eligibility = this.AspectClass.GetEligibility( declaration );

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

            var predecessorKindComparison = this.FirstPredecessor.Kind.CompareTo( other.AssertNotNull().FirstPredecessor.Kind );

            if ( predecessorKindComparison != 0 )
            {
                return predecessorKindComparison;
            }

            // TODO: implement ordering within individual categories.

            return 0;
        }

        public AspectInstance CreateDerivedInstance( IDeclaration target )
            => new(
                this.Aspect,
                ((IDeclarationImpl) target).ToRef(),
                (AspectClass) this.AspectClass,
                new AspectPredecessor( AspectPredecessorKind.Inherited, this ) );
    }
}