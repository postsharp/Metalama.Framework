// Copyright (c) SharpCrafters s.r.o. All rights reserved.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Xml.Linq;

namespace PostSharp.Engineering.BuildTools.Coverage
{
    public partial class WarnCommand
    {
        private class SourceFile
        {
            private SyntaxTree? _syntaxTree;
            public int Id { get; }

            public string Path { get; }

            public SyntaxTree SyntaxTree =>
                this._syntaxTree ??= CSharpSyntaxTree.ParseText( 
                    File.ReadAllText( this.Path ),
                    CSharpParseOptions.Default, 
                    this.Path );

            public bool IsParsed => this._syntaxTree != null;

            public SourceFile( XElement element )
            {
                this.Path = element.Attribute( "fullPath" )!.Value;
                this.Id = int.Parse( element.Attribute( "uid" )!.Value );
            }

            public override string ToString() => this.Path;
        }
    }
}