// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Caravela.Framework.Impl.Aspects
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

        public IAspectClass AspectClass { get; }

        public IDeclaration TargetDeclaration { get; }

        public bool IsSkipped { get; private set; }

        public ImmutableArray<IAspectInstance> SecondaryInstances => ImmutableArray<IAspectInstance>.Empty;

        public void Skip() { this.IsSkipped = true; }

        public ImmutableDictionary<TemplateClass, TemplateClassInstance> TemplateInstances { get; }

        public AspectPredecessor Predecessor { get; }

        public EligibleScenarios Eligibility { get; }

        ImmutableArray<AspectPredecessor> IAspectInstance.Predecessors => ImmutableArray.Create( this.Predecessor );

        internal AspectInstance( IAspect aspect, IDeclaration declaration, AspectClass aspectClass, in AspectPredecessor predecessor )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
            this.Predecessor = predecessor;
            this.Eligibility = ComputeEligibility( aspectClass, declaration );

            this.TemplateInstances = ImmutableDictionary.Create<TemplateClass, TemplateClassInstance>()
                .Add( aspectClass, new TemplateClassInstance( aspect, aspectClass ) );
        }

        private static EligibleScenarios ComputeEligibility( IAspectClassImpl aspectClass, IDeclaration declaration )
        {
            var eligibility = aspectClass.GetEligibility( declaration );

            if ( (eligibility & EligibleScenarios.Inheritance) != 0 && !((IDeclarationImpl) declaration).CanBeInherited )
            {
                eligibility &= ~EligibleScenarios.Inheritance;
            }

            return eligibility;
        }

        internal AspectInstance(
            IAspect aspect,
            IDeclaration declaration,
            IAspectClass aspectClass,
            IEnumerable<TemplateClassInstance> templateInstances,
            AspectPredecessor predecessor )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
            this.Predecessor = predecessor;
            this.Eligibility = ComputeEligibility( (IAspectClassImpl) aspectClass, declaration );

            this.TemplateInstances = templateInstances.ToImmutableDictionary( t => t.TemplateClass, t => t );
        }

        internal AspectInstance(
            IServiceProvider serviceProvider,
            Expression<Func<IAspect>> aspectExpression,
            IDeclaration declaration,
            AspectClass aspectClass,
            AspectPredecessor predecessor )
        {
            var userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();

            var aspectFunc = aspectExpression.Compile();
            this.Aspect = userCodeInvoker.Invoke( () => aspectFunc() );
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
            this.Predecessor = predecessor;
            this.Eligibility = ComputeEligibility( aspectClass, declaration );

            this.TemplateInstances = ImmutableDictionary.Create<TemplateClass, TemplateClassInstance>()
                .Add( aspectClass, new TemplateClassInstance( this.Aspect, aspectClass ) );
        }

        public override string ToString() => this.AspectClass.ShortName + "@" + this.TargetDeclaration;

        public FormattableString FormatPredecessor() => $"aspect '{this.AspectClass.ShortName}' applied to '{this.TargetDeclaration}'";

        public Location? GetDiagnosticLocation( Compilation compilation )
            => compilation.GetTypeByMetadataName( this.AspectClass.FullName )?.GetDiagnosticLocation();

        public int CompareTo( AspectInstance? other )
        {
            if ( other == null )
            {
                return 1;
            }
            
            var predecessorKindComparison = this.Predecessor.Kind.CompareTo( other.AssertNotNull().Predecessor.Kind );

            if ( predecessorKindComparison != 0 )
            {
                return predecessorKindComparison;
            }

            // TODO: implement ordering within individual categories.

            return 0;
        }

        public virtual AttributeAspectInstance CreateDerivedInstance( IDeclaration target )
        {
            // Inherited aspects should not be created with a method that accepts an IAspect, but should provide a way to replicate the aspect.
            throw new AssertionFailedException();
        }
    }
}