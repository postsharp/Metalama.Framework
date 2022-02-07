// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

#pragma warning disable IDE0005
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Tests.Integration.Runners;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynPad.Roslyn;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

#pragma warning restore IDE0005

namespace Metalama.AspectWorkbench.CodeEditor
{
    public class RoslynPadHost : RoslynHost
    {
        public static RoslynPadHost Create()
        {
            var dotnetDirectory = Path.GetDirectoryName( typeof(object).Assembly.Location )!;
            var dotNetAssemblies = new[] { "System.Runtime", "System.Linq", "System.Console", "System.Collections" };

            var host = new RoslynPadHost(
                ImmutableArray.Create( "IDE0051" /* Private member is unused. */ ),
                new[] { Assembly.Load( "RoslynPad.Roslyn.Windows" ), Assembly.Load( "RoslynPad.Editor.Windows" ) },
                RoslynHostReferences.Empty
                    .With(
                        references: dotNetAssemblies.Select( a => MetadataReference.CreateFromFile( Path.Combine( dotnetDirectory, a + ".dll" ) ) ),
                        assemblyReferences: new[]
                        {
                            typeof(object).Assembly,
                            typeof(Console).Assembly,
                            typeof(Enumerable).Assembly,
                            typeof(Dictionary<,>).Assembly,
                            typeof(DynamicAttribute).Assembly,
                            typeof(SyntaxFactory).Assembly,
                            typeof(meta).Assembly,
                            typeof(TemplateSyntaxFactory).Assembly,
                            typeof(TestTemplateAttribute).Assembly,
                            typeof(TestTemplateCompiler).Assembly,
                            typeof(IAspectWeaver).Assembly,
                            typeof(SyntaxTree).Assembly,
                            typeof(CSharpSyntaxTree).Assembly
                        } ) );

            return host;
        }

        public RoslynPadHost(
            ImmutableArray<string>? disabledDiagnostics = default,
            IEnumerable<Assembly>? additionalAssemblies = null,
            RoslynHostReferences? references = null ) : base( additionalAssemblies, references, disabledDiagnostics ) { }

        protected override Project CreateProject(
            Solution solution,
            DocumentCreationArgs args,
            CompilationOptions compilationOptions,
            Project? previousProject = null )
        {
            var name = args.Name ?? "Template";
            var id = ProjectId.CreateNewId( name );

            var parseOptions = new CSharpParseOptions( kind: SourceCodeKind.Regular, languageVersion: LanguageVersion.Latest );

            solution = solution.AddProject(
                ProjectInfo.Create(
                    id,
                    VersionStamp.Create(),
                    name,
                    name,
                    LanguageNames.CSharp,
                    parseOptions: parseOptions,
                    compilationOptions: compilationOptions,
                    metadataReferences: previousProject != null ? ImmutableArray<MetadataReference>.Empty : this.DefaultReferences,
                    projectReferences: previousProject != null ? new[] { new ProjectReference( previousProject.Id ) } : null ) );

            var project = solution.GetProject( id )!;

            return project;
        }
    }
}