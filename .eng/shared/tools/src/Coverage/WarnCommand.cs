// Copyright (c) SharpCrafters s.r.o. All rights reserved.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Xml.Linq;

namespace PostSharp.Engineering.BuildTools.Coverage
{
    public partial class WarnCommand : Command
    {
        public WarnCommand() : base( "warn", "Emit warnings based on a test coverage report" )
        {
            this.AddArgument( new Argument<string>( "path", "Path to the OpenCover xml file" ) );

            this.Handler = CommandHandler.Create<InvocationContext, string>( this.Execute );
        }

        private void Execute( InvocationContext context, string path )
        {
            var totalInvalidDeclarations = 0;

            var document = XDocument.Load( path );

            foreach ( var moduleNode in document.Root!.Element( "Modules" )!.Elements( "Module" ) )
            {
                this.ProcessModule( context, moduleNode, ref totalInvalidDeclarations );
            }

            context.Console.Out.WriteLine(
                $"The whole solution has {totalInvalidDeclarations} declaration(s) with insufficient test coverage." );
            context.ResultCode = totalInvalidDeclarations == 0 ? 0 : 1;
        }

        private void ProcessModule( InvocationContext context, XElement moduleNode, ref int totalInvalidDeclarations )
        {
            var moduleName = moduleNode.Element( "ModuleName" )!.Value;
            if ( moduleName.Contains( ".Tests" ) )
            {
                return;
            }

            var files = new Dictionary<int, SourceFile>();
            HashSet<SyntaxNode> nonCoveredNodes = new();

            foreach ( var fileNode in moduleNode.Element( "Files" )!.Elements( "File" ) )
            {
                var file = new SourceFile( fileNode );
                files.Add( file.Id, file );
            }

            foreach ( var classNode in moduleNode.Element( "Classes" )!.Elements( "Class" ) )
            {
                foreach ( var methodNode in classNode.Element( "Methods" )!.Elements( "Method" ) )
                {
                    foreach ( var sequencePointNode in methodNode.Element( "SequencePoints" )!
                        .Elements( "SequencePoint" ) )
                    {
                        var sequencePoint = new SequencePoint( sequencePointNode );

                        if ( sequencePoint.CoveredBranchCount == 0 && sequencePoint.TotalBranchCount > 0 )
                        {
                            // The sequence point is not covered.
                            var file = files[sequencePoint.FileId];
                            var sourceText = file.SyntaxTree.GetText();
                            var startLine = sourceText.Lines[sequencePoint.StartLine];
                            var endLine = sourceText.Lines[sequencePoint.EndLine];
                            var span = TextSpan.FromBounds( startLine.Start, endLine.End );

                            var node = file.SyntaxTree.GetRoot().FindNode( span );


                            nonCoveredNodes.Add( node );
                        }
                    }
                }
            }

            CoverageVisitor visitor = new(nonCoveredNodes, context.Console);
            foreach ( var file in files.Values )
            {
                if ( file.IsParsed )
                {
                    visitor.Visit( file.SyntaxTree.GetRoot() );
                }
            }

            context.Console.Out.WriteLine(
                $"Module {moduleName} has {visitor.InvalidDeclarationCounts} declaration(s) with insufficient test coverage." );
            totalInvalidDeclarations += visitor.InvalidDeclarationCounts;
        }
    }
}