// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Runtime.Serialization;

namespace Metalama.Framework.Engine.CodeFixes;

[DataContract]
public class SyntaxTreeChange
{
    [DataMember( Order = 0 )]
    public string FilePath { get; }

    [DataMember( Order = 1 )]
    public SerializableSyntaxNode Root { get; }

    public SyntaxTreeChange( string filePath, SerializableSyntaxNode root )
    {
        this.FilePath = filePath;
        this.Root = root;
    }

    public SyntaxTreeChange( SyntaxTree tree ) : this( tree.FilePath, tree.GetRoot() ) { }

    public SyntaxTreeChange( string filePath, SyntaxNode root )
    {
        this.FilePath = filePath;
        this.Root = new SerializableSyntaxNode( root );
    }
}