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
        private ReferenceEnd _referencedEnd;
        private ReferenceEnd _referencingEnd;

        /// <summary>
        /// Gets the list of individual references that are being collectively analyzed and grouped by granularity.
        /// </summary>
        public abstract IEnumerable<ReferenceInstance> References { get; }

        internal abstract IDiagnosticSource DiagnosticSource { get; }

        internal ICompilation Compilation => this._referencedEnd.Declaration.Compilation;

        /// <summary>
        /// Gets the optional opaque object defined by the aspect for the specific target declaration using the <see cref="IAspectBuilder.AspectState"/>
        /// property of the <see cref="IAspectBuilder"/> interface. 
        /// </summary>
        public IAspectState? AspectState { get; }

        /// <summary>
        /// Gets information about the referenced declaration.
        /// </summary>
        public ref ReferenceEnd Referenced => ref this._referencedEnd;

        /// <summary>
        /// Gets information about the referencing declaration, i.e. the declaration containing the reference.
        /// </summary>
        public ref ReferenceEnd Referencing => ref this._referencingEnd;

        /// <summary>
        /// Gets the <see cref="ReferenceEnd"/> according to a <see cref="ReferenceDirection"/>.
        /// </summary>
        public ref ReferenceEnd GetReferenceEnd( ReferenceDirection direction )
        {
            if ( direction == ReferenceDirection.Outbound )
            {
                return ref this._referencingEnd;
            }
            else
            {
                return ref this._referencedEnd;
            }
        }

        [Obsolete( "Use the Referenced property and consider which ReferenceEnd property to get according to the granularity of the validator." )]
        public IDeclaration ReferencedDeclaration => this.Referenced.Declaration;

        [Obsolete( "Use the Referenced property and consider which ReferenceEnd property to get according to the granularity of the validator." )]
        public INamedType ReferencedType => this.Referenced.Type;

        [Obsolete( "Use the Referencing property and consider which ReferenceEnd property to get according to the granularity of the validator." )]
        public IDeclaration ReferencingDeclaration => this.Referencing.Declaration;

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

        /// <summary>
        /// Gets an object that allows to report diagnostics to all reference instances at one.
        /// </summary>
        public ReferenceValidationDiagnosticSink Diagnostics => new( this._diagnosticSink, this );

        internal abstract ISourceReferenceImpl SourceReferenceImpl { get; }

        internal ReferenceValidationContext(
            IDeclaration referencedDeclaration,
            IDeclaration referencingDeclaration,
            ReferenceGranularity outboundGranularity,
            IAspectState? aspectState,
            IDiagnosticSink diagnosticSink )
        {
            this.AspectState = aspectState;
            this._diagnosticSink = diagnosticSink;
            this._referencedEnd = new ReferenceEnd( referencedDeclaration, GetInboundGranularity( referencedDeclaration.DeclarationKind ) );
            this._referencingEnd = new ReferenceEnd( referencingDeclaration, outboundGranularity );
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

        internal abstract IDeclaration ResolveDeclaration( ReferenceInstance referenceInstance );

        internal abstract IDiagnosticLocation? ResolveLocation( ReferenceInstance referenceInstance );
    }
}