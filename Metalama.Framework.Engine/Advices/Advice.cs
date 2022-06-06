// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;

namespace Metalama.Framework.Engine.Advices
{
    internal abstract class Advice
    {
        public IAspectInstanceInternal Aspect { get; }

        public TemplateClassInstance TemplateInstance { get; }

        public Ref<IDeclaration> TargetDeclaration { get; }

        public AspectLayerId AspectLayerId { get; }

        public int Order { get; set; }

        /// <summary>
        /// Gets the compilation from which the advice was instantiated.
        /// </summary>
        public ICompilation SourceCompilation { get; }

        protected Advice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance template,
            IDeclaration targetDeclaration,
            string? layerName )
        {
            this.Aspect = aspect;
            this.TemplateInstance = template;
            this.TargetDeclaration = targetDeclaration.AssertNotNull().ToTypedRef();
            this.SourceCompilation = targetDeclaration.Compilation;
            this.AspectLayerId = new AspectLayerId( this.Aspect.AspectClass, layerName );
        }

        /// <summary>
        /// Initializes the advice. Executed before any advices are executed.
        /// </summary>
        /// <param name="diagnosticAdder">Diagnostic adder.</param>
        /// <remarks>
        /// The advice should only report diagnostics that do not take into account the target declaration(s).
        /// </remarks>
        public abstract void Initialize( IDiagnosticAdder diagnosticAdder );

        /// <summary>
        /// Applies the advice on the given compilation and returns the set of resulting transformations and diagnostics.
        /// </summary>
        /// <param name="compilation">Input compilation.</param>
        /// <returns>Advice result containing transformations and diagnostics.</returns>
        public abstract AdviceResult ToResult( ICompilation compilation );
    }
}