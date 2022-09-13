// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.Engine;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Diff
{
    internal enum PartialTypeChangeKind
    {
        None,
        Added,
        Removed
    }

    internal readonly struct PartialTypeChange
    {
        public TypeDependencyKey Type { get; }

        public PartialTypeChangeKind Kind { get; }

        public PartialTypeChange( TypeDependencyKey type, PartialTypeChangeKind kind )
        {
            this.Type = type;
            this.Kind = kind;
        }

        public PartialTypeChange Merge( PartialTypeChange change )
            => (this.Kind, change.Kind) switch
            {
                (_, PartialTypeChangeKind.None) => this,
                (PartialTypeChangeKind.None, _) => change,
                _ => new PartialTypeChange( this.Type, PartialTypeChangeKind.None )
            };
    }

    /// <summary>
    /// Represents a change between two versions of a <see cref="SyntaxTree"/>.
    /// </summary>
    internal readonly struct SyntaxTreeChange
    {
        /// <summary>
        /// Gets the kind of change between the old and new syntax trees.
        /// </summary>
        public SyntaxTreeChangeKind SyntaxTreeChangeKind { get; }

        /// <summary>
        /// Gets a value indicating how the <see cref="HasCompileTimeCode"/> value has changed between the old
        /// and the new syntax tree.
        /// </summary>
        public CompileTimeChangeKind CompileTimeChangeKind { get; }

        /// <summary>
        /// Gets the path of the syntax tree.
        /// </summary>
        public string FilePath { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public readonly SyntaxTreeVersion OldSyntaxTreeVersion;

        // ReSharper disable once MemberCanBePrivate.Global
        public readonly SyntaxTreeVersion NewSyntaxTreeVersion;

        /// <summary>
        /// Gets the list of partial types that have been added between the old version and the new version.
        /// </summary>
        public ImmutableArray<PartialTypeChange> PartialTypeChanges { get; }

        /// <summary>
        /// Gets the new syntax tree, unless the current item represents a deleted tree.
        /// </summary>
        public SyntaxTree NewTree => this.NewSyntaxTreeVersion.SyntaxTree;

        public ulong OldHash => this.OldSyntaxTreeVersion.DeclarationHash;

        public ulong NewHash => this.NewSyntaxTreeVersion.DeclarationHash;

        /// <summary>
        /// Gets a value indicating whether the new syntax tree contain compile-time code.
        /// </summary>
        public bool HasCompileTimeCode => this.NewSyntaxTreeVersion.HasCompileTimeCode;

        public static SyntaxTreeChange NonIncremental( in SyntaxTreeVersion syntaxTreeVersion )
            => new(
                syntaxTreeVersion.SyntaxTree.FilePath,
                SyntaxTreeChangeKind.Added,
                syntaxTreeVersion.HasCompileTimeCode ? CompileTimeChangeKind.NewlyCompileTime : CompileTimeChangeKind.None,
                default,
                syntaxTreeVersion );

        public SyntaxTreeChange(
            string filePath,
            SyntaxTreeChangeKind syntaxTreeChangeKind,
            CompileTimeChangeKind compileTimeChangeKind,
            in SyntaxTreeVersion oldSyntaxTreeVersion,
            in SyntaxTreeVersion newSyntaxTreeVersion )
        {
            this.SyntaxTreeChangeKind = syntaxTreeChangeKind;
            this.CompileTimeChangeKind = compileTimeChangeKind;
            this.FilePath = filePath;
            this.NewSyntaxTreeVersion = newSyntaxTreeVersion;
            this.OldSyntaxTreeVersion = oldSyntaxTreeVersion;

            // Detecting changes in partial types.
            this.PartialTypeChanges = ImmutableArray<PartialTypeChange>.Empty;

            switch ( syntaxTreeChangeKind )
            {
                case SyntaxTreeChangeKind.Changed when newSyntaxTreeVersion.PartialTypesHash != oldSyntaxTreeVersion.PartialTypesHash:
                    foreach ( var partialType in newSyntaxTreeVersion.PartialTypes )
                    {
                        if ( !oldSyntaxTreeVersion.PartialTypes.Contains( partialType ) )
                        {
                            this.PartialTypeChanges = this.PartialTypeChanges.Add( new PartialTypeChange( partialType, PartialTypeChangeKind.Added ) );
                        }
                    }

                    foreach ( var partialType in oldSyntaxTreeVersion.PartialTypes )
                    {
                        if ( !newSyntaxTreeVersion.PartialTypes.Contains( partialType ) )
                        {
                            this.PartialTypeChanges = this.PartialTypeChanges.Add( new PartialTypeChange( partialType, PartialTypeChangeKind.Removed ) );
                        }
                    }

                    break;

                case SyntaxTreeChangeKind.Added:
                    this.PartialTypeChanges = newSyntaxTreeVersion.PartialTypes.Select( t => new PartialTypeChange( t, PartialTypeChangeKind.Added ) )
                        .ToImmutableArray();

                    break;
            }
        }

        public override string ToString() => $"{this.FilePath}, ChangeKind={this.SyntaxTreeChangeKind}, CompileTimeChangeKind={this.CompileTimeChangeKind}";

        public SyntaxTreeChange Merge( in SyntaxTreeChange newChange )
        {
            var newSyntaxTreeChangeKind = (this.SyntaxTreeChangeKind, newChange.SyntaxTreeChangeKind) switch
            {
                (SyntaxTreeChangeKind.Added, SyntaxTreeChangeKind.Changed) => SyntaxTreeChangeKind.Added,
                (SyntaxTreeChangeKind.Added, SyntaxTreeChangeKind.Deleted) => SyntaxTreeChangeKind.None,
                (SyntaxTreeChangeKind.Added, SyntaxTreeChangeKind.Added) => throw new AssertionFailedException(),
                (_, SyntaxTreeChangeKind.Deleted) => SyntaxTreeChangeKind.Deleted,
                (SyntaxTreeChangeKind.Deleted, SyntaxTreeChangeKind.Added) when newChange.NewHash != this.OldHash => SyntaxTreeChangeKind.Changed,
                (SyntaxTreeChangeKind.Deleted, SyntaxTreeChangeKind.Added) when newChange.NewHash == this.OldHash => SyntaxTreeChangeKind.None,
                (SyntaxTreeChangeKind.Deleted, _) => throw new AssertionFailedException(),
                (SyntaxTreeChangeKind.Changed, SyntaxTreeChangeKind.Changed) => SyntaxTreeChangeKind.Changed,
                _ => throw new AssertionFailedException()
            };

            var newCompileTimeChangeKind = (this.CompileTimeChangeKind, newChange.CompileTimeChangeKind) switch
            {
                (_, CompileTimeChangeKind.None) => this.CompileTimeChangeKind,
                (CompileTimeChangeKind.None, _) => newChange.CompileTimeChangeKind,
                (CompileTimeChangeKind.NewlyCompileTime, CompileTimeChangeKind.NoLongerCompileTime) => CompileTimeChangeKind.None,
                (CompileTimeChangeKind.NewlyCompileTime, _) => CompileTimeChangeKind.NewlyCompileTime,
                (CompileTimeChangeKind.NoLongerCompileTime, CompileTimeChangeKind.NewlyCompileTime) => CompileTimeChangeKind.None,

                _ => throw new AssertionFailedException()
            };

            return new SyntaxTreeChange(
                this.FilePath,
                newSyntaxTreeChangeKind,
                newCompileTimeChangeKind,
                this.OldSyntaxTreeVersion,
                newChange.NewSyntaxTreeVersion );
        }
    }
}