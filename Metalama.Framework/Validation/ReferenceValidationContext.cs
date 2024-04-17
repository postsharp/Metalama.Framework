// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly IDeclaration _referencedDeclaration;
        private readonly IDeclaration _referencingDeclaration;

        internal abstract ReferenceGranularity OutboundGranularity { get; }

        public abstract IEnumerable<ReferenceInstance> References { get; }

        internal abstract IDiagnosticSource DiagnosticSource { get; }

        internal ICompilation Compilation => this._referencedDeclaration.Compilation;

        /// <summary>
        /// Gets the optional opaque object defined by the aspect for the specific target declaration using the <see cref="IAspectBuilder.AspectState"/>
        /// property of the <see cref="IAspectBuilder"/> interface. 
        /// </summary>
        public IAspectState? AspectState { get; }

        public ReferenceEnd Referenced => new( this._referencedDeclaration, ReferenceGranularity.ParameterOrAttribute );

        public ReferenceEnd Referencing => new( this._referencingDeclaration, this.OutboundGranularity );

        [Obsolete( "Use the Referenced property and consider which ReferenceEnd property to get according to the granularity of the validator." )]
        public IDeclaration ReferencedDeclaration => this.Referenced.ParameterOrAttribute;

        [Obsolete( "Use the Referenced property and consider which ReferenceEnd property to get according to the granularity of the validator." )]
        public INamedType ReferencedType => this.Referenced.Type;

        [Obsolete( "Use the Referencing property and consider which ReferenceEnd property to get according to the granularity of the validator." )]
        public IDeclaration ReferencingDeclaration => this.Referencing.ParameterOrAttribute;

        [Obsolete( "Use the Referencing property and consider which ReferenceEnd property to get according to the granularity of the validator." )]
        public INamedType ReferencingType => this.Referencing.Type;

        [Obsolete( "Use References to get all references, then ReferenceInstance.ReferenceKinds." )]
        public abstract ReferenceKinds ReferenceKinds { get; }

        [Obsolete( "Use References to get all references, then ReferenceInstance.DiagnosticLocation." )]
        public IDiagnosticLocation DiagnosticLocation => this.Source.DiagnosticLocation;

        [Obsolete( "Use References to get all references, then ReferenceInstance.Source." )]
        public SourceReference Source => this.References.Single().Source;

        [Obsolete( "Use References to get all references, then ReferenceInstance.Source." )]
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
            this._referencedDeclaration = referencedDeclaration;
            this._referencingDeclaration = referencingDeclaration;
        }

        public abstract IDeclaration ResolveDeclaration( ReferenceInstance referenceInstance );

        public abstract IDiagnosticLocation? ResolveLocation( ReferenceInstance referenceInstance );

        internal ReferenceGranularity GetGranularity( ReferenceDirection direction )
        {
            throw new NotImplementedException();
        }
    }
}