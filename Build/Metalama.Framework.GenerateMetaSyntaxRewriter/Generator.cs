// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.GenerateMetaSyntaxRewriter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter;

internal class Generator
{
    private static string RemoveSuffix( string s, string suffix )
        => s.EndsWith( suffix, StringComparison.Ordinal ) ? s.Substring( 0, s.Length - suffix.Length ) : s;

    private readonly Tree _syntaxDocument;
    private readonly IDictionary<string, string?> _parentMap;

    private readonly IDictionary<string, Node> _nodeMap;

    public Generator()
    {
        this._syntaxDocument = TreeReader.ReadTree( "Syntax-4.0.1.xml" );
        this._nodeMap = this._syntaxDocument.Types.OfType<Node>().ToDictionary( n => n.Name );
        this._parentMap = this._syntaxDocument.Types.ToDictionary( n => n.Name, n => n.Base )!;
        this._parentMap.Add( this._syntaxDocument.Root, null );
    }

    public void GenerateTemplateFiles()
    {
        var targetFile = Path.GetFullPath( "MetaSyntaxRewriter.g.cs" );
        Console.WriteLine( "Creating " + targetFile );

        using var writer = File.CreateText( targetFile );

        WriteUsings( writer );
        writer.WriteLine();
        writer.WriteLine( "namespace Metalama.Framework.Engine.Templating" );
        writer.WriteLine( "{" );
        this.GenerateMetaSyntaxRewriter( writer );

        // Generate MetaSyntaxFactoryImpl.
        this.GenerateMetaSyntaxFactory( writer );
        writer.WriteLine( "}" );
        writer.WriteLine( "}" );
    }

    private static void WriteUsings( StreamWriter writer )
    {
        writer.WriteLine( "#pragma warning disable CS8669 // Nullability" );
        writer.WriteLine( "using System;" );
        writer.WriteLine( "using System.Linq;" );
        writer.WriteLine( "using System.Collections.Generic;" );
        writer.WriteLine( "using System.Diagnostics.CodeAnalysis;" );
        writer.WriteLine( "using Microsoft.CodeAnalysis;" );
        writer.WriteLine( "using Microsoft.CodeAnalysis.CSharp;" );
        writer.WriteLine( "using Microsoft.CodeAnalysis.CSharp.Syntax;" );
        writer.WriteLine( "using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;" );
    }

    private static bool IsAutoCreatableToken( Node node, Field field )
    {
        return field.Type == "SyntaxToken"
               && field.Kinds != null
               && ((field.Kinds.Count == 1 && field.Kinds[0].Name != "IdentifierToken"
                                           && !field.Kinds[0].Name.EndsWith( "LiteralToken", StringComparison.Ordinal ))
                   || (field.Kinds.Count > 1 && field.Kinds.Count == node.Kinds.Count));
    }

    private static IEnumerable<Field> DetermineRedFactoryWithNoAutoCreatableTokenFields( Node nd )
    {
        return nd.Fields.Where( f => !IsAutoCreatableToken( nd, f ) );
    }

    private Node? GetNode( string typeName ) => this._nodeMap.TryGetValue( typeName, out var node ) ? node : null;

    private static bool IsAnyList( string typeName )
    {
        return IsNodeList( typeName ) || IsSeparatedNodeList( typeName ) || typeName == "SyntaxNodeOrTokenList";
    }

    private bool CanBeAutoCreated( Node node, Field field ) => IsAutoCreatableToken( node, field ) || this.IsAutoCreatableNode( field );

    private bool IsAutoCreatableNode( Field field )
    {
        var referencedNode = this.GetNode( field.Type );

        return referencedNode != null && this.RequiredFactoryArgumentCount( referencedNode ) == 0;
    }

    private bool IsRequiredFactoryField( Node node, Field field )
    {
        return (!IsOptional( field ) && !IsAnyList( field.Type ) && !this.CanBeAutoCreated( node, field )) || this.IsValueField( field );
    }

