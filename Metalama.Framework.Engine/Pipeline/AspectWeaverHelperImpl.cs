// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Pipeline
{
    internal class AspectWeaverHelperImpl : AspectWeaverHelper
    {
        private readonly ReflectionMapper _reflectionMapper;

        public AspectWeaverHelperImpl( ProjectServiceProvider serviceProvider, Compilation compilation )
        {
            this._reflectionMapper = serviceProvider.GetRequiredService<CompilationContextFactory>().GetInstance( compilation ).ReflectionMapper;
        }

        public override ITypeSymbol GetTypeSymbol( Type type ) => this._reflectionMapper.GetTypeSymbol( type );
    }
}