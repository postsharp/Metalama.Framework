using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Patterns.Costura
{
    public class ResourceNameFinder
    {
        private readonly AssemblyLoaderInfo info;
        private readonly IEnumerable<string> resourceNames;

        public ResourceNameFinder(AssemblyLoaderInfo info, IEnumerable<string> resourceNames)
        {
            this.info = info;
            this.resourceNames = resourceNames;
        }
        
        public void FillInStaticConstructor(bool createTemporaryAssemblies, string[] preloadOrder, string resourcesHash, Checksums checksums)
        {
            var statements = new List<StatementSyntax>();

            var orderedResources = preloadOrder
                .Join(this.resourceNames, p => p.ToLowerInvariant(),
                    r =>
                    {
                        var parts = r.Split('.');
                        GetNameAndExt(parts, out var name, out _);
                        return name;
                    }, (s, r) => r)
                .Union(this.resourceNames.OrderBy(r => r));
        
            foreach (var resource in orderedResources)
            {
                var parts = resource.Split('.');
        
                GetNameAndExt(parts, out var name, out var ext);
        
                if (string.Equals(parts[0], "costura", StringComparison.OrdinalIgnoreCase))
                {
                    if (createTemporaryAssemblies)
                    {
                        AddToList(statements, info.PreloadListField, resource);
                    }
                    else
                    {
                        if (string.Equals(ext, "pdb", StringComparison.OrdinalIgnoreCase))
                        {
                            AddToDictionary(statements, info.SymbolNamesField, name, resource);
                        }
                        else
                        {
                            AddToDictionary(statements, info.AssemblyNamesField, name, resource);
                        }
                    }
                }
                else if (string.Equals(parts[0], "costura32", StringComparison.OrdinalIgnoreCase))
                {
                    AddToList(statements, info.Preload32ListField, resource);
                }
                else if (string.Equals(parts[0], "costura64", StringComparison.OrdinalIgnoreCase))
                {
                    AddToList(statements, info.Preload64ListField, resource);
                }
            }

            if (info.ChecksumsField != null)
            {
                foreach (var checksum in checksums.AllChecksums)
                {
                    AddToDictionary(statements, info.ChecksumsField, checksum.Key, checksum.Value);
                }
            }

            if (info.Md5HashField != null)
            {
                statements.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(info.Md5HashField), LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(resourcesHash)))));
            }

            var staticConstructor = ConstructorDeclaration(info.SourceTypeName).AddModifiers(Token(SyntaxKind.StaticKeyword)).WithBody(Block(statements));
            info.SourceType = info.SourceType.InsertNodesAfter(info.SourceType.DescendantNodes().OfType<ClassDeclarationSyntax>().Single().Members.Last(), new[] { staticConstructor });
        }
        
        static void GetNameAndExt(string[] parts, out string name, out string ext)
        {
            var isCompressed = string.Equals(parts[parts.Length - 1], "compressed", StringComparison.OrdinalIgnoreCase);
        
            ext = parts[parts.Length - (isCompressed ? 2 : 1)];
        
            name = string.Join(".", parts.Skip(1).Take(parts.Length - (isCompressed ? 3 : 2)));
        }
        
        void AddToDictionary(List<StatementSyntax> statements, string field, string key, string name)
        {
            statements.Add(ExpressionStatement(
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(field), IdentifierName("Add")))
                    .AddArgumentListArguments(
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(key))),
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(name))))));
        }
        
        void AddToList(List<StatementSyntax> statements, string field, string name)
        {
            statements.Add(ExpressionStatement(
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(field), IdentifierName("Add")))
                    .AddArgumentListArguments(
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(name))))));
        }
    }
}