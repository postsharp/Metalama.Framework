// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Impl.Utilities
{
    internal class CompilationReferenceGraph
    {
        private static readonly ConditionalWeakTable<Compilation, CompilationReferenceGraph> _instances = new();
        private readonly Dictionary<IAssemblySymbol, (int Min, int Max)> _depth = new();

        public static CompilationReferenceGraph GetInstance( Compilation compilation )
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if ( !_instances.TryGetValue( compilation, out var instance ) )
            {
                lock ( _instances )
                {
                    if ( !_instances.TryGetValue( compilation, out instance ) )
                    {
                        instance = new CompilationReferenceGraph( compilation );
                        _instances.Add( compilation, instance );
                    }
                }
            }

            return instance;
        }

        private CompilationReferenceGraph( Compilation compilation )
        {
            foreach ( var assembly in compilation.SourceModule.ReferencedAssemblySymbols )
            {
                this.Visit( assembly, 0 );
            }
        }

        public (int Min, int Max) GetDepth( IAssemblySymbol assembly ) => this._depth[assembly];

        private void Visit( IAssemblySymbol assembly, int depth )
        {
            if ( assembly.Name.StartsWith( "System.", StringComparison.OrdinalIgnoreCase ) )
            {
                // We are not interested in system assemblies, and here a cheap trick to exclude them.
                return;
            }

            if ( !this._depth.TryGetValue( assembly, out var depthRange ) )
            {
                depthRange = (depth, depth);
            }
            else
            {
                depthRange = (Math.Min( depthRange.Min, depth ), Math.Max( depthRange.Max, depth ));
            }

            this._depth[assembly] = depthRange;

            foreach ( var reference in assembly.Modules.SelectMany( m => m.ReferencedAssemblySymbols ) )
            {
                this.Visit( reference, depth + 1 );
            }
        }
    }
}