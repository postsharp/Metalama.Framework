using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Patterns.Costura
{
    public class AssemblyLoaderInfo
    {
        public string AssemblyNamesField { get; private set; }
        public string SymbolNamesField { get; private set; }
        public string PreloadListField { get; private set; }
        public string Preload32ListField { get; private set; }
        public string Preload64ListField { get; private set; }
        public string ChecksumsField { get; private set; }
        public string Md5HashField { get; private set; }

        public CompilationUnitSyntax SourceType { get; set; }
        public string SourceTypeName { get; private set; }

        public static AssemblyLoaderInfo LoadAssemblyLoader(
            bool createTemporaryAssemblies, bool hasUnmanaged, ref CSharpCompilation compilation, CSharpParseOptions parseOptions)
        {
            compilation = compilation.AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(Resources.Common, parseOptions));

            AssemblyLoaderInfo info = new AssemblyLoaderInfo();
            string sourceTypeCode;
            if (createTemporaryAssemblies)
            {
                sourceTypeCode = Resources.TemplateWithTempAssembly;
                info.SourceTypeName = "TemplateWithTempAssembly";
            }
            else if (hasUnmanaged)
            {
                sourceTypeCode = Resources.TemplateWithUnmanagedHandler;
                info.SourceTypeName = "TemplateWithUnmanagedHandler";
            }
            else
            {
                sourceTypeCode = Resources.Template;
                info.SourceTypeName = "Template";
            }

            info.SourceType = SyntaxFactory.ParseCompilationUnit(sourceTypeCode/*, options: parseOptions*/);

            info.AssemblyNamesField = "assemblyNames";
            info.SymbolNamesField = "symbolNames";
            info.PreloadListField = "preloadList";
            info.Preload32ListField = "preload32List";
            info.Preload64ListField = "preload64List";
            info.ChecksumsField = Optional("checksums");
            info.Md5HashField = Optional("md5Hash");
            return info;

            string Optional(string field) =>
                info.SourceType.DescendantNodes().OfType<FieldDeclarationSyntax>()
                    .SelectMany(f => f.Declaration.Variables)
                    .Any(f => f.Identifier.ValueText == field)
                    ? field : null;
        }
    }
}