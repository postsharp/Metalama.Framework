// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    /// <summary>
    /// Encapsulates properties that are common to all constructors of <see cref="Metalama.Framework.Engine.Templating.MetaModel.MetaApi"/>.
    /// </summary>
    internal sealed class MetaApiProperties
    {
        public UserDiagnosticSink Diagnostics { get; }

        public TemplateMember<IMemberOrNamedType> Template { get; }

        public IObjectReader Tags { get; }

        public AspectLayerId AspectLayerId { get; }

        public SyntaxGenerationContext SyntaxGenerationContext { get; }

        public IAspectInstanceInternal? AspectInstance { get; }

        public ProjectServiceProvider ServiceProvider { get; }

        public ExecutionScenario ExecutionScenario => this.ServiceProvider.GetRequiredService<ExecutionScenario>();

        public MetaApiStaticity Staticity { get; }

        public ICompilation SourceCompilation { get; }

        public MetaApiProperties(
            ICompilation sourceCompilation,
            UserDiagnosticSink diagnostics,
            TemplateMember<IMemberOrNamedType> template,
            IObjectReader tags,
            AspectLayerId aspectLayerId,
            SyntaxGenerationContext syntaxGenerationContext,
            IAspectInstanceInternal? aspectInstance, // Can be null in tests.
            ProjectServiceProvider serviceProvider,
            MetaApiStaticity staticity )
        {
            this.SourceCompilation = sourceCompilation;
            this.Diagnostics = diagnostics;
            this.Template = template;
            this.Tags = tags;
            this.AspectLayerId = aspectLayerId;
            this.SyntaxGenerationContext = syntaxGenerationContext;
            this.AspectInstance = aspectInstance;
            this.ServiceProvider = serviceProvider;
            this.Staticity = staticity;
        }

        internal T Translate<T>( T declaration )
            where T : class, IDeclaration
            => declaration.ForCompilation( this.SourceCompilation, ReferenceResolutionOptions.CanBeMissing );
    }
}