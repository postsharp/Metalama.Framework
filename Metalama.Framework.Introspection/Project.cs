// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Introspection
{
    /// <summary>
    /// Represents a C# project for a specific compilation.
    /// </summary>
    public sealed class Project
    {
        public string Path { get; }

        public ICompilation Compilation { get; }

        public TargetFramework TargetFramework { get; }

        internal Project( string path, ICompilation compilation, string? targetFramework )
        {
            this.Path = path;
            this.Compilation = compilation;
            this.TargetFramework = new TargetFramework( targetFramework );
        }

        [Memo]
        public ImmutableArray<IDiagnostic> Diagnostics
            => this.Compilation
                .GetRoslynCompilation()
                .GetDiagnostics()
                .Select( x => new DiagnosticModel( x, this.Compilation ) )
                .ToImmutableArray<IDiagnostic>();

        /// <summary>
        /// Gets the set of types defined in the project, including nested types.
        /// </summary>
        [Memo]
        public ImmutableArray<INamedType> Types => this.Compilation.Types.SelectManyRecursive( t => t.NestedTypes ).ToImmutableArray();

        public override string ToString()
        {
            var name = System.IO.Path.GetFileNameWithoutExtension( this.Path );

            if ( this.TargetFramework.Id != null )
            {
                return name + "(" + this.TargetFramework + ")";
            }
            else
            {
                return name;
            }
        }
    }
}