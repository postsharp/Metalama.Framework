// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.AspectWeavers
{
    /// <summary>
    /// Context for the <see cref="IAspectWeaver"/>.
    /// </summary>
    [CompileTime]
    public sealed partial class AspectWeaverContext
    {
        private readonly Action<Diagnostic> _addDiagnostic;
        private IPartialCompilation _compilation;

        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the type of aspects that must be handled.
        /// </summary>
        public IAspectClass AspectClass { get; }

        /// <summary>
        /// Gets the set of aspect instances that must be weaved.
        /// </summary>
        public IReadOnlyDictionary<ISymbol, IAspectInstance> AspectInstances { get; }

        /// <summary>
        /// Gets the current project.
        /// </summary>
        public IProject Project { get; }

        /// <summary>
        /// Gets or sets the compilation.
        /// </summary>
        public IPartialCompilation Compilation
        {
            get => this._compilation;

            set
            {
                if ( ((IPartialCompilationInternal) value).InitialCompilation != ((IPartialCompilationInternal) this._compilation).InitialCompilation )
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        "The compilation must have been derived from the initial value of the Compilation property." );
                }

                this._compilation = value;
            }
        }

        public IAspectWeaverHelper Helper { get; }

        private CancellationToken GetCancellationToken( in CancellationToken cancellationToken )
            => cancellationToken == default ? this.CancellationToken : cancellationToken;

        /// <summary>
        /// Rewrites all syntax trees in the compilation.
        /// </summary>
        /// <param name="rewriter">A <see cref="CSharpSyntaxRewriter"/> called for each <see cref="SyntaxTree"/> in the compilation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        public async Task RewriteSyntaxTrees( CSharpSyntaxRewriter rewriter, CancellationToken cancellationToken = default )
            => this.Compilation = await this.Compilation.RewriteSyntaxTreesAsync(
                rewriter,
                this.ServiceProvider,
                this.GetCancellationToken( cancellationToken ) );

        /// <summary>
        /// Rewrites the syntax nodes targeted by aspects.
        /// </summary>
        /// <param name="rewriter">A <see cref="CSharpSyntaxRewriter"/> whose <c>Visit</c> method is invoked for all declarations
        /// that are the target of aspects handled by the current <see cref="IAspectWeaver"/> (see <see cref="AspectInstances"/>).
        /// In case of partial classes or methods, the <c>Visit</c> method is invoked for each partial declaration.
        /// </param>
        public async Task RewriteAspectTargetsAsync( CSharpSyntaxRewriter rewriter, CancellationToken cancellationToken = default )
        {
            cancellationToken = this.GetCancellationToken( cancellationToken );

            var taskScheduler = this.ServiceProvider.GetRequiredService<ITaskScheduler>();

            var nodesBySyntaxTree = this.AspectInstances.Values
                .Select( a => a.TargetDeclaration.GetSymbol( this._compilation.Compilation ) )
                .Where( s => s != null )
                .SelectMany( s => s!.DeclaringSyntaxReferences )
                .GroupBy( r => r.SyntaxTree );

            ConcurrentLinkedList<SyntaxTreeTransformation> modifiedSyntaxTrees = new();

            await taskScheduler.RunInParallelAsync( nodesBySyntaxTree, ProcessSyntaxTree, cancellationToken );

            void ProcessSyntaxTree( IGrouping<SyntaxTree, SyntaxReference> group )
            {
                cancellationToken.ThrowIfCancellationRequested();

                var oldTree = @group.Key;
                var outerRewriter = new Rewriter( group.Select( r => r.GetSyntax() ).ToImmutableHashSet(), rewriter );
                var oldRoot = oldTree.GetRoot();
                var newRoot = outerRewriter.Visit( oldRoot )!;

                if ( oldRoot != newRoot )
                {
                    modifiedSyntaxTrees.Add( SyntaxTreeTransformation.ReplaceTree( oldTree, oldTree.WithRootAndOptions( newRoot, oldTree.Options ) ) );
                }
            }

            this.Compilation = this.Compilation.WithSyntaxTreeTransformations( modifiedSyntaxTrees.ToList() );
        }

        internal AspectWeaverContext(
            IAspectClass aspectClass,
            IReadOnlyDictionary<ISymbol, IAspectInstance> aspectInstances,
            IPartialCompilation compilation,
            Action<Diagnostic> addDiagnostic,
            IAspectWeaverHelper helper,
            IServiceProvider serviceProvider,
            IProject project,
            SyntaxAnnotation generatedCodeAnnotation,
            CancellationToken cancellationToken )
        {
            this.AspectClass = aspectClass;
            this.AspectInstances = aspectInstances;
            this._compilation = compilation;
            this._addDiagnostic = addDiagnostic;
            this.Project = project;
            this.GeneratedCodeAnnotation = generatedCodeAnnotation;
            this.CancellationToken = cancellationToken;
            this.Helper = helper;
            this.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Reports a <see cref="Diagnostic" />.
        /// </summary>
        /// <param name="diagnostic"></param>
        public void ReportDiagnostic( Diagnostic diagnostic ) => this._addDiagnostic( diagnostic );

        /// <summary>
        /// Gets the annotation with which code generated by the aspect must be annotated. To mark
        /// a node, the <see cref="FormattingAnnotations.WithGeneratedCodeAnnotation(Microsoft.CodeAnalysis.SyntaxToken,Microsoft.CodeAnalysis.SyntaxAnnotation)"/>
        /// method must be called.
        /// </summary>
        public SyntaxAnnotation GeneratedCodeAnnotation { get; }

        public CancellationToken CancellationToken { get; }

        // TODO: add support for suppressions.
    }
}