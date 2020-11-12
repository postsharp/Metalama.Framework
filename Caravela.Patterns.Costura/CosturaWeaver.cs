using Caravela.Framework.Project;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;

[assembly: CompileTime]

namespace Caravela.Patterns.Costura
{
    [AspectWeaver( typeof( CosturaAspect ) )]
    public class CosturaWeaver : IAspectWeaver
    {
        public CSharpCompilation Transform( AspectWeaverContext context )
        {
            var compilation = context.Compilation;

            if (compilation.LanguageVersion < LanguageVersion.CSharp9)
            {
                context.Diagnostics.AddDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "CO001", "Language version too low", "RoslynEx.Constura requires language version at least 9.0, but it's set to {0}.", "Caravela.Patterns.Costura", DiagnosticSeverity.Error, true),
                    null, new object[] { compilation.LanguageVersion.ToDisplayString() }));
                return context.Compilation;
            }

            var attributeData = compilation.Assembly.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToString() == typeof(CosturaAspect).FullName);
            if (attributeData == null)
                return context.Compilation;

            var config = Configuration.Read(attributeData, context.Diagnostics);

            var excludedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Reference Assemblies\Microsoft\Framework\.NETFramework");

            string[] paths = context.Compilation.References.Select(r => r switch { 
                PortableExecutableReference peReference => peReference.FilePath,
                _ => throw new NotSupportedException()
            })
                .Where(path => !path.StartsWith(excludedPath))
                .ToArray();

            var parseOptions = new CSharpParseOptions(compilation.LanguageVersion);

            // Embed resources:
            var checksums = new Checksums();
            var resourceEmbedder = new ResourceEmbedder(context);
            resourceEmbedder.EmbedResources(config, paths, checksums);
            bool unmanagedFromEmbedder = resourceEmbedder.HasUnmanaged;

            // Load references:
            AssemblyLoaderInfo info = AssemblyLoaderInfo.LoadAssemblyLoader(config.CreateTemporaryAssemblies, unmanagedFromEmbedder, ref compilation, parseOptions);

            // Alter code:
            string resourcesHash = ResourceHash.CalculateHash(resourceEmbedder.Resources);
            new AttachCallSynthesis().SynthesizeCallToAttach(ref compilation, parseOptions, info);
            new ResourceNameFinder(info, resourceEmbedder.Resources.Select(r => r.Name)).FillInStaticConstructor(
                config.CreateTemporaryAssemblies,
                config.PreloadOrder,
                resourcesHash,
                checksums);

            compilation = compilation.AddSyntaxTrees(SyntaxFactory.SyntaxTree(info.SourceType, parseOptions));

            return compilation;
        }
    }
}