    private int RequiredFactoryArgumentCount( Node nd, bool includeKind = true )
    {
        var count = 0;

        // kind must be specified in factory
        if ( nd.Kinds.Count > 1 && includeKind )
        {
            count++;
        }

        foreach ( var field in nd.Fields )
        {
            if ( this.IsRequiredFactoryField( nd, field ) )
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsOptional( Field f ) => IsTrue( f.Optional );

    private bool IsNode( string typeName )
    {
        return this._parentMap.ContainsKey( typeName );
    }

    private static bool IsTrue( string? val ) => val != null && string.Compare( val, "true", StringComparison.OrdinalIgnoreCase ) == 0;

    private bool IsValueField( Field field )
    {
        return !this.IsNodeOrNodeList( field.Type );
    }

    private bool IsNodeOrNodeList( string typeName )
    {
        return this.IsNode( typeName ) || IsNodeList( typeName ) || IsSeparatedNodeList( typeName ) || typeName == "SyntaxNodeOrTokenList";
    }

    private static bool IsNodeList( string typeName )
    {
        return typeName.StartsWith( "SyntaxList<", StringComparison.Ordinal );
    }

    private static bool IsSeparatedNodeList( string typeName )
    {
        return typeName.StartsWith( "SeparatedSyntaxList<", StringComparison.Ordinal );
    }

    private static string CommaJoin( params object[] values ) => Join( ", ", values );

    private static string Join( string separator, params object[] values )
        => string.Join(
            separator,
            values.SelectMany(
                v => (v switch
                {
                    string s => new[] { s },
                    IEnumerable<string> ss => ss,
                    _ => throw new InvalidOperationException( "Join must be passed strings or collections of strings" )
                }).Where( s => s != "" ) ) );

    private static string CamelCase( string name )
    {
        if ( char.IsUpper( name[0] ) )
        {
            name = char.ToLowerInvariant( name[0] ) + name.Substring( 1 );
        }

        return FixKeyword( name );
    }

    private static string FixKeyword( string name )
    {
        if ( IsKeyword( name ) )
        {
            return "@" + name;
        }

        return name;
    }

    private static bool IsKeyword( string name )
    {
        switch ( name )
        {
            case "bool":
            case "byte":
            case "sbyte":
            case "short":
            case "ushort":
            case "int":
            case "uint":
            case "long":
            case "ulong":
            case "double":
            case "float":
            case "decimal":
            case "string":
            case "char":
            case "object":
            case "typeof":
            case "sizeof":
            case "null":
            case "true":
            case "false":
            case "if":
            case "else":
            case "while":
            case "for":
            case "foreach":
            case "do":
            case "switch":
            case "case":
            case "default":
            case "lock":
            case "try":
            case "throw":
            case "catch":
            case "finally":
            case "goto":
            case "break":
            case "continue":
            case "return":
            case "public":
            case "protected":
            case "internal":
            case "private":
            case "static":
            case "readonly":
            case "sealed":
            case "const":
            case "new":
            case "override":
            case "abstract":
            case "virtual":
            case "partial":
            case "ref":
            case "out":
            case "in":
            case "where":
            case "params":
            case "this":
            case "base":
            case "namespace":
            case "using":
            case "class":
            case "struct":
            case "interface":
            case "delegate":
            case "checked":
            case "get":
            case "set":
            case "add":
            case "remove":
            case "operator":
            case "implicit":
            case "explicit":
            case "fixed":
            case "extern":
            case "event":
            case "enum":
            case "unsafe":
                return true;

            default:
                return false;
        }
    }

    public static bool IsAnyNodeList( string typeName )
    {
        return IsNodeList( typeName ) || IsSeparatedNodeList( typeName );
    }

    private void GenerateMetaSyntaxRewriter( StreamWriter writer )
    {
        writer.WriteLine( "\tpartial class MetaSyntaxRewriter" );
        writer.WriteLine( "\t{" );

        var nodes = this._syntaxDocument.Types.Where( n => n is not PredefinedNode ).OfType<Node>().ToList();

        foreach ( var node in nodes )
        {
            var nodeTypeName = node.Name;
            var factoryMethodName = RemoveSuffix( nodeTypeName, "Syntax" );

            // Generate the Visit method.
            writer.WriteLine( $"\t\t[ExcludeFromCodeCoverage]" );
            writer.WriteLine( $"\t\tpublic override SyntaxNode Visit{RemoveSuffix( node.Name, "Syntax" )}( {nodeTypeName} node )" );
            writer.WriteLine( "\t\t{" );
            writer.WriteLine( "\t\t\tswitch ( this.GetTransformationKind( node ) ) " );
            writer.WriteLine( "\t\t\t{" );

            // Generating Clone is interesting to validate the current code generation, but it is not used.
            // writer.WriteLine( "\t\t\t\tcase TransformationKind.Clone: " );
            // writer.WriteLine( $"\t\t\t\t\treturn {factoryMethod.Name}( {string.Join( ", ", parameters.Select( FindProperty ) )});" );
            writer.WriteLine( "\t\t\t\tcase TransformationKind.Transform: " );
            writer.WriteLine( $"\t\t\t\t\treturn this.Transform{factoryMethodName}( node );" );

            writer.WriteLine( "\t\t\t\tdefault: " );
            writer.WriteLine( $"\t\t\t\t\treturn base.Visit{RemoveSuffix( node.Name, "Syntax" )}( node );" );
            writer.WriteLine( "\t\t\t}" );
            writer.WriteLine( "\t\t}" );

            // Generate the Transform* method.
            writer.WriteLine( $"\t\t[ExcludeFromCodeCoverage]" );
            writer.WriteLine( $"\t\tprotected virtual ExpressionSyntax Transform{factoryMethodName}( {nodeTypeName} node)" );
            writer.WriteLine( "\t\t{" );
            writer.WriteLine( "\t\t\tthis.Indent();" );
            writer.Write( $"\t\t\tvar result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof({factoryMethodName})))" );

            var hasKindParameter = node.Kinds.Count > 1;

            if ( node.Fields.Count == 0 && !hasKindParameter )
            {
                writer.WriteLine( ";" );
            }
            else
            {
                writer.WriteLine( ".WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{" );

                var appendComma = false;

                if ( hasKindParameter )
                {
                    writer.WriteLine( $"\t\t\t\tArgument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation())," );
                    appendComma = true;
                }

                foreach ( var field in node.Fields )
                {
                    if ( appendComma )
                    {
                        writer.WriteLine( "\t\t\t\tToken(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak())," );
                    }
                    else
                    {
                        appendComma = true;
                    }

                    writer.WriteLine( $"\t\t\t\tArgument(this.Transform(node.{field.Name})).WithLeadingTrivia(this.GetIndentation())," );
                }

                writer.WriteLine( "\t\t\t})));" );
            }

            writer.WriteLine( "\t\t\tthis.Unindent();" );
            writer.WriteLine( "\t\t\treturn result;" );
            writer.WriteLine( "\t\t}" );
        }
    }

    private void GenerateMetaSyntaxFactory( StreamWriter writer )
    {
        writer.WriteLine( "\tpartial class MetaSyntaxFactoryImpl" );
        writer.WriteLine( "\t{" );

        var nodes = this._syntaxDocument.Types.Where( n => n is not PredefinedNode and not AbstractNode ).OfType<Node>().ToList();

        // Generate methods for all types of syntax nodes.
        foreach ( var node in nodes )
        {
            void WriteMethod( List<Field> fields )
            {
                writer.WriteLine( $"\t\t[ExcludeFromCodeCoverage]" );
                writer.Write( $"\t\tpublic InvocationExpressionSyntax {RemoveSuffix( node.Name, "Syntax" )}" );

                writer.Write( "(" );

                var hasKindParameter = node.Kinds.Count > 1;

                writer.Write(
                    CommaJoin(
                        hasKindParameter ? "ExpressionSyntax kind" : "",
                        fields.Select( f => $"ExpressionSyntax {CamelCase( f.Name )}" ) ) );

                writer.WriteLine( ")" );

                var factoryMethod = $"this.SyntaxFactoryMethod( \"{RemoveSuffix( node.Name, "Syntax" )}\" )";

                writer.WriteLine(
                    $"\t\t\t=> SyntaxFactory.InvocationExpression( {factoryMethod}, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{{" );

                var appendComma = false;

                if ( hasKindParameter )
                {
                    writer.WriteLine();
                    writer.Write( $"\t\t\t\t\tSyntaxFactory.Argument( kind )" );
                    appendComma = true;
                }

                foreach ( var field in fields )
                {
                    if ( appendComma )
                    {
                        writer.WriteLine( ", " );
                    }
                    else
                    {
                        appendComma = true;
                    }

                    writer.Write( $"\t\t\t\t\tSyntaxFactory.Argument( {CamelCase( field.Name )} )" );
                }

                writer.Write( "}))" );

                writer.WriteLine( ");" );
                writer.WriteLine();
            }

            WriteMethod( node.Fields );

            // Minimal factory.
            if ( node.Fields.Any( f => IsAutoCreatableToken( node, f ) ) )
            {
                var factoryWithNoAutoCreatableTokenFields = new HashSet<Field>( DetermineRedFactoryWithNoAutoCreatableTokenFields( node ) );
                var orderedMinimalFactoryFields = node.Fields.Where( factoryWithNoAutoCreatableTokenFields.Contains ).ToList();
                WriteMethod( orderedMinimalFactoryFields );
            }
        }

        writer.WriteLine( "}" );
    }

    public void GenerateHasher( string path, string className, bool isCompileTime )
    {
        Console.WriteLine( "Creating " + Path.GetFullPath( path ) );

        using var writer = File.CreateText( path );

        WriteUsings( writer );
        writer.WriteLine( "using K4os.Hash.xxHash;" );
        writer.WriteLine();
        writer.WriteLine( "namespace Metalama.Framework.DesignTime.Pipeline.Diff" );
        writer.WriteLine( "{" );
        writer.WriteLine( $"\tinternal class {className} : BaseCodeHasher" );
        writer.WriteLine( "\t{" );
        writer.WriteLine( $"\t\tpublic {className}(XXH64 hasher) : base(hasher) {{}}" );
        writer.WriteLine();

        var nodes = this._syntaxDocument.Types.Where( n => n is not PredefinedNode ).OfType<Node>().ToList();

        foreach ( var node in nodes )
        {
            var nodeType = node.Name;

            if ( nodeType == "SyntaxNode" )
            {
                continue;
            }

            writer.WriteLine( $"\t\t[ExcludeFromCodeCoverage]" );
            writer.WriteLine( $"\t\tpublic override void Visit{RemoveSuffix( nodeType, "Syntax" )}( {nodeType} node )" );
            writer.WriteLine( "\t\t{" );

            void ProcessField( Field field, bool isInChoice )
            {
                var fieldName = field.Name;
                var fieldType = field.Type;
                var fieldIsOptional = field.IsOptional;

                switch ( fieldType )
                {
                    case "bool":
                        return;
                }

                if ( !isCompileTime && IgnoreFieldInRunTimeCode( fieldType ) )
                {
                    writer.WriteLine( $"\t\t\t// Skipping {fieldName} because it is irrelevant at design time for non-compile-time code." );

                    return;
                }

                if ( fieldType == "SyntaxToken" )
                {
                    var fieldTokenKinds = field.Kinds;
                    var isTrivialToken = fieldTokenKinds.All( IsTrivialToken );

                    if ( !isInChoice && fieldTokenKinds.Count == 1 && isTrivialToken && !fieldIsOptional )
                    {
                        writer.WriteLine( $"\t\t\t// Skipping {fieldName} because it is trivial." );

                        return;
                    }
                    else if ( isTrivialToken )
                    {
                        writer.WriteLine( $"\t\t\tthis.VisitTrivialToken( node.{fieldName} );" );

                        return;
                    }
                    else
                    {
                        writer.WriteLine( $"\t\t\tthis.VisitNonTrivialToken( node.{fieldName} );" );

                        return;
                    }
                }

                writer.WriteLine( $"\t\t\tthis.Visit( node.{fieldName} );" );
            }

            void ProcessParent( List<TreeTypeChild> children, bool isInChoice )
            {
                foreach ( var field in children.OfType<Field>() )
                {
                    ProcessField( field, isInChoice );
                }

                foreach ( var choice in children.OfType<Choice>() )
                {
                    ProcessParent( choice.Children, true );
                }

                foreach ( var sequence in children.OfType<Sequence>() )
                {
                    ProcessParent( sequence.Children, true );
                }
            }

            ProcessParent( node.Children, false );

            writer.WriteLine( "\t\t}" ); // End of method
        }

        writer.WriteLine( "\t}" ); // End of class
        writer.WriteLine( "}" );   // End of namespace
    }

    private static bool IgnoreFieldInRunTimeCode( string fieldType )
        => fieldType switch
        {
            "BlockSyntax" => true,
            "ArrowExpressionClauseSyntax" => true,
            "EqualsValueClauseSyntax" => true,
            _ => false
        };

    private static bool IsTrivialToken( Kind tokenKind )
        => tokenKind.Name switch
        {
            "StringLiteralToken" => false,
            "CharacterLiteralToken" => false,
            "NumericLiteralToken" => false,
            "IdentifierToken" => false,
            _ => true
        };
}