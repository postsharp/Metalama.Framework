using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynPad.Roslyn;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Caravela.AspectWorkbench.CodeEditor
{
    public class CustomRoslynHost : RoslynHost
    {
        public static CustomRoslynHost Create()
        {
            var host = new CustomRoslynHost(
                additionalAssemblies: new[]
                {
                    Assembly.Load( "RoslynPad.Roslyn.Windows" ),
                    Assembly.Load( "RoslynPad.Editor.Windows" ),
                },
                references: RoslynHostReferences.Empty
                .With(
                    assemblyReferences: new[]
                    {
                        typeof(object).Assembly, typeof(DateTime).Assembly, typeof(Enumerable).Assembly,
                        typeof(Console).Assembly,
                        typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly,
                        typeof(SyntaxFactory).Assembly,
                        typeof(Framework.Aspects.TemplateContext).Assembly,
                        typeof(Framework.Impl.Templating.TemplateSyntaxFactory).Assembly
                    },
                    imports: new[]
                    {
                        "Caravela.Framework.Aspects",
                        "Caravela.Framework.Aspects.TemplateContext"
                    }
                )
            );

            return host;
        }

        public CustomRoslynHost( IEnumerable<Assembly> additionalAssemblies = null, RoslynHostReferences references = null ) : base( additionalAssemblies, references )
        {
        }

        protected override Project CreateProject( Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project previousProject = null )
        {
            string name = args.Name ?? "Template";
            ProjectId id = ProjectId.CreateNewId( name );

            CSharpParseOptions parseOptions = new CSharpParseOptions( kind: SourceCodeKind.Script, languageVersion: LanguageVersion.Latest );

            solution = solution.AddProject( ProjectInfo.Create(
                id,
                VersionStamp.Create(),
                name,
                name,
                LanguageNames.CSharp,
                isSubmission: true,
                parseOptions: parseOptions,
                compilationOptions: compilationOptions,
                metadataReferences: previousProject != null ? ImmutableArray<MetadataReference>.Empty : this.DefaultReferences,
                projectReferences: previousProject != null ? new[] { new ProjectReference( previousProject.Id ) } : null ) );

            Project project = solution.GetProject( id );

            return project;
        }
    }
}
