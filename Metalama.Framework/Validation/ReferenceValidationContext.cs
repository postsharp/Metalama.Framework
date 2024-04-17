// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using System;
using System.Collections.Generic;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// The context object passed to the single parameter of validators added using <see cref="IValidatorReceiver{TDeclaration}.ValidateReferences{TValidator}"/>.
    /// </summary>
    /// <seealso href="@validation"/>
    [CompileTime]
    [PublicAPI]
    public abstract class ReferenceValidationContext
    {
        private readonly IDiagnosticSink _diagnosticSink;

        public abstract IEnumerable<ReferenceInstance> References { get; }

        internal abstract IDiagnosticSource DiagnosticSource { get; }

        /// <summary>
        /// Gets the optional opaque object defined by the aspect for the specific target declaration using the <see cref="IAspectBuilder.AspectState"/>
        /// property of the <see cref="IAspectBuilder"/> interface. 
        /// </summary>
        public IAspectState? AspectState { get; }

        /// <summary>
        /// Gets the declaration at the outbound end of the reference.
        /// </summary>
        public IDeclaration ReferencedDeclaration { get; }

        /// <summary>
        /// Gets the type containing the  declaration at the outbound end of the reference.
        /// </summary>
        public INamedType ReferencedType
            => this.ReferencedDeclaration.GetClosestNamedType()
               ?? throw new InvalidOperationException( $"Cannot get the declaring type of '{this.ReferencedDeclaration}'." );

        /// <summary>
        /// Gets the declaration containing the inbound end of the reference.
        /// </summary>
        public IDeclaration ReferencingDeclaration { get; }

        /// <summary>
        /// Gets the type containing the inbound end of the reference.
        /// </summary>
        public INamedType ReferencingType
            => this.ReferencingDeclaration.GetClosestNamedType()
               ?? throw new InvalidOperationException( $"Cannot get the declaring type of '{this.ReferencingDeclaration}'." );

        /// <summary>
        /// Gets the set (bit mask) of reference kinds for the current <see cref="Source"/>. For instance, while validating a parameter of type <c>Foo[]</c>,
        /// both bits <see cref="Validation.ReferenceKinds.ParameterType"/> and <see cref="Validation.ReferenceKinds.ArrayType"/> will be set.
        /// </summary>
        [Obsolete]
        public abstract ReferenceKinds ReferenceKinds { get; }

        /// <summary>
        /// Gets the location on which the diagnostic should be reported.
        /// </summary>
        [Obsolete]
        public IDiagnosticLocation DiagnosticLocation => this.Source.DiagnosticLocation;

        /// <summary>
        /// Gets the syntax node that represents the reference.
        /// </summary>
        [Obsolete]
        public SourceReference Source { get; }

        [Obsolete( "Use the Source property." )]
        public SourceReference Syntax => this.Source;

        public ReferenceValidationDiagnosticSink Diagnostics => new( this._diagnosticSink, this );

        internal abstract ISourceReferenceImpl SourceReferenceImpl { get; }

        internal ReferenceValidationContext(
            IDeclaration referencedDeclaration,
            IDeclaration referencingDeclaration,
            IAspectState? aspectState,
            IDiagnosticSink diagnosticSink )
        {
            this.AspectState = aspectState;
            this._diagnosticSink = diagnosticSink;
            this.ReferencedDeclaration = referencedDeclaration;
            this.ReferencingDeclaration = referencingDeclaration;
        }

        public abstract IDeclaration ResolveDeclaration( ReferenceInstance referenceInstance );

        public abstract IDiagnosticLocation? ResolveLocation( ReferenceInstance referenceInstance );
    }
}