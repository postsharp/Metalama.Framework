// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.AspectOrdering
{
    /// <summary>
    /// Compares and sorts dependency objects.
    /// </summary>
    internal static class AspectLayerSorter
    {
        public static bool TrySort(
            ImmutableArray<AspectLayer> unsortedAspectLayers,
            IReadOnlyList<IAspectOrderingSource> aspectOrderingSources,
            IDiagnosticAdder diagnosticAdder,
            out ImmutableArray<OrderedAspectLayer> sortedAspectLayers )
            => TrySort(
                unsortedAspectLayers,
                aspectOrderingSources.SelectMany( s => s.GetAspectOrderSpecification( diagnosticAdder ) ).ToImmutableArray(),
                diagnosticAdder,
                out sortedAspectLayers );

        private static bool TrySort(
            ImmutableArray<AspectLayer> unsortedAspectLayers,
            IReadOnlyList<AspectOrderSpecification> relationships,
            IDiagnosticAdder diagnosticAdder,
            out ImmutableArray<OrderedAspectLayer> sortedAspectLayers )
        {
            // Build a graph of dependencies between unorderedTransformations.
            var n = unsortedAspectLayers.Length;

            var partNameToIndexMapping =
                unsortedAspectLayers
                    .Select( ( t, i ) => (t.AspectLayerId.FullName, Index: i) )
                    .ToDictionary( x => x.FullName!, x => x.Index );

            var aspectNameToIndicesMapping =
                unsortedAspectLayers
                    .Select( ( t, i ) => (t.AspectName, Index: i) )
                    .ToMultiValueDictionary( p => p.AspectName, p => p.Index );

            var aspectLayerNameToLocationsMappingBuilder = ImmutableDictionaryOfArray<string, AspectOrderSpecification>.CreateBuilder();

            DirectedGraph directedGraph = new( n );
            var hasPredecessor = new bool[n];

            foreach ( var relationship in relationships )
            {
                var previousIndices = ImmutableArray<int>.Empty;

                foreach ( var matchExpression in relationship.OrderedLayers )
                {
                    // Map the part string to a set of indices. We require the match to exist because it is a normal case
                    // to specify ordering for aspects that exist but are not necessarily a part of the current compilation.
                    ImmutableArray<int> currentIndices;

                    if ( matchExpression.EndsWith( ":*", StringComparison.Ordinal ) )
                    {
                        var aspectName = matchExpression.Substring( 0, matchExpression.Length - 2 );
                        currentIndices = aspectNameToIndicesMapping[aspectName];
                    }
                    else
                    {
                        if ( partNameToIndexMapping.TryGetValue( matchExpression, out var currentIndex ) )
                        {
                            currentIndices = ImmutableArray.Create( currentIndex );
                        }
                        else
                        {
                            currentIndices = ImmutableArray<int>.Empty;
                        }
                    }

                    if ( !currentIndices.IsEmpty )
                    {
                        if ( !previousIndices.IsEmpty )
                        {
                            // Index the relationship so we can later resolve locations.
                            aspectLayerNameToLocationsMappingBuilder.AddRange(
                                currentIndices,
                                i => unsortedAspectLayers[i].AspectLayerId.FullName,
                                _ => relationship );

                            // Add edges to previous nodes.
                            foreach ( var previousIndex in previousIndices )
                            {
                                foreach ( var currentIndex in currentIndices )
                                {
                                    directedGraph.AddEdge( previousIndex, currentIndex );
                                    hasPredecessor[currentIndex] = true;
                                }
                            }
                        }

                        previousIndices = currentIndices;
                    }
                }
            }

            var aspectLayerNameToLocationsMapping = aspectLayerNameToLocationsMappingBuilder.ToImmutable();

            // Perform a breadth-first search on the graph.
            var distances = directedGraph.GetInitialVector();
            var predecessors = directedGraph.GetInitialVector();

            var cycle = -1;

            for ( var i = 0; i < n; i++ )
            {
                if ( !hasPredecessor[i] )
                {
                    cycle = directedGraph.DoBreadthFirstSearch( i, distances, predecessors );

                    if ( cycle >= 0 )
                    {
                        break;
                    }
                }
            }

            // If we did not find any cycle, we need to check that we have ordered the whole graph.
            if ( cycle < 0 )
            {
                for ( var i = 0; i < n; i++ )
                {
                    if ( distances[i] == DirectedGraph.NotDiscovered )
                    {
                        // There is a node that we haven't ordered, which means that there is a cycle.
                        // Force the detection on the node to find the cycle.
                        cycle = directedGraph.DoBreadthFirstSearch( 0, distances, predecessors );

                        break;
                    }
                }
            }

            // Detect cycles.
            if ( cycle >= 0 )
            {
                // Build a string containing the unorderedTransformations of the cycle.
                Stack<int> cycleStack = new( unsortedAspectLayers.Length );

                var cursor = cycle;

                do
                {
                    cycleStack.Push( cursor );
                    cursor = predecessors[cursor];
                }
                while ( cursor != cycle && /* Workaround PostSharp bug 25438 */ cursor != DirectedGraph.NotDiscovered );

                var cycleNodes = cycleStack.Select( index => unsortedAspectLayers[index].AspectLayerId.FullName ).ToList();

                var cycleLocations = cycleNodes
                    .SelectMany( c => aspectLayerNameToLocationsMapping[c] )
                    .Select( s => s.DiagnosticLocation )
                    .WhereNotNull()
                    .GroupBy( l => l )
                    .OrderByDescending( g => g.Count() )
                    .Select( g => g.Key )
                    .ToList();

                var mainLocation = cycleLocations.FirstOrDefault();
                var additionalLocations = cycleLocations.Skip( 1 );

                var cycleNodesString = string.Join( ", ", cycleNodes );

                var diagnostic =
                    GeneralDiagnosticDescriptors.CycleInAspectOrdering.CreateDiagnostic(
                        mainLocation,
                        cycleNodesString,
                        additionalLocations );

                diagnosticAdder.Report( diagnostic );

                return false;
            }

            // Sort the distances vector.
            var sortedIndexes = new int[n];

            for ( var i = 0; i < n; i++ )
            {
                sortedIndexes[i] = i;
            }

            Array.Sort( sortedIndexes, ( i, j ) => distances[i].CompareTo( distances[j] ) );

            // Build the ordered list of aspects and assign the distance.
            // Note that we don't detect cycles because some aspect types that are present in the compilation may actually be unused.
            var sortedAspectLayersBuilder = ImmutableArray.CreateBuilder<OrderedAspectLayer>( n );

            for ( var i = 0; i < n; i++ )
            {
                var order = distances[sortedIndexes[i]];
                sortedAspectLayersBuilder.Add( new OrderedAspectLayer( order, unsortedAspectLayers[sortedIndexes[i]] ) );
            }

            sortedAspectLayers = sortedAspectLayersBuilder.ToImmutable();

            return true;
        }
    }
}