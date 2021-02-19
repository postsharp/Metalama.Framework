using Caravela.Framework.Impl.Collections;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.AspectOrdering
{
    /// <summary>
    /// Compares and sorts dependency objects.
    /// </summary>
    internal static class AspectPartSorter
    {
        public static bool TrySort(
            ImmutableArray<AspectPart> unsortedAspectParts,
            IReadOnlyList<IAspectOrderingSource> aspectOrderingSources,
            Action<Diagnostic> reportDiagnostic,
            out ImmutableArray<OrderedAspectPart> sortedAspectParts )
            => TrySort(
                unsortedAspectParts, 
                aspectOrderingSources.SelectMany( s => s.GetAspectOrderSpecification() ).ToImmutableArray(),
                reportDiagnostic, 
                out sortedAspectParts );
        
        public static bool TrySort(
            ImmutableArray<AspectPart> unsortedAspectParts,
            IReadOnlyList<AspectOrderSpecification> relationships, 
            Action<Diagnostic> reportDiagnostic,
            out ImmutableArray<OrderedAspectPart> sortedAspectParts )
        {
            // Build a graph of dependencies between unorderedTransformations.
            var n = unsortedAspectParts.Length;

            Dictionary<string, int> partNameToIndexMapping =
                unsortedAspectParts
                    .Select((t, i) => (t.FullName, Index: i))
                    .ToDictionary(x => x.FullName!, x => x.Index);
            
            ImmutableMultiValueDictionary<string, int> aspectNameToIndicesMapping = 
                unsortedAspectParts
                    .Select((t, i) => ( AspectName: t.AspectType.Name, Index: i))
                    .ToMultiValueDictionary( p => p.AspectName, p => p.Index );

            var aspectPartNameToLocationsMappingBuilder = ImmutableMultiValueDictionary<string, AspectOrderSpecification>.CreateBuilder();


            Graph graph = new Graph(n);
            bool[] hasPredecessor = new bool[n];

            foreach (var relationship in relationships)
            {
                ImmutableArray<int> previousIndices = ImmutableArray<int>.Empty;

                foreach (var matchExpression in relationship.OrderedParts)
                {
                    // Map the part string to a set of indices. We don't require the match to exist because it is a normal case
                    // to specify ordering for aspects that exist but are not necessarily a part of the current compilation.
                    ImmutableArray<int> currentIndices;
                    if ( matchExpression.EndsWith( ":*" ) )
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

                    if (!currentIndices.IsEmpty)
                    {
                        if (!previousIndices.IsEmpty)
                        {
                            // Index the relationship so we can later resolve locations.
                            aspectPartNameToLocationsMappingBuilder.AddRange( currentIndices, i => unsortedAspectParts[i].FullName, i => relationship );
                            
                            // Add edges to previous nodes.
                            foreach ( var previousIndex in previousIndices )
                            {
                                foreach ( var currentIndex in currentIndices )
                                {
                                    graph.AddEdge( previousIndex, currentIndex);
                                    hasPredecessor[currentIndex] = true;
                                }
                            }
                        }
                        
                        previousIndices= currentIndices;

                    }
                }
            }

            var aspectPartNameToLocationsMapping = aspectPartNameToLocationsMappingBuilder.ToImmutable();

            // Perform a breadth-first search on the graph.
            int[] distances = graph.GetInitialVector();
            int[] predecessors = graph.GetInitialVector();

            var cycle = -1;
            for (var i = 0; i < n; i++)
            {
                if (!hasPredecessor[i])
                {
                    cycle = graph.DoBreadthFirstSearch(i, distances, predecessors);
                    if (cycle >= 0)
                    {
                        break;
                    }
                }
            }

            // If did not manage to find a cycle, we need to check that we have ordered the whole graph.
            if (cycle < 0)
            {
                for (var i = 0; i < n; i++)
                {
                    if (distances[i] == AbstractGraph.NotDiscovered)
                    {
                        // There is a node that we haven't ordered, which means that there is a cycle.
                        // Force the detection on the node to find the cycle.
                        cycle = graph.DoBreadthFirstSearch(0, distances, predecessors);
                        break;
                    }
                }
            }

            // Detect cycles.
            if (cycle >= 0)
            {
                // Build a string containing the unorderedTransformations of the cycle.
                Stack<int> cycleStack = new Stack<int>(unsortedAspectParts.Length);

                var cursor = cycle;
                do
                {
                    cycleStack.Push(cursor);
                    cursor = predecessors[cursor];
                }
                while (cursor != cycle && /* Workaround PostSharp bug 25438 */ cursor != AbstractGraph.NotDiscovered);

                var cycleNodes = cycleStack.Select(cursor => unsortedAspectParts[cursor].FullName);
                var cycleLocations = cycleNodes
                    .SelectMany( c => aspectPartNameToLocationsMapping[c] )
                    .Select( s => s.DiagnosticLocation )
                    .Where( l => l != null )
                    .GroupBy( l => l )
                    .OrderByDescending( g => g.Count() )
                    .Select( g => g.Key );

                var mainLocation = cycleLocations.FirstOrDefault();
                var additionalLocations = cycleLocations.Skip( 1 );
                    
                var cycleNodesString = string.Join(", ", cycleNodes);

                var diagnostic = Diagnostic.Create(
                    GeneralDiagnosticDescriptors.CycleInAspectOrdering,
                    mainLocation,
                    additionalLocations,
                    cycleNodesString );
                reportDiagnostic( diagnostic );

                return false;
            }

            // Sort the distances vector.
            int[] sortedIndexes = new int[n];
            for (var i = 0; i < n; i++)
            {
                sortedIndexes[i] = i;
            }

            Array.Sort(sortedIndexes, (i, j) => distances[i].CompareTo(distances[j]));

            // Build the ordered list of aspects and assign the distance.
            // Note that we don't detect cycles because some aspect types that are present in the compilation may actually be ununsed.
            var sortedAspectPartsBuilder = ImmutableArray.CreateBuilder<OrderedAspectPart>(n);
            for (var i = 0; i < n; i++)
            {
                var order = distances[sortedIndexes[i]];
                sortedAspectPartsBuilder.Add( new OrderedAspectPart( order, unsortedAspectParts[sortedIndexes[i]]) );
            }

            sortedAspectParts = sortedAspectPartsBuilder.ToImmutable();

            return true;

        }

    }
}
