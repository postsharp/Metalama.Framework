// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// The context object passed to the single parameter of validators added using <see cref="IValidatorReceiver.Validate"/>.
    /// </summary>
    /// <seealso href="@validation"/>
    [CompileTime]
    [PublicAPI]
    public readonly struct DeclarationValidationContext
    {
        private readonly IDiagnosticSink _diagnosticSink;
        private readonly IDiagnosticSource _diagnosticSource;

        internal object? Tag { get; }

        /// <summary>
        /// Gets the optional opaque object defined by the aspect for the specific target declaration using the <see cref="IAspectBuilder.AspectState"/>
        /// property of the <see cref="IAspectBuilder"/> interface. 
        /// </summary>
        public IAspectState? AspectState { get; }

        /// <summary>
        /// Gets a service that allows to report or suppress diagnostics.
        /// </summary>
        public ScopedDiagnosticSink Diagnostics => new( this._diagnosticSink, this._diagnosticSource, this.Declaration, this.Declaration );

        /// <summary>
        /// Gets the declaration that should be validated.
        /// </summary>
        public IDeclaration Declaration { get; }

        internal DeclarationValidationContext(
            IDeclaration declaration,
            IAspectState? aspectState,
            IDiagnosticSink diagnosticSink,
            IDiagnosticSource diagnosticSource,
            object? tag )
        {
            this.AspectState = aspectState;
            this._diagnosticSink = diagnosticSink;
            this._diagnosticSource = diagnosticSource;
            this.Tag = tag;
            this.Declaration = declaration;
        }
    }
}