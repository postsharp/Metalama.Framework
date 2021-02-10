using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynPad.Roslyn;

namespace Caravela.AspectWorkbench.CodeEditor
{
    public class CustomRoslynHost : RoslynHost
    {
        public static CustomRoslynHost Create()
        {
            var host = new CustomRoslynHost(
                disabledDiagnostics: ImmutableArray.Create( "IDE0051" /* Private member is unused. */ ),
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
                            "Caravela.Framework.Aspects.TemplateContext",
                        } ) );

            return host;
        }

        public CustomRoslynHost(
            ImmutableArray<string>? disabledDiagnostics = default,
            IEnumerable<Assembly>? additionalAssemblies = null,
            RoslynHostReferences? references = null ) : base( additionalAssemblies, references, disabledDiagnostics: disabledDiagnostics )
        {
        }

        protected override Project CreateProject( Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project? previousProject = null )
        {
            var name = args.Name ?? "Template";
            var id = ProjectId.CreateNewId( name );

            var parseOptions = new CSharpParseOptions( kind: SourceCodeKind.Script, languageVersion: LanguageVersion.Latest );

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

            var project = solution.GetProject( id )!;

            return project;
        }
    }
}
