// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.AspectWeavers
{
    /// <summary>
    /// Context for the <see cref="IAspectWeaver"/>.
    /// </summary>
    public sealed partial class AspectWeaverContext
    {
        private readonly Action<Diagnostic> _addDiagnostic;
        private readonly Action<ManagedResource> _addResource;
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

        /// <summary>
        /// Adds a new <see cref="ManagedResource" /> to the compilation.
        /// </summary>
        /// <param name="resource"></param>
        public void AddResource( ManagedResource resource ) => this._addResource( resource );

        /// <summary>
        /// Rewrites the syntax trees affected by aspects.
        /// </summary>
        /// <param name="rewriter">A <c>CSharpSyntaxRewriter</c> whose <c>Visit</c> method is invoked for all declarations
        /// that are the target of aspects handled by the current <see cref="IAspectWeaver"/> (see <see cref="AspectInstances"/>).
        /// In case of partial classes or methods, the <c>Visit</c> method is invoked for each partial declaration.
        /// </param>
        public void RewriteAspectTargets( CSharpSyntaxRewriter rewriter )
        {
            var nodes = this.AspectInstances.Values
                .Select( a => a.TargetDeclaration.GetSymbol( this._compilation.Compilation ) )
                .Where( s => s != null )
                .SelectMany( s => s!.DeclaringSyntaxReferences )
                .GroupBy( r => r.SyntaxTree );

            List<SyntaxTreeModification> modifiedSyntaxTrees = new();

            foreach ( var group in nodes )
            {
                var oldTree = @group.Key;
                var outerRewriter = new Rewriter( group.Select( r => r.GetSyntax() ).ToImmutableHashSet(), rewriter );
                var oldRoot = oldTree.GetRoot();
                var newRoot = outerRewriter.Visit( oldRoot )!;

                if ( oldRoot != newRoot )
                {
                    modifiedSyntaxTrees.Add( new SyntaxTreeModification( oldTree.WithRootAndOptions( newRoot, oldTree.Options ), oldTree ) );
                }
            }

            this.Compilation = this.Compilation.WithSyntaxTreeModifications( modifiedSyntaxTrees );
        }

        internal AspectWeaverContext(
            IAspectClass aspectClass,
            IReadOnlyDictionary<ISymbol, IAspectInstance> aspectInstances,
            IPartialCompilation compilation,
            Action<Diagnostic> addDiagnostic,
            Action<ManagedResource> addManifestResource,
            IAspectWeaverHelper helper,
            IServiceProvider serviceProvider,
            IProject project )
        {
            this.AspectClass = aspectClass;
            this.AspectInstances = aspectInstances;
            this._compilation = compilation;
            this._addDiagnostic = addDiagnostic;
            this.Project = project;
            this._addResource = addManifestResource;
            this.Helper = helper;
            this.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Reports a <see cref="Diagnostic" />.
        /// </summary>
        /// <param name="diagnostic"></param>
        public void ReportDiagnostic( Diagnostic diagnostic ) => this._addDiagnostic( diagnostic );

        // TODO: add support for suppressions.
    }
}