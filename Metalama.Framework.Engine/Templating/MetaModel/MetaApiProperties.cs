// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
using System;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    /// <summary>
    /// Encapsulates properties that are common to all constructors of <see cref="Metalama.Framework.Engine.Templating.MetaModel.MetaApi"/>.
    /// </summary>
    internal class MetaApiProperties
    {
        public UserDiagnosticSink Diagnostics { get; }

        public TemplateMember<IMemberOrNamedType> Template { get; }

        public IObjectReader Tags { get; }

        public AspectLayerId AspectLayerId { get; }

        public SyntaxGenerationContext SyntaxGenerationContext { get; }

        public IAspectInstance AspectInstance { get; }

        public IServiceProvider ServiceProvider { get; }

        public AspectPipelineDescription PipelineDescription => this.ServiceProvider.GetRequiredService<AspectPipelineDescription>();

        public MetaApiStaticity Staticity { get; }

        public MetaApiProperties(
            UserDiagnosticSink diagnostics,
            TemplateMember<IMemberOrNamedType> template,
            IObjectReader tags,
            AspectLayerId aspectLayerId,
            SyntaxGenerationContext syntaxGenerationContext,
            IAspectInstance aspectInstance,
            IServiceProvider serviceProvider,
            MetaApiStaticity staticity )
        {
            serviceProvider.GetRequiredService<ServiceProviderMark>().RequireProjectWide();

            this.Diagnostics = diagnostics;
            this.Template = template;
            this.Tags = tags;
            this.AspectLayerId = aspectLayerId;
            this.SyntaxGenerationContext = syntaxGenerationContext;
            this.AspectInstance = aspectInstance;
            this.ServiceProvider = serviceProvider;
            this.Staticity = staticity;
        }
    }
}