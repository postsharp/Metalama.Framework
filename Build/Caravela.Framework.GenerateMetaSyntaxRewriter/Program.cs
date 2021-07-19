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

namespace Caravela.Framework.GenerateMetaSyntaxRewriter
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
            var targetFile = Path.GetFullPath( "MetaSyntaxRewriter.g.cs" );
            Console.WriteLine( "Creating " + targetFile );

            using var writer = File.CreateText( targetFile );

            writer.WriteLine( "#pragma warning disable CS8669 // Nullability" );
            writer.WriteLine( "using System;" );
            writer.WriteLine( "using System.Linq;" );
            writer.WriteLine( "using System.Collections.Generic;" );
            writer.WriteLine( "using Microsoft.CodeAnalysis;" );
            writer.WriteLine( "using Microsoft.CodeAnalysis.CSharp;" );
            writer.WriteLine( "using Microsoft.CodeAnalysis.CSharp.Syntax;" );
            writer.WriteLine( "using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;" );
            writer.WriteLine();
            writer.WriteLine( "namespace Caravela.Framework.Impl.Templating" );
            writer.WriteLine( "{" );
            writer.WriteLine( "\tpartial class MetaSyntaxRewriter" );
            writer.WriteLine( "\t{" );

            var allFactoryMethods =
                typeof(SyntaxFactory).GetMethods( BindingFlags.Static | BindingFlags.Public ).ToArray();

            // Generate Visit* and Transform* methods.
            foreach ( var method in typeof(CSharpSyntaxRewriter).GetMethods( BindingFlags.Public | BindingFlags.Instance ).OrderBy( m => m.Name ) )
            {
                if ( !method.Name.StartsWith( "Visit", StringComparison.Ordinal ) || method.ReturnType != typeof(SyntaxNode) )
                {
                    continue;
                }

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
                    Console.WriteLine( $"Cannot find factory method {factoryMethodName}." );

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

            // Generate Transform method (big switch).
            // writer.WriteLine( "\t\tprotected virtual ExpressionSyntax Transform(SyntaxNode node)" );
            // writer.WriteLine( "\t\t{" );
            // writer.WriteLine( "\t\t\tswitch ( node.Kind() )" );
            // writer.WriteLine( "\t\t\t{" );

            // var syntaxDocument = XElement.Load( "https://raw.githubusercontent.com/dotnet/roslyn/main/src/Compilers/CSharp/Portable/Syntax/Syntax.xml" );
            // foreach ( var node in syntaxDocument.Elements("Node"))
            // {
            //    foreach ( var kind in node.Elements("Kind") )
            //    {
            //        writer.WriteLine( $"\t\t\t\tcase SyntaxKind.{kind.Attribute( "Name" ).Value}: " );
            //    }

            // writer.WriteLine( $"\t\t\t\t\treturn this.Transform{node.Attribute("Name").Value.Replace("Syntax", "")}( ({node.Attribute("Name").Value}) node ) ;" );
            // }

            // writer.WriteLine( $"\t\t\t\tdefault: " );
            // writer.WriteLine( $"\t\t\t\t\tthrow new AssertionFailedException();" );
            // writer.WriteLine( "\t\t\t}" );
            // writer.WriteLine( "\t\t}" );

            // Generate MetaSyntaxFactoryImpl.
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
                                    "\t\t\t\t))" );
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
            writer.WriteLine( "}" );
            writer.WriteLine( "}" );
        }

        private static bool IsEnumerable( ParameterInfo parameter )
            => parameter.ParameterType.IsGenericType && parameter.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
    }
}