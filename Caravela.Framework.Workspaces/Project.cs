// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Impl.Utilities.Dump;
using System.Collections.Immutable;

namespace Caravela.Framework.Workspaces
{
    public sealed class Project
    {
        public string Path { get; }

        public ICompilation Compilation { get; }

        public string? TargetFramework { get; }

        internal Project( string path, ICompilation compilation, string? targetFramework )
        {
            this.Path = path;
            this.Compilation = compilation;
            this.TargetFramework = targetFramework;
        }

        [Memo]
        public ImmutableArray<INamedType> Types => this.Compilation.Types.SelectManyRecursive( t => t.NestedTypes ).ToImmutableArray();

        // ReSharper disable once UnusedMember.Local
        private object? ToDump() => ObjectDumper.Dump( this );

        public override string ToString()
        {
            var name = System.IO.Path.GetFileNameWithoutExtension( this.Path );

            if ( this.TargetFramework != null )
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