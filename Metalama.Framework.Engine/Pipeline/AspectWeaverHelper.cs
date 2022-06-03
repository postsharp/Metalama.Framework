// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Simplification;
using System;

namespace Metalama.Framework.Engine.Pipeline
{
    internal class AspectWeaverHelper : IAspectWeaverHelper
    {
        static AspectWeaverHelper()
        {
            FormattingAnnotations.SimplifyAnnotation = Simplifier.Annotation;
        }

        private readonly ReflectionMapper _reflectionMapper;

        public AspectWeaverHelper( IServiceProvider serviceProvider, Compilation compilation )
        {
            this._reflectionMapper = serviceProvider.GetRequiredService<ReflectionMapperFactory>().GetInstance( compilation );
        }

        public ITypeSymbol? GetTypeSymbol( Type type ) => this._reflectionMapper.GetTypeSymbol( type );
    }
}