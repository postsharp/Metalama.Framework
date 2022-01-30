// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter
{
    internal class Program
    {
        private static string RemoveSuffix( string s, string suffix )
            => s.EndsWith( suffix, StringComparison.Ordinal ) ? s.Substring( 0, s.Length - suffix.Length ) : s;

        // private static string RemovePrefix( string s, string prefix )
        // {
        //    return s.Substring( prefix.Length );
        // }

        private static void Main()
        {
            GenerateTemplateFiles();
            GenerateHasher( "RunTimeCodeHasher.g.cs", "RunTimeCodeHasher", false );
            GenerateHasher( "CompileTimeCodeHasher.g.cs", "CompileTimeCodeHasher", true );
        }

        private static void GenerateTemplateFiles()
        {
            var targetFile = Path.GetFullPath( "MetaSyntaxRewriter.g.cs" );
            Console.WriteLine( "Creating " + targetFile );

            using var writer = File.CreateText( targetFile );

            WriteUsings( writer );
            writer.WriteLine();
            writer.WriteLine( "namespace Metalama.Framework.Engine.Templating" );
            writer.WriteLine( "{" );
            GenerateMetaSyntaxRewriter( writer );

            // Generate MetaSyntaxFactoryImpl.
            GenerateMetaSyntaxFactory( writer );
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

        private static void GenerateMetaSyntaxRewriter( StreamWriter writer )
        {
            writer.WriteLine( "\tpartial class MetaSyntaxRewriter" );
            writer.WriteLine( "\t{" );

            var allFactoryMethods =
                typeof(SyntaxFactory).GetMethods( BindingFlags.Static | BindingFlags.Public ).ToArray();

            // Generate Visit* and Transform* methods.
            var visitMethods = typeof(CSharpSyntaxRewriter).GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .OrderBy( m => m.Name )
                .Where( method => method.Name.StartsWith( "Visit", StringComparison.Ordinal ) && method.ReturnType == typeof(SyntaxNode) );

            foreach ( var method in visitMethods )
            {
                var nodeType = method.GetParameters()[0].ParameterType;
                var nodeTypeName = nodeType.Name;
                var factoryMethodName = RemoveSuffix( nodeTypeName, "Syntax" );

                var factoryMethod = allFactoryMethods.Where( m => m.Name == factoryMethodName )
                    .OrderByDescending( m => m.GetParameters().Length )
                    .ThenByDescending(
                        delegate( MethodInfo m )
                        {
                            // Prefer tokens to strings and lists to arrays.
                            var p = m.GetParameters();

                            if ( p.Length == 0 )
                            {
                                return 0;
                            }

                            return p[0].ParameterType == typeof(string) || p[0].ParameterType.IsArray ? 1 : 2;
                        } )
                    .FirstOrDefault();

                if ( factoryMethod == null )
                {
                    if ( factoryMethodName != "SyntaxNode" )
                    {
                        Console.WriteLine( $"Cannot find factory method {factoryMethodName}." );
                    }

                    continue;
                }

                var parameters = factoryMethod.GetParameters();

                var properties = nodeType.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                    .ToDictionary( p => p.Name, StringComparer.OrdinalIgnoreCase );

                string FindProperty( ParameterInfo parameter )
                {
                    if ( string.Equals( parameter.Name, "kind", StringComparison.Ordinal ) )
                    {
                        return "node.Kind()";
                    }

                    if ( !properties.TryGetValue( parameter.Name, out var property ) )
                    {
                        Console.WriteLine( $"Cannot find property {parameter.Name} in type {nodeTypeName}. Expected type is {parameter.ParameterType}." );

                        return "default";
                    }

                    if ( property.PropertyType != parameter.ParameterType )
                    {
                        Console.WriteLine(
                            $"Parameter {factoryMethodName}.{parameter.Name} is of type {parameter.ParameterType}, but the property is of type {property.PropertyType}." );
                    }

                    return $"node.{property.Name}";
                }

                // Generate the Visit method.
                writer.WriteLine( $"\t\t[ExcludeFromCodeCoverage]" );
                writer.WriteLine( $"\t\tpublic override SyntaxNode {method.Name}( {nodeTypeName} node )" );
                writer.WriteLine( "\t\t{" );
                writer.WriteLine( "\t\t\tswitch ( this.GetTransformationKind( node ) ) " );
                writer.WriteLine( "\t\t\t{" );

                // Generating Clone is interesting to validate the current code generation, but it is not used.
                // writer.WriteLine( "\t\t\t\tcase TransformationKind.Clone: " );
                // writer.WriteLine( $"\t\t\t\t\treturn {factoryMethod.Name}( {string.Join( ", ", parameters.Select( FindProperty ) )});" );
                writer.WriteLine( "\t\t\t\tcase TransformationKind.Transform: " );
                writer.WriteLine( $"\t\t\t\t\treturn this.Transform{factoryMethodName}( node );" );

                writer.WriteLine( "\t\t\t\tdefault: " );
                writer.WriteLine( $"\t\t\t\t\treturn base.{method.Name}( node );" );
                writer.WriteLine( "\t\t\t}" );
                writer.WriteLine( "\t\t}" );

                // Generate the Transform* method.
                writer.WriteLine( $"\t\t[ExcludeFromCodeCoverage]" );
                writer.WriteLine( $"\t\tprotected virtual ExpressionSyntax Transform{factoryMethodName}( {nodeTypeName} node)" );
                writer.WriteLine( "\t\t{" );
                writer.WriteLine( "\t\t\tthis.Indent();" );
                writer.Write( $"\t\t\tvar result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof({factoryMethodName})))" );

                if ( parameters.Length == 0 )
                {
                    writer.WriteLine( ";" );
                }
                else
                {
                    writer.WriteLine( ".WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{" );

                    for ( var i = 0; i < parameters.Length; i++ )
                    {
                        if ( i > 0 )
                        {
                            writer.WriteLine( "\t\t\tToken(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak())," );
                        }

                        writer.WriteLine( $"\t\t\tArgument(this.Transform({FindProperty( parameters[i] )})).WithLeadingTrivia(this.GetIndentation())," );
                    }

                    writer.WriteLine( "\t\t\t})));" );
                }

                writer.WriteLine( "\t\t\tthis.Unindent();" );
                writer.WriteLine( "\t\t\treturn result;" );
                writer.WriteLine( "\t\t}" );
            }
        }

        private static void GenerateMetaSyntaxFactory( StreamWriter writer )
        {
            writer.WriteLine( "\tpartial class MetaSyntaxFactoryImpl" );
            writer.WriteLine( "\t{" );

            foreach ( var methodGroup in typeof(SyntaxFactory).GetMethods( BindingFlags.Public | BindingFlags.Static )
                         .OrderBy( m => m.Name )
                         .GroupBy( m => m.Name ) )
            {
                MethodInfo SelectBestMethod( IEnumerable<MethodInfo> methods )
                {
                    return methods
                        .OrderBy( m => m.GetParameters().LastOrDefault()?.IsDefined( typeof(ParamArrayAttribute) ) ?? false ? 0 : 1 )
                        .First();
                }

                foreach ( var methodsWithSameParameterCount in methodGroup
                             .Select( m => (Method: m, Parameters: string.Join( ",", m.GetParameters().Select( p => p.Name ) )) )
                             .GroupBy( m => m.Parameters, m => m.Method )
                             .Select( SelectBestMethod )
                             .GroupBy( m => m.GetParameters().Length ) )
                {
                    void WriteSyntaxFactoryMethod( MethodInfo method, string? suffix = null )
                    {
                        ParameterInfo? paramsParameter = null;

                        writer.WriteLine( $"\t\t[ExcludeFromCodeCoverage]" );
                        writer.Write( $"\t\tpublic InvocationExpressionSyntax {method.Name}{suffix}" );

                        if ( method.IsGenericMethodDefinition )
                        {
                            writer.Write( "<" );

                            foreach ( var genericArgument in method.GetGenericArguments() )
                            {
                                if ( genericArgument.GenericParameterPosition > 0 )
                                {
                                    writer.Write( ", " );
                                }

                                writer.Write( genericArgument.Name );
                            }

                            writer.Write( ">" );
                        }

                        writer.Write( "(" );

                        foreach ( var parameter in method.GetParameters() )
                        {
                            if ( parameter.Position > 0 )
                            {
                                writer.Write( ", " );
                            }

                            if ( parameter.IsDefined( typeof(ParamArrayAttribute) ) )
                            {
                                writer.Write( "params " );
                            }

                            if ( IsEnumerable( parameter ) )
                            {
                                writer.Write( "IEnumerable<ExpressionSyntax>" );
                            }
                            else
                            {
                                writer.Write( "ExpressionSyntax" );
                            }

                            if ( parameter.IsDefined( typeof(ParamArrayAttribute) ) )
                            {
                                paramsParameter = parameter;
                                writer.Write( "[]" );
                            }

                            writer.Write( " @" + parameter.Name );
                        }

                        writer.WriteLine( ")" );

                        string factoryMethod;

                        if ( method.IsGenericMethod )
                        {
                            var genericArguments = string.Join( ", ", method.GetGenericArguments().Select( t => $"this.Type(typeof({t.Name}))" ) );
                            factoryMethod = $"this.GenericSyntaxFactoryMethod( \"{method.Name}\", {genericArguments} )";
                        }
                        else
                        {
                            factoryMethod = $"this.SyntaxFactoryMethod( \"{method.Name}\" )";
                        }

                        writer.Write(
                            $"\t\t\t=> SyntaxFactory.InvocationExpression( {factoryMethod}, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{{" );

                        foreach ( var parameter in method.GetParameters() )
                        {
                            if ( parameter == paramsParameter )
                            {
                                continue;
                            }

                            if ( parameter.Position > 0 )
                            {
                                writer.WriteLine( ", " );
                            }
                            else
                            {
                                writer.WriteLine();
                            }

                            if ( IsEnumerable( parameter ) )
                            {
                                var itemType = parameter.ParameterType.GenericTypeArguments[0];
                                var typeOfItemType = $"typeof({itemType.Name})";

                                writer.Write(
                                    "\t\t\t\tSyntaxFactory.Argument(SyntaxFactory.ArrayCreationExpression( \n" +
                                    $"\t\t\t\t\tSyntaxFactory.ArrayType( this.Type({typeOfItemType}) ).WithRankSpecifiers(SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression() ) ) ) ), \n"
                                    +
                                    $"\t\t\t\t\tSyntaxFactory.InitializerExpression( SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList( @{parameter.Name} ))\n"
                                    +
                                    "\t\t\t\t)).NormalizeWhitespace()" );
                            }
                            else
                            {
                                writer.Write( $"\t\t\t\tSyntaxFactory.Argument( @{parameter.Name} )" );
                            }
                        }

                        writer.Write( "}))" );

                        if ( paramsParameter != null )
                        {
                            writer.WriteLine( $"\t\t\t\t.AddArguments( @{paramsParameter.Name}.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )" );
                        }

                        writer.WriteLine( ");" );
                        writer.WriteLine();
                    }

                    if ( methodsWithSameParameterCount.Count() == 1 )
                    {
                        var method = methodsWithSameParameterCount.Single();
                        WriteSyntaxFactoryMethod( method );
                    }
                    else
                    {
                        var i = 1;

                        foreach ( var method in methodsWithSameParameterCount.OrderBy( m => m.ToString() ) )
                        {
                            WriteSyntaxFactoryMethod( method, i++.ToString( CultureInfo.InvariantCulture ) );
                        }
                    }
                }
            }

            writer.WriteLine( "}" );
        }

        private static void GenerateHasher( string path, string className, bool isCompileTime )
        {
            var syntaxDocument = XElement.Load( "Syntax-4.0.1.xml" );
            var nodes = syntaxDocument.Elements( "Node" ).ToDictionary( x => x.Attribute( "Name" ).Value, x => x );

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

            var visitMethods = typeof(CSharpSyntaxRewriter).GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .OrderBy( m => m.Name )
                .Where( method => method.Name.StartsWith( "Visit", StringComparison.Ordinal ) && method.ReturnType == typeof(SyntaxNode) );

            foreach ( var visitMethod in visitMethods )
            {
                var nodeType = visitMethod.GetParameters()[0].ParameterType.Name;

                if ( nodeType == "SyntaxNode" )
                {
                    continue;
                }

                var node = nodes[nodeType];

                writer.WriteLine( $"\t\tpublic override void {visitMethod.Name}( {nodeType} node )" );
                writer.WriteLine( "\t\t{" );

                void ProcessField( XElement field, bool isInChoice )
                {
                    var fieldName = field.Attribute( "Name" ).Value;
                    var fieldType = field.Attribute( "Type" ).Value;
                    var fieldIsOptional = field.Attribute( "Optional" )?.Value == "true";

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
                        var fieldTokenKinds = field.Elements( "Kind" ).Select( k => k.Attribute( "Name" ).Value ).ToArray();
                        var isTrivialToken = fieldTokenKinds.All( IsTrivialToken );

                        if ( !isInChoice && fieldTokenKinds.Length == 1 && isTrivialToken && !fieldIsOptional )
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

                void ProcessParent( XElement parent, bool isInChoice )
                {
                    foreach ( var field in parent.Elements( "Field" ) )
                    {
                        ProcessField( field, isInChoice );
                    }

                    foreach ( var choice in parent.Elements( "Choice" ) )
                    {
                        ProcessParent( choice, true );
                    }

                    foreach ( var sequence in parent.Elements( "Sequence" ) )
                    {
                        ProcessParent( sequence, true );
                    }
                }

                ProcessParent( node, false );

                writer.WriteLine( "\t\t}" ); // End of method
            }

            writer.WriteLine( "\t}" ); // End of class
            writer.WriteLine( "}" );   // End of namespace
        }

        private static bool IsEnumerable( ParameterInfo parameter )
            => parameter.ParameterType.IsGenericType && parameter.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);

        private static bool IgnoreFieldInRunTimeCode( string fieldType )
            => fieldType switch
            {
                "BlockSyntax" => true,
                "ArrowExpressionClauseSyntax" => true,
                "EqualsValueClauseSyntax" => true,
                _ => false
            };

        private static bool IsTrivialToken( string tokenKind )
            => tokenKind switch
            {
                "StringLiteralToken" => false,
                "CharacterLiteralToken" => false,
                "NumericLiteralToken" => false,
                "IdentifierToken" => false,
                _ => true
            };
    }
}