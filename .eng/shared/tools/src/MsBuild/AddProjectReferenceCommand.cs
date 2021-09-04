// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Xml;

namespace PostSharp.Engineering.BuildTools.MsBuild
{
    internal class AddProjectReferenceCommand : Command
    {
        public AddProjectReferenceCommand() : base( "apr",
            "Adds a project reference next to another project reference in all projects matching a filter." )
        {
            this.AddArgument( new Argument<string>( "existing", "Existing reference file name" ) );
            this.AddArgument( new Argument<string>( "new", "Added reference path" ) );
            this.AddArgument( new Argument<string?>( "filter", () => null, "Project name filter" ) );

            this.Handler = CommandHandler.Create<InvocationContext, string, string, string>( Execute );
        }

        private static int Execute( InvocationContext context, string existing, string @new, string filter )
        {
            foreach ( var project in Directory.EnumerateFiles( Directory.GetCurrentDirectory(), $"*{filter}*.csproj",
                SearchOption.AllDirectories ) )
            {
                AddReference( context, project, existing, @new );
            }

            return 0;
        }

        private static void AddReference( InvocationContext context, string project, string existingReference,
            string newReference )
        {
            context.Console.Out.Write( Path.GetFileName( project ) );
            context.Console.Out.Write( ": " );

            var xml = new XmlDocument();
            xml.Load( project );

            var newReferenceFileName = Path.GetFileName( newReference );

            var newReferenceItem =
                xml.SelectSingleNode( $"//ProjectReference[contains(@Include,'{newReferenceFileName}')]" );

            if ( newReferenceItem != null )
            {
                context.Console.Out.WriteLine( "skipped - contains new reference" );
                return;
            }

            var existingReferenceItem =
                xml.SelectSingleNode( $"//ProjectReference[contains(@Include,'{existingReference}')]" );

            if ( existingReferenceItem == null )
            {
                context.Console.Out.WriteLine( $"skipped - doesn't reference {existingReference}" );
                return;
            }

            // https://stackoverflow.com/questions/1766748/how-do-i-get-a-relative-path-from-one-path-to-another-in-c-sharp
            var projectUri = new Uri( project );
            var newReferenceFullPath = Path.GetFullPath( newReference );
            var newReferenceUri = new Uri( newReferenceFullPath );
            var newReferenceRelativeUri = projectUri.MakeRelativeUri( newReferenceUri );
            var newReferenceRelativePath =
                Uri.UnescapeDataString( newReferenceRelativeUri.OriginalString ).Replace( "/", "\\" );

            newReferenceItem = xml.CreateElement( "ProjectReference" );
            var newReferenceAttribute = xml.CreateAttribute( "Include" );
            newReferenceAttribute.Value = newReferenceRelativePath;
            newReferenceItem.Attributes.Append( newReferenceAttribute );

            existingReferenceItem.ParentNode.InsertAfter( newReferenceItem, existingReferenceItem );
            xml.Save( project );

            context.Console.Out.WriteLine( "modified" );
        }
    }
}