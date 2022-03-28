// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.Linking;

internal class LinkerCodeTransformationRegistry
{
    private readonly CompilationModel _finalCompilationModel;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<InsertedStatement>> _codeTransformations;
    private readonly HashSet<ISymbol> _declarationsWithStaticCtorCodeTransformations;
    private readonly HashSet<ISymbol> _declarationsWithCodeTransformations;

    public LinkerCodeTransformationRegistry(
        CompilationModel finalCompilationModel,
        IReadOnlyDictionary<string, IReadOnlyList<InsertedStatement>> codeTransformations )
    {
        this._finalCompilationModel = finalCompilationModel;
        this._codeTransformations = codeTransformations;
        this._declarationsWithStaticCtorCodeTransformations = new HashSet<ISymbol>( StructuralSymbolComparer.Default );
        this._declarationsWithCodeTransformations = new HashSet<ISymbol>( StructuralSymbolComparer.Default );

        foreach ( var codeTransformationMark in codeTransformations.Values.SelectMany( x => x ) )
        {
            var symbol = codeTransformationMark.Parent.TargetDeclaration.GetSymbol();

            if ( symbol != null )
            {
                this._declarationsWithCodeTransformations.Add( symbol );
            }
            else
            {
                var typeSymbol = codeTransformationMark.Parent.TargetDeclaration.DeclaringType.GetSymbol();
                this._declarationsWithStaticCtorCodeTransformations.Add( typeSymbol );
            }
        }
    }

    public bool HasCodeTransformations( ISymbol symbol )
        => this._declarationsWithCodeTransformations.Contains( symbol )
           || (symbol is IMethodSymbol { MethodKind: MethodKind.StaticConstructor }
               && this._declarationsWithStaticCtorCodeTransformations.Contains( symbol.ContainingSymbol ));

    public bool TryGetTransformationMarksForNode( SyntaxNode node, [NotNullWhen( true )] out IEnumerable<InsertedStatement>? marks )
    {
        if ( node.GetLinkerMarkedNodeId() is { } id
             && this._codeTransformations.TryGetValue( id, out var unsortedMarks ) )
        {
            marks = this.SortMarks( unsortedMarks );

            return true;
        }

        marks = null;

        return false;
    }

    private IEnumerable<InsertedStatement> SortMarks( IEnumerable<InsertedStatement> marks )
    {
        // TODO: This sort is intended only for InsertHead marks.
        // TODO: Needs to get a comparer.
        var memberMarks = new Dictionary<IMember, List<InsertedStatement>>( this._finalCompilationModel.InvariantComparer );
        var typeMarks = new List<InsertedStatement>();

        foreach ( var mark in marks )
        {
            switch ( mark.Parent.ContextDeclaration )
            {
                case INamedType:
                    typeMarks.Add( mark );

                    break;

                case IMember member:
                    if ( !memberMarks.TryGetValue( member, out var list ) )
                    {
                        memberMarks[member] = list = new List<InsertedStatement>();
                    }

                    list.Add( mark );

                    break;

                default:
                    throw new AssertionFailedException();
            }
        }

        foreach ( var pair in memberMarks )
        {
            foreach ( var mark in pair.Value )
            {
                yield return mark;
            }
        }

        foreach ( var mark in typeMarks )
        {
            yield return mark;
        }
    }
}