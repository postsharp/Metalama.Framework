// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using LINQPad;
using LINQPad.Extensibility.DataContext;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Introspection;
using Metalama.Framework.Workspaces;
using System;
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
            return new ConnectionDialog( cxInfo ).ShowDialog() == true;
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(
            IConnectionInfo cxInfo,
            AssemblyName assemblyToBuild,
            ref string nameSpace,
            ref string typeName )
        {
            var connectionData = new ConnectionData( cxInfo );

            var source = $@"using System;
using System;
using System.Collections.Generic;
using Metalama.LinqPad;

namespace {nameSpace}
{{
    // The main typed data class. The user's queries subclass this, so they have easy access to all its members.
	public class {typeName} : MetalamaDataContext
	{{
	    public {typeName}() : base( @""{connectionData.Project}"" )
		{{
		}}
        
	}}	
}}";

            Compile( source, assemblyToBuild.CodeBase!, cxInfo );

            var projectSchema = GetSchema( "workspace.", typeof(IProjectSet) );

            var metalamaOutputItem =
                new ExplorerItem( "MetalamaOutput", ExplorerItemKind.Property, ExplorerIcon.Schema )
                {
                    Children = GetSchema( "workspace.MetalamaOutput.", typeof(IMetalamaCompilationSet) )
                };

            projectSchema.Add( metalamaOutputItem );

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

        private static List<ExplorerItem> GetSchema( string prefix, Type type )
        {
            // Return the objects with which to populate the Schema Explorer by reflecting over customType.

            // We'll start by retrieving all the properties of the custom type that implement IEnumerable<T>:
            var topLevelProps =
            (
                from property in GetProperties( type )
                where property.PropertyType != typeof(string)
                let enumerableType = GetIEnumerable( property.PropertyType ).FirstOrDefault()
                where enumerableType != null
                orderby property.Name
                select new ExplorerItem( property.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table )
                {
                    IsEnumerable = true,
                    DragText = prefix + property.Name,
                    ToolTipText = FormatTypeName( property.PropertyType, false ),

                    // Store the entity type to the Tag property. We'll use it later.
                    Tag = enumerableType.GetGenericArguments()[0]
                }
            ).ToList();

            // Create a lookup keying each element type to the properties of that type. This will allow
            // us to build hyperlink targets allowing the user to click between associations:
            var elementTypeLookup = topLevelProps.ToLookup( tp => (Type) tp.Tag );

            // Populate the columns (properties) of each entity:
            foreach ( var table in topLevelProps )
            {
                var parentType = (Type) table.Tag;

                var props = GetProperties( parentType )
                    .OrderBy( p => (p.Name, p.PropertyType), PropertyComparer.Instance )
                    .Select( p => GetChildItem( elementTypeLookup, p.Name, p.PropertyType ) );

                table.Children = props.ToList();
            }

            return topLevelProps;
        }

        private static IEnumerable<Type> GetIEnumerable( Type type )
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? new[] { type }
                : type.GetInterfaces().Where( t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>) );

        private static ExplorerItem GetChildItem( ILookup<Type, ExplorerItem> elementTypeLookup, string childPropName, Type childPropType )
        {
            // If the property's type is in our list of entities, then it's a Many:1 (or 1:1) reference.
            // We'll assume it's a Many:1 (we can't reliably identify 1:1s purely from reflection).
            if ( elementTypeLookup.Contains( childPropType ) )
            {
                return new ExplorerItem( childPropName, ExplorerItemKind.ReferenceLink, ExplorerIcon.ManyToOne )
                {
                    HyperlinkTarget = elementTypeLookup[childPropType].First(),

                    // FormatTypeName is a helper method that returns a nicely formatted type name.
                    ToolTipText = FormatTypeName( childPropType, true )
                };
            }

            // Is the property's type a collection of entities?
            var enumerableType = GetIEnumerable( childPropType ).FirstOrDefault();

            if ( enumerableType != null )
            {
                var elementType = enumerableType.GetGenericArguments()[0];

                if ( elementTypeLookup.Contains( elementType ) )
                {
                    return new ExplorerItem( childPropName, ExplorerItemKind.CollectionLink, ExplorerIcon.OneToMany )
                    {
                        HyperlinkTarget = elementTypeLookup[elementType].First(), ToolTipText = FormatTypeName( elementType, true )
                    };
                }
            }

            // Ordinary property:
            return new ExplorerItem(
                childPropName + " (" + FormatTypeName( childPropType, false ) + ")",
                ExplorerItemKind.Property,
                ExplorerIcon.Column );
        }

        private static IReadOnlyList<PropertyInfo> GetProperties( Type type )
        {
            if ( type.IsInterface )
            {
                var properties = new Dictionary<string, PropertyInfo>();

                // Properties defined in the top interface win.
                foreach ( var property in type.GetProperties() )
                {
                    properties[property.Name] = property;
                }

                // For base interfaces, we don't know which ones are the deepest, so the result is random.
                foreach ( var property in type.GetInterfaces().SelectMany( i => i.GetProperties() ) )
                {
                    properties[property.Name] = property;
                }

                return properties.Values.ToArray();
            }
            else
            {
                return type.GetProperties();
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