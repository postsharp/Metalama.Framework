// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// The context object passed to the single parameter of validators added using <see cref="IDeclarationSelection{TDeclaration}.RegisterDeclarationValidator{T}"/>.
    /// </summary>
    [CompileTimeOnly]
    public readonly struct DeclarationValidationContext
    {
        /// <summary>
        /// Gets the optional opaque object defined by the aspect for the specific target declaration using the <see cref="IAspectBuilder.State"/>
        /// property of the <see cref="IAspectBuilder"/> interface. 
        /// </summary>
        public IAspectState? AspectState { get; }

        /// <summary>
        /// Gets a service that allows to report or suppress diagnostics.
        /// </summary>
        public IDiagnosticSink Diagnostics { get; }

        /// <summary>
        /// Gets the declaration that should be validated.
        /// </summary>
        public IDeclaration Declaration { get; }

        internal DeclarationValidationContext( IDeclaration declaration, IAspectState? aspectState, IDiagnosticSink diagnostics )
        {
            this.AspectState = aspectState;
            this.Diagnostics = diagnostics;
            this.Declaration = declaration;
        }
    }
}