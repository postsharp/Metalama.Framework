// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdviceDeclaration<T> : IDeclaration
        where T : IDeclaration
    {
        public AdviceDeclaration( T underlying )
        {
            this.Underlying = underlying;
        }

        protected T Underlying { get; }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Underlying.ToDisplayString( format, context );

        public IDiagnosticLocation? DiagnosticLocation => this.Underlying.DiagnosticLocation;

        public ICompilation Compilation => this.Underlying.Compilation;

        public DeclarationOrigin Origin => this.Underlying.Origin;

        public IDeclaration? ContainingDeclaration => this.Underlying.ContainingDeclaration;

        public IAttributeList Attributes => this.Underlying.Attributes;

        public DeclarationKind DeclarationKind => this.Underlying.DeclarationKind;

        public bool HasAspect<T1>() where T1 : IAspect => this.Underlying.HasAspect<T1>();

        public IAnnotationList GetAnnotations<T1>() where T1 : IAspect => this.Underlying.GetAnnotations<T1>();
    }
}