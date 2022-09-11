﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Diff
{
    /// <summary>
    /// Represents changes between two instances of the <see cref="Microsoft.CodeAnalysis.Compilation"/> class.
    /// </summary>
    internal readonly struct CompilationChanges
    {
        private readonly ImmutableDictionary<string, SyntaxTreeChange> _syntaxTreeChanges;

        /// <summary>
        /// Gets the set of syntax tree changes.
        /// </summary>
        public IEnumerable<SyntaxTreeChange> SyntaxTreeChanges => this._syntaxTreeChanges.Values;

        public ImmutableHashSet<TypeDependencyKey> AddedPartialTypes { get; }

        /// <summary>
        /// Gets a value indicating whether the changes affects the compile-time subproject.
        /// </summary>
        public bool HasCompileTimeCodeChange { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationChanges"/> class.
        /// </summary>
        public CompilationChanges(
            ImmutableDictionary<string, SyntaxTreeChange> syntaxTreeChanges,
            ImmutableHashSet<TypeDependencyKey> addedPartialTypes,
            bool hasCompileTimeCodeChange,
            Compilation compilationToAnalyze,
            bool isIncremental )
        {
            this._syntaxTreeChanges = syntaxTreeChanges;
            this.AddedPartialTypes = addedPartialTypes;
            this.HasCompileTimeCodeChange = hasCompileTimeCodeChange;
            this.CompilationToAnalyze = compilationToAnalyze;
            this.IsIncremental = isIncremental;
        }

        public static CompilationChanges Empty( Compilation compilation )
            => new( ImmutableDictionary<string, SyntaxTreeChange>.Empty, ImmutableHashSet<TypeDependencyKey>.Empty, false, compilation, true );

        public bool HasChange => this._syntaxTreeChanges.Count > 0 || this.HasCompileTimeCodeChange;

        public bool IsIncremental { get; }

        /// <summary>
        /// Gets the <see cref="Microsoft.CodeAnalysis.Compilation"/> that must be analyzed. If <see cref="HasChange"/> is false,
        /// this is the last compilation of <see cref="CompilationVersion"/>. Otherwise, this is the new compilation. 
        /// </summary>
        public Compilation CompilationToAnalyze { get; }

        public CompilationChanges Merge( CompilationChanges newChanges )
        {
            if ( !this.HasChange || !newChanges.IsIncremental )
            {
                return newChanges;
            }
            else if ( !newChanges.HasChange )
            {
                return this;
            }
            else
            {
                var mergedSyntaxTreeBuilder = this._syntaxTreeChanges.ToBuilder();

                foreach ( var newChange in newChanges._syntaxTreeChanges )
                {
                    if ( !mergedSyntaxTreeBuilder.TryGetValue( newChange.Key, out var oldChange ) )
                    {
                        mergedSyntaxTreeBuilder.Add( newChange.Key, newChange.Value );
                    }
                    else
                    {
                        var merged = oldChange.Merge( newChange.Value );

                        if ( merged.SyntaxTreeChangeKind == SyntaxTreeChangeKind.None )
                        {
                            mergedSyntaxTreeBuilder.Remove( newChange.Key );
                        }
                        else
                        {
                            mergedSyntaxTreeBuilder[newChange.Key] = merged;
                        }
                    }
                }

                return new CompilationChanges(
                    mergedSyntaxTreeBuilder.ToImmutable(),
                    this.AddedPartialTypes.Union( newChanges.AddedPartialTypes ),
                    this.HasCompileTimeCodeChange | newChanges.HasCompileTimeCodeChange,
                    newChanges.CompilationToAnalyze,
                    this.IsIncremental );
            }
        }

        public override string ToString() => $"HasCompileTimeCodeChange={this.HasCompileTimeCodeChange}, SyntaxTreeChanges={this._syntaxTreeChanges.Count}";
    }
}