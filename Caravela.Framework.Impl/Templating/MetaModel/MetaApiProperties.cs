// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.ServiceProvider;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// Encapsulates properties that are common to all constructors of <see cref="Caravela.Framework.Impl.Templating.MetaModel.MetaApi"/>.
    /// </summary>
    internal record MetaApiProperties(
        UserDiagnosticSink Diagnostics,
        TemplateMember<IMemberOrNamedType> Template,
        IReadOnlyDictionary<string, object?> Tags,
        AspectLayerId AspectLayerId,
        SyntaxGenerationContext SyntaxGenerationContext,
        IServiceProvider ServiceProvider )
    {
        public AspectPipelineDescription PipelineDescription { get; } = ServiceProvider.GetService<AspectPipelineDescription>();
    }
}