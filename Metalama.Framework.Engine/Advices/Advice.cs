﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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

        protected IObjectReader Tags { get; }

        public int Order { get; set; }

        /// <summary>
        /// Gets the compilation from which the advice was instantiated.
        /// </summary>
        public ICompilation SourceCompilation { get; }

        protected Advice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance template,
            IDeclaration targetDeclaration,
            string? layerName,
            IObjectReader tags )
        {
            this.Tags = tags;
            this.Aspect = aspect;
            this.TemplateInstance = template;
            this.TargetDeclaration = targetDeclaration.AssertNotNull().ToTypedRef();
            this.SourceCompilation = targetDeclaration.Compilation;
            this.AspectLayerId = new AspectLayerId( this.Aspect.AspectClass, layerName );
        }

        public abstract void Initialize( IDiagnosticAdder diagnosticAdder );

        public abstract AdviceResult ToResult( ICompilation compilation );
    }
}