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
        private readonly IDeclaration _referencedDeclaration;
        private readonly IDeclaration _referencingDeclaration;
        private readonly ReferenceGranularity _originGranularity;
        private readonly IDiagnosticSink _diagnosticSink;
        private ReferenceEnd? _destinationEnd;
        private ReferenceEnd? _originEnd;

        /// <summary>
        /// Gets the list of individual references that are being collectively analyzed and grouped by granularity.
        /// </summary>
        public abstract IEnumerable<ReferenceDetail> Details { get; }

        internal abstract IDiagnosticSource DiagnosticSource { get; }

        internal ICompilation Compilation => this._referencedDeclaration.Compilation;

        /// <summary>
        /// Gets the optional opaque object defined by the aspect for the specific target declaration using the <see cref="IAspectBuilder.AspectState"/>
        /// property of the <see cref="IAspectBuilder"/> interface. 
        /// </summary>
        public IAspectState? AspectState { get; }

        /// <summary>
        /// Gets information about the referenced declaration.
        /// </summary>
        public ReferenceEnd Destination
            => this._destinationEnd ??= new ReferenceEnd( this._referencedDeclaration, GetInboundGranularity( this._referencedDeclaration.DeclarationKind ) );

        /// <summary>
        /// Gets information about the referencing declaration, i.e. the declaration containing the reference.
        /// </summary>
        public ReferenceEnd Origin => this._originEnd ??= new ReferenceEnd( this._referencingDeclaration, this._originGranularity );

        /// <summary>
        /// Gets the <see cref="ReferenceEnd"/> according to a <see cref="ReferenceEndRole"/>.
        /// </summary>
        public ReferenceEnd GetReferenceEnd( ReferenceEndRole role )
        {
            if ( role == ReferenceEndRole.Origin )
            {
                return this.Origin;
            }
            else
            {
                return this.Destination;
            }
        }

        [Obsolete( "Use the Destination property and consider which ReferenceEnd property to get according to the granularity of the validator." )]
        public IDeclaration ReferencedDeclaration => this.Destination.Declaration;

        [Obsolete( "Use the Destination property and consider which ReferenceEnd property to get according to the granularity of the validator." )]
        public INamedType ReferencedType => this.Destination.Type;

        [Obsolete( "Use the Origin property and consider which ReferenceEnd property to get according to the granularity of the validator." )]
        public IDeclaration ReferencingDeclaration => this.Origin.Declaration;

        [Obsolete( "Use the Origin property and consider which ReferenceEnd property to get according to the granularity of the validator." )]
        public INamedType ReferencingType => this.Origin.Type;

        [Obsolete( "Use Details to get all references, then ReferenceInstance.ReferenceKind." )]
        public abstract ReferenceKinds ReferenceKinds { get; }

        [Obsolete( "Use Details to get all references. The SourceReference implements IDiagnosticLocation." )]
        public IDiagnosticLocation DiagnosticLocation => this.Source;

        [Obsolete( "Use Details to get all references, then ReferenceInstance.Source." )]
        public SourceReference Source => this.Details.Single().Source;

        [Obsolete( "Use Details to get all references, then ReferenceInstance.Source." )]
        public SourceReference Syntax => this.Source;

        /// <summary>
        /// Gets an object that allows to report diagnostics to all reference instances at one.
        /// </summary>
        public ReferenceValidationDiagnosticSink Diagnostics => new( this._diagnosticSink, this );

        internal abstract ISourceReferenceImpl SourceReferenceImpl { get; }

        internal ReferenceValidationContext(
            IDeclaration referencedDeclaration,
            IDeclaration referencingDeclaration,
            ReferenceGranularity originGranularity,
            IAspectState? aspectState,
            IDiagnosticSink diagnosticSink )
        {
            this.AspectState = aspectState;
            this._referencedDeclaration = referencedDeclaration;
            this._referencingDeclaration = referencingDeclaration;
            this._originGranularity = originGranularity;
            this._diagnosticSink = diagnosticSink;
        }

        private static ReferenceGranularity GetInboundGranularity( DeclarationKind kind )
            => kind switch
            {
                DeclarationKind.Constructor or DeclarationKind.Event or DeclarationKind.Method or DeclarationKind.Field or DeclarationKind.Finalizer
                    or DeclarationKind.Operator or DeclarationKind.Property or DeclarationKind.Indexer => ReferenceGranularity.Member,
                DeclarationKind.Compilation or DeclarationKind.AssemblyReference => ReferenceGranularity.Compilation,
                DeclarationKind.Namespace => ReferenceGranularity.Namespace,
                DeclarationKind.NamedType => ReferenceGranularity.Type,
                DeclarationKind.Parameter or DeclarationKind.TypeParameter or DeclarationKind.Attribute => ReferenceGranularity.ParameterOrAttribute,
                _ => throw new ArgumentOutOfRangeException( nameof(kind), $"Unexpected kind: '{kind}'" )
            };

        internal abstract IDeclaration ResolveOriginDeclaration( ReferenceDetail referenceDetail );

        internal abstract IDeclaration ResolveDestinationDeclaration( ReferenceDetail referenceDetail );

        internal abstract IDiagnosticLocation? ResolveDiagnosticLocation( ReferenceDetail referenceDetail );
    }
}