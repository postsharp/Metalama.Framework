// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Base interface for objects that can cause aspects to be added to a compilation. Predecessors are exposed on
    /// the <see cref="IAspectInstance.Predecessors"/> property.
    /// </summary>
    [CompileTime]
    [InternalImplement]
    public interface IAspectPredecessor
    {
        /// <summary>
        /// Gets the number of predecessors between the root cause and the current predecessor, or <c>0</c>
        /// if the current predecessor is the root cause. 
        /// </summary>
        int PredecessorDegree { get; }

        /// <summary>
        /// Gets the declaration to which the aspect or fabric is applied.
        /// </summary>
        IRef<IDeclaration> TargetDeclaration { get; }

        /// <summary>
        /// Gets the list of objects that have caused the current aspect instance (but not any instance in the <see cref="IAspectInstance.SecondaryInstances"/> list)
        /// to be created.
        /// </summary>
        /// <seealso href="@child-aspects"/>
        ImmutableArray<AspectPredecessor> Predecessors { get; }
    }

    public static class AspectPredecessorExtensions
    {
        public static IReadOnlyList<IAspectPredecessor> GetRoots( this IAspectPredecessor predecessor )
        {
            if ( predecessor.Predecessors.IsDefaultOrEmpty )
            {
                return new [] { predecessor };
            }

            var list = new List<IAspectPredecessor>();
            ProcessRecursive( predecessor );

            return list;

            void ProcessRecursive( IAspectPredecessor p )
            {
                if ( p.Predecessors.IsDefaultOrEmpty )
                {
                    list.Add( p );
                }
                else
                {
                    foreach ( var child in p.Predecessors )
                    {
                        ProcessRecursive( child.Instance );
                    }
                }
            }
        }
    }
}