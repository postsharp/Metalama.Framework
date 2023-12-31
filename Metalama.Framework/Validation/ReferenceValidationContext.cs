// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using System;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// The context object passed to the single parameter of validators added using <see cref="IValidatorReceiver{TDeclaration}.ValidateReferences{TValidator}"/>.
    /// </summary>
    /// <seealso href="@validation"/>
    [CompileTime]
    [PublicAPI]
    public readonly struct ReferenceValidationContext
    {
        private readonly IDiagnosticSink _diagnosticSink;

        internal IDiagnosticSource DiagnosticSource { get; }

        /// <summary>
        /// Gets the optional opaque object defined by the aspect for the specific target declaration using the <see cref="IAspectBuilder.AspectState"/>
        /// property of the <see cref="IAspectBuilder"/> interface. 
        /// </summary>
        public IAspectState? AspectState { get; }

        /// <summary>
        /// Gets the declaration being validated (i.e. the one referenced by the <see cref="Source"/> being inspected).
        /// </summary>
        public IDeclaration ReferencedDeclaration { get; }

        /// <summary>
        /// Gets the declaration containing the reference.
        /// </summary>
        public IDeclaration ReferencingDeclaration { get; }

        /// <summary>
        /// Gets the type containing the reference.
        /// </summary>
        public INamedType ReferencingType
            => this.ReferencingDeclaration.GetClosestNamedType()
               ?? throw new InvalidOperationException( $"Don't know how to get the declaring type of '{this.ReferencingDeclaration}'." );

        /// <summary>
        /// Gets the set (bit mask) of reference kinds for the current <see cref="Source"/>. For instance, while validating a parameter of type <c>Foo[]</c>,
        /// both bits <see cref="Validation.ReferenceKinds.ParameterType"/> and <see cref="Validation.ReferenceKinds.ArrayType"/> will be set.
        /// </summary>
        public ReferenceKinds ReferenceKinds { get; }

        /// <summary>
        /// Gets the location on which the diagnostic should be reported.
        /// </summary>
        public IDiagnosticLocation DiagnosticLocation => this.Source.DiagnosticLocation;

        /// <summary>
        /// Gets the syntax node that represents the reference.
        /// </summary>
        public SourceReference Source { get; }

        [Obsolete( "Use the Source property." )]
        public SourceReference Syntax => this.Source;

        public ScopedDiagnosticSink Diagnostics => new( this._diagnosticSink, this.DiagnosticSource, this.DiagnosticLocation, this.ReferencedDeclaration );

        internal ReferenceValidationContext(
            IDeclaration referencedDeclaration,
            IDeclaration referencingDeclaration,
            in SourceReference source,
            IAspectState? aspectState,
            IDiagnosticSink diagnosticSink,
            IDiagnosticSource diagnosticSource,
            ReferenceKinds referenceKinds )
        {
            this.AspectState = aspectState;
            this._diagnosticSink = diagnosticSink;
            this.DiagnosticSource = diagnosticSource;
            this.ReferencedDeclaration = referencedDeclaration;
            this.ReferencingDeclaration = referencingDeclaration;
            this.ReferenceKinds = referenceKinds;
            this.Source = source;
        }
    }
}