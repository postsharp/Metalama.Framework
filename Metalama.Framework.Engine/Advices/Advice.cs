// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advices
{
    internal abstract class Advice
    {
        public IAspectInstanceInternal Aspect { get; }

        public TemplateClassInstance TemplateInstance { get; }

        public Ref<IDeclaration> TargetDeclaration { get; }

        public AspectLayerId AspectLayerId { get; }

        /// <summary>
        /// Gets the compilation from which the advice was instantiated.
        /// </summary>
        public ICompilation SourceCompilation { get; }

        protected Advice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance template,
            IDeclaration targetDeclaration,
            ICompilation sourceCompilation,
            string? layerName )
        {
            this.Aspect = aspect;
            this.TemplateInstance = template;
            this.TargetDeclaration = targetDeclaration.AssertNotNull().ToTypedRef();
            this.SourceCompilation = sourceCompilation;
            this.AspectLayerId = new AspectLayerId( this.Aspect.AspectClass, layerName );
        }

        /// <summary>
        /// Initializes the advice. Executed before any advices are executed.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="diagnosticAdder">Diagnostic adder.</param>
        /// <remarks>
        /// The advice should only report diagnostics that do not take into account the target declaration(s).
        /// </remarks>
        public virtual void Initialize( IServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder ) { }

        /// <summary>
        /// Applies the advice on the given compilation and returns the set of resulting transformations and diagnostics.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="compilation">Input compilation.</param>
        /// <param name="addTransformation"></param>
        /// <returns>Advice result containing transformations and diagnostics.</returns>
        public abstract AdviceImplementationResult Implement(
            IServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation );
    }
}