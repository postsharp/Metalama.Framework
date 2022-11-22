// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using LINQPad;
using LINQPad.Extensibility.DataContext;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using Metalama.Framework.Workspaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.LinqPad
{
    /// <summary>
    /// A LinqPad driver that lets you query Metalama workspaces.
    /// </summary>
    public sealed class MetalamaDriver : DynamicDataContextDriver
    {
        public override string Name => "Metalama";

        public override string Author => "PostSharp Technologies";

        private readonly FacadeObjectFactory _facadeObjectFactory = new();

        public override string GetConnectionDescription( IConnectionInfo cxInfo )
        {
            // For static drivers, we can use the description of the custom type & its assembly:
            var connectionData = new ConnectionData( cxInfo );

            return connectionData.DisplayName;
        }

        public override bool ShowConnectionDialog( IConnectionInfo cxInfo, ConnectionDialogOptions dialogOptions )
        {
            // Prompt the user for a custom assembly and type name:
            var dialog = new ConnectionDialog( cxInfo );

            return dialog.ShowDialog() == true;
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(
            IConnectionInfo cxInfo,
            AssemblyName assemblyToBuild,
            ref string nameSpace,
            ref string typeName )
        {
            var connectionData = new ConnectionData( cxInfo );

            var escapedPath = connectionData.Project.ReplaceOrdinal( "\"", "\"\"" );

            var source = $@"using System;
using System;
using System.Collections.Generic;
using Metalama.LinqPad;

namespace {nameSpace}
{{
    // The main typed data class. The user's queries subclass this, so they have easy access to all its members.
	public class {typeName} : {nameof(MetalamaDataContext)}
	{{
	    public {typeName}() : base( @""{escapedPath}"" )
		{{
		}}
        
	}}	
}}";

            Compile( source, assemblyToBuild.CodeBase!, cxInfo );

            var workspace = WorkspaceCollection.Default.Load( connectionData.Project );

            var schemaFactory = new SchemaFactory( FormatTypeName );
            var projectSchema = schemaFactory.GetSchema( workspace );

            return projectSchema;
        }

        public override IEnumerable<string> GetNamespacesToAdd( IConnectionInfo cxInfo )
            => new[] { "Metalama.Framework.Workspaces", "Metalama.Framework.Code", "Metalama.Framework.Code.Collections" };

        private static IReadOnlyList<string> GetAssembliesToAdd( bool addReferenceAssemblies, IConnectionInfo connectionInfo )
        {
            List<string> assembliesToReference = new();

            if ( addReferenceAssemblies )
            {
                assembliesToReference.AddRange( GetCoreFxReferenceAssemblies( connectionInfo ) );
            }

            // Metalama.LinqPad
            assembliesToReference.Add( typeof(MetalamaDriver).Assembly.Location );

            // Metalama.Framework
            assembliesToReference.Add( typeof(IDeclaration).Assembly.Location );

            // Metalama.Framework.Workspaces
            assembliesToReference.Add( typeof(Workspace).Assembly.Location );

            // Metalama.Framework.Inspection
            assembliesToReference.Add( typeof(IIntrospectionAspectInstance).Assembly.Location );

            // Metalama.Framework.Engine
            assembliesToReference.Add( typeof(AspectPipeline).Assembly.Location );

            return assembliesToReference;
        }

        public override IEnumerable<string> GetAssembliesToAdd( IConnectionInfo cxInfo ) => GetAssembliesToAdd( false, cxInfo );

        private static void Compile( string cSharpSourceCode, string outputFile, IConnectionInfo connectionInfo )
        {
            var assembliesToReference = GetAssembliesToAdd( true, connectionInfo );

            // CompileSource is a static helper method to compile C# source code using LINQPad's built-in Roslyn libraries.
            // If you prefer, you can add a NuGet reference to the Roslyn libraries and use them directly.
            var compileResult = CompileSource(
                new CompilationInput
                {
                    FilePathsToReference = assembliesToReference.ToArray(), OutputPath = outputFile, SourceCode = new[] { cSharpSourceCode }
                } );

            if ( compileResult.Errors.Length > 0 )
            {
                throw new AssertionFailedException( "Cannot compile typed context: " + compileResult.Errors[0] );
            }
        }

        public override ICustomMemberProvider? GetCustomDisplayMemberProvider( object objectToWrite ) => this._facadeObjectFactory.GetFacade( objectToWrite );

        public override void InitializeContext( IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager )
        {
            Util.HtmlHead.AddStyles( "a.error { color: red !important; } span.null, .empty { color: #888 !important; }" );

            base.InitializeContext( cxInfo, context, executionManager );
        }
    }
}