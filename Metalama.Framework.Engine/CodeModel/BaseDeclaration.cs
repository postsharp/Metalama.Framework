// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel
{
    public abstract class BaseDeclaration : IDeclarationImpl
    {
        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );

        public abstract CompilationModel Compilation { get; }

        ICompilation ICompilationElement.Compilation => this.Compilation;

        IRef<IDeclaration> IDeclaration.ToRef() => this.ToRef();

        public abstract ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

        public abstract bool CanBeInherited { get; }

        public abstract IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true );

        internal abstract Ref<IDeclaration> ToRef();

        Ref<IDeclaration> IDeclarationImpl.ToRef() => this.ToRef();

        public abstract IAssembly DeclaringAssembly { get; }

        public abstract IDeclarationOrigin Origin { get; }

        public abstract IDeclaration? ContainingDeclaration { get; }

        public abstract IAttributeCollection Attributes { get; }

        public abstract DeclarationKind DeclarationKind { get; }

        public abstract bool IsImplicitlyDeclared { get; }

        ISymbol? ISdkDeclaration.Symbol => this.GetSymbol();

        protected virtual ISymbol? GetSymbol() => null;

        public T GetMetric<T>()
            where T : IMetric
            => this.Compilation.MetricManager.GetMetric<T>( this );

        public abstract IDeclaration OriginalDefinition { get; }

        public abstract Location? DiagnosticLocation { get; }

        public abstract SyntaxTree? PrimarySyntaxTree { get; }

        /// <summary>
        /// This method is called from code model queries for which design-time cache invalidation is not implemented.
        /// </summary>
        protected static void OnUnsupportedDependency( string api )
        {
            UserCodeExecutionContext.CurrentInternal?.OnUnsupportedDependency( api );
        }
    }
}