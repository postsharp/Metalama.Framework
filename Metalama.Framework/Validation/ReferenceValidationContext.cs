// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using System;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// The context object passed to the single parameter of validators added using <see cref="IDeclarationSelection{TDeclaration}.RegisterReferenceValidator"/>.
    /// </summary>
    /// <seealso href="@validation"/>
    [CompileTime]
    public readonly struct ReferenceValidationContext
    {
        private readonly IDiagnosticSink _diagnostics;

        /// <summary>
        /// Gets the optional opaque object defined by the aspect for the specific target declaration using the <see cref="IAspectBuilder.State"/>
        /// property of the <see cref="IAspectBuilder"/> interface. 
        /// </summary>
        public IAspectState? AspectState { get; }

        /// <summary>
        /// Gets the declaration being validated (i.e. the one referenced by the <see cref="Syntax"/> being inspected).
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
            => this.ReferencingDeclaration.GetDeclaringType()
               ?? throw new InvalidOperationException( $"Don't know how to get the declaring type of '{this.ReferencingDeclaration}'." );

        /// <summary>
        /// Gets the set (bit mask) of reference kinds for the current <see cref="Syntax"/>. For instance, while validating a parameter of type <c>Foo[]</c>,
        /// both bits <see cref="Validation.ReferenceKinds.ParameterType"/> and <see cref="Validation.ReferenceKinds.ArrayType"/> will be set.
        /// </summary>
        public ReferenceKinds ReferenceKinds { get; }

        /// <summary>
        /// Gets the location on which the diagnostic should be reported.
        /// </summary>
        public IDiagnosticLocation DiagnosticLocation => this.Syntax.DiagnosticLocation;

        /// <summary>
        /// Gets the syntax node that represents the reference.
        /// </summary>
        public SyntaxReference Syntax { get; }

        public ScopedDiagnosticSink Diagnostics => new( this._diagnostics, this.DiagnosticLocation, this.ReferencedDeclaration );

        internal ReferenceValidationContext(
            IDeclaration referencedDeclaration,
            IDeclaration referencingDeclaration,
            in SyntaxReference syntax,
            IAspectState? aspectState,
            IDiagnosticSink diagnostics,
            ReferenceKinds referenceKinds )
        {
            this.AspectState = aspectState;
            this._diagnostics = diagnostics;
            this.ReferencedDeclaration = referencedDeclaration;
            this.ReferencingDeclaration = referencingDeclaration;
            this.ReferenceKinds = referenceKinds;
            this.Syntax = syntax;
        }
    }
}