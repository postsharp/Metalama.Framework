using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.GenerateMetaSyntaxRewriter
{
    class Program
    {
        private static string RemoveSuffix(string s, string suffix)
        {
            return s.EndsWith(suffix) ? s.Substring(0, s.Length - suffix.Length) : s;
        }
        static void Main()
        {
            using var writer = File.CreateText("MetaSyntaxRewriter.g.cs");
            
            writer.WriteLine("using Microsoft.CodeAnalysis;");
            writer.WriteLine("using Microsoft.CodeAnalysis.CSharp;");
            writer.WriteLine("using Microsoft.CodeAnalysis.CSharp.Syntax;");
            writer.WriteLine("using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;");
            writer.WriteLine();
            writer.WriteLine("namespace Caravela.Framework.Impl.Templating");
            writer.WriteLine("{");
            writer.WriteLine("\tpartial class MetaSyntaxRewriter");
            writer.WriteLine("\t{");

            var allFactoryMethods =
                typeof(SyntaxFactory).GetMethods(BindingFlags.Static | BindingFlags.Public).ToArray();
            
            foreach (var method in typeof(CSharpSyntaxRewriter).GetMethods(BindingFlags.Public | BindingFlags.Instance).OrderBy(m => m.Name))
            {
                if (!method.Name.StartsWith("Visit") || method.ReturnType != typeof(SyntaxNode))
                {
                    continue;
                }

                var nodeType = method.GetParameters()[0].ParameterType;
                var nodeTypeName = nodeType.Name;
                var factoryMethodName = RemoveSuffix(nodeTypeName, "Syntax");
                var factoryMethod = allFactoryMethods.Where(m => m.Name == factoryMethodName)
                    .OrderByDescending(m => m.GetParameters().Length)
                    .ThenByDescending(delegate(MethodInfo m)
                    {
                        // Prefer tokens to strings and lists to arrays.
                        var p = m.GetParameters();
                        if (p.Length == 0)
                        {
                            return 0;
                        }

                        return p[0].ParameterType == typeof(string) || p[0].ParameterType.IsArray ? 1 : 2;
                    })
                    .FirstOrDefault();

                if (factoryMethod == null)
                {
                    Console.WriteLine($"Cannot find factory method {factoryMethodName}.");
                    continue;
                }
                var parameters = factoryMethod.GetParameters();

                var properties = nodeType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .ToDictionary( p => p.Name, StringComparer.OrdinalIgnoreCase);
                
                string FindProperty(ParameterInfo parameter)
                {
                    if (parameter.Name == "kind")
                    {
                        return "node.Kind()";
                    }
                    /*
                    else if (nodeType == typeof(IdentifierNameSyntax) && parameter.Name == "name")
                    {
                        return "node.Identifier.Text";
                    }
                    else if (nodeType == typeof(XmlTextSyntax) && parameter.Name == "value")
                    {
                        XmlTextSyntax x;
                    }
                    */

                    if (!properties.TryGetValue(parameter.Name, out var property))
                    {
                        Console.WriteLine($"Cannot find property {parameter.Name} in type {nodeTypeName}. Expected type is {parameter.ParameterType}.");
                        return "default";
                    }
                    else
                    {
                        if (property.PropertyType != parameter.ParameterType)
                        {
                            Console.WriteLine($"Parameter {factoryMethodName}.{parameter.Name} is of type {parameter.ParameterType}, but the property is of type {property.PropertyType}.");
                        }
                        
                        return $"node.{property.Name}";
                    }
                }

                // Generate the Visit method.
                writer.WriteLine($"\t\tpublic override SyntaxNode {method.Name}( {nodeTypeName} node )");
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t\tswitch ( this.GetTransformationKind( node ) ) ");
                writer.WriteLine("\t\t\t{");
                writer.WriteLine("\t\t\t\tcase TransformationKind.Clone: ");
                writer.WriteLine($"\t\t\t\t\treturn {factoryMethod.Name}( {string.Join(", ", parameters.Select(FindProperty ) )});");
                writer.WriteLine("\t\t\t\tcase TransformationKind.Transform: ");
                writer.WriteLine($"\t\t\t\t\treturn this.Transform{factoryMethodName}( node );");
                writer.WriteLine("\t\t\t\tdefault: ");
                writer.WriteLine($"\t\t\t\t\treturn base.{method.Name}( node );");
                writer.WriteLine("\t\t\t}");
                writer.WriteLine("\t\t}");
                
                // Generate the Transform method.
                writer.WriteLine($"\t\tprotected virtual ExpressionSyntax Transform{factoryMethodName}( {nodeTypeName} node)");
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t\tthis.Indent();");
                writer.Write($"\t\t\tvar result = InvocationExpression(IdentifierName(nameof({factoryMethodName})))");
                if (parameters.Length == 0)
                {
                    writer.WriteLine(";");
                }
                else
                {
                    writer.WriteLine(
                        ".WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{");
                    for (var i = 0; i < parameters.Length; i++ )
                    {
                        if (i > 0)
                        {
                            writer.WriteLine("\t\t\tToken(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),");
                        }
                        
                        writer.WriteLine($"\t\t\tArgument(this.Transform({FindProperty(parameters[i])})).WithLeadingTrivia(this.GetIndentation()),");
                    }
                    writer.WriteLine("\t\t\t})));");
                }
                writer.WriteLine("\t\t\tthis.Unindent();");
                writer.WriteLine("\t\t\treturn result;");
                writer.WriteLine("\t\t}");
                
            }
            
            
            
            writer.WriteLine("}");
            writer.WriteLine("}");
        }
    }
}