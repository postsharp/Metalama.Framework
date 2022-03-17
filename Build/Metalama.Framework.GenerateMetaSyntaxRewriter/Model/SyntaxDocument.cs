// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.GenerateMetaSyntaxRewriter.Model;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter;

internal class SyntaxDocument
{
    public RoslynVersion Version { get; }

    private readonly Tree _tree;
    private readonly IDictionary<string, string?> _parentMap;
    private readonly IDictionary<string, Node> _nodeMap;

    public SyntaxDocument( RoslynVersion version )
    {
        this.Version = version;
        this._tree = TreeReader.ReadTree( $"Syntax-{version.Name}.xml" );
        this._nodeMap = this._tree.Types.OfType<Node>().ToDictionary( n => n.Name );
        this._parentMap = this._tree.Types.ToDictionary( n => n.Name, n => n.Base )!;
        this._parentMap.Add( this._tree.Root, null );
    }

    public Node? GetNode( string typeName ) => this._nodeMap.TryGetValue( typeName, out var node ) ? node : null;

    public bool IsNode( string typeName )
    {
        return this._parentMap.ContainsKey( typeName );
    }

    public IReadOnlyList<TreeType> Types => this._tree.Types;
}