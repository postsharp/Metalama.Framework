// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

        private IDeclaration GetWithGranularity( IDeclaration declaration, ReferenceGranularity granularity, [CallerMemberName] string? callingProperty = null )
        {
            if ( granularity > this.OutboundGranularity )
            {
                throw new InvalidOperationException(
                    $"Cannot get the {callingProperty} because the granularity of outbound references for this validator is set to {this.OutboundGranularity}" );
            }

            return granularity switch
            {
                ReferenceGranularity.Namespace => declaration as INamespace
                                                  ?? declaration.GetClosestNamedType()?.Namespace
                                                  ?? throw new InvalidOperationException(
                                                      $"Cannot get the namespace of '{declaration}' because it is a {declaration.DeclarationKind}." ),
                ReferenceGranularity.Type => declaration.GetClosestNamedType()
                                             ?? throw new InvalidOperationException(
                                                 $"Cannot get the declaring type of '{declaration}' because it is a {declaration.DeclarationKind}." ),
                ReferenceGranularity.Member => declaration.GetClosestMemberOrNamedType() as IMember
                                               ?? throw new InvalidOperationException(
                                                   $"Cannot get the member of '{declaration}' because it is a {declaration}." ),
                ReferenceGranularity.Declaration => declaration,
                _ => throw new ArgumentOutOfRangeException( nameof(granularity) )
            };
        }

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
        public IDeclaration ReferencedDeclaration => this.GetWithGranularity( this._referencedDeclaration, ReferenceGranularity.Declaration );

        /// <summary>
        /// Gets the type containing the  declaration at the outbound end of the reference.
        /// </summary>
        public INamedType ReferencedType => (INamedType) this.GetWithGranularity( this._referencedDeclaration, ReferenceGranularity.Type );

        public INamespace ReferencedNamespace => (INamespace) this.GetWithGranularity( this._referencedDeclaration, ReferenceGranularity.Namespace );

        public IMember ReferencedMember => (IMember) this.GetWithGranularity( this._referencedDeclaration, ReferenceGranularity.Member );

        public IAssembly ReferenceAssembly => this.ReferencedDeclaration.DeclaringAssembly;

        /// <summary>
        /// Gets the declaration containing the inbound end of the reference.
        /// </summary>
        public IDeclaration ReferencingDeclaration => this.GetWithGranularity( this._referencingDeclaration, ReferenceGranularity.Declaration );

        /// <summary>
        /// Gets the type containing the inbound end of the reference.
        /// </summary>
        public INamedType ReferencingType => (INamedType) this.GetWithGranularity( this._referencingDeclaration, ReferenceGranularity.Type );

        public INamespace ReferencingNamespace => (INamespace) this.GetWithGranularity( this._referencingDeclaration, ReferenceGranularity.Namespace );

        public IMember ReferencingMember => (IMember) this.GetWithGranularity( this._referencingDeclaration, ReferenceGranularity.Member );

        public IAssembly ReferencingAssembly => this.ReferencingDeclaration.DeclaringAssembly;

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
        public SourceReference Source => this.References.Single().Source;

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
            this._referencedDeclaration = referencedDeclaration;
            this._referencingDeclaration = referencingDeclaration;
        }

        public abstract IDeclaration ResolveDeclaration( ReferenceInstance referenceInstance );

        public abstract IDiagnosticLocation? ResolveLocation( ReferenceInstance referenceInstance );
    }
}