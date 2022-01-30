// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Runtime.Serialization;

namespace Metalama.Framework.Engine.CodeFixes;

[DataContract]
public class SerializableSyntaxNode
{
    private SyntaxNode? _syntaxNode;
    private byte[]? _bytes;

    public byte[] Bytes
    {
        get
        {
            if ( this._bytes == null )
            {
                var memoryStream = new MemoryStream();
                this._syntaxNode!.SerializeTo( memoryStream );
                this._bytes = memoryStream.ToArray();
            }

            return this._bytes;
        }
        set => this._bytes = value;
    }

    public SyntaxNode Node
    {
        get
        {
            if ( this._syntaxNode == null )
            {
                var memoryStream = new MemoryStream( this._bytes );
                this._syntaxNode = CSharpSyntaxNode.DeserializeFrom( memoryStream );
            }

            return this._syntaxNode;
        }
    }

    public SerializableSyntaxNode() { }

    public SerializableSyntaxNode( SyntaxNode syntaxNode )
    {
        this._syntaxNode = syntaxNode;
    }
}