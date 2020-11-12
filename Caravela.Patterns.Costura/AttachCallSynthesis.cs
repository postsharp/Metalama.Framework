using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Patterns.Costura
{
    public class AttachCallSynthesis
    {
        public void SynthesizeCallToAttach(ref CSharpCompilation compilation, CSharpParseOptions parseOptions, AssemblyLoaderInfo assemblyLoaderInfo)
        {
            string code = Resources.ModuleInitializer.Replace("TEMPLATE", assemblyLoaderInfo.SourceTypeName);
            
            compilation = compilation.AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(code, parseOptions));
        }
    }
}