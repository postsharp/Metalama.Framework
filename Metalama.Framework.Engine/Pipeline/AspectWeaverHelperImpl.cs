// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Simplification;
using System;

namespace Metalama.Framework.Engine.Pipeline
{
    internal class AspectWeaverHelperImpl : AspectWeaverHelper
    {
        private readonly ReflectionMapper _reflectionMapper;

        public AspectWeaverHelperImpl( IServiceProvider serviceProvider, Compilation compilation )
        {
            this._reflectionMapper = serviceProvider.GetRequiredService<ReflectionMapperFactory>().GetInstance( compilation );
        }

        public override ITypeSymbol? GetTypeSymbol( Type type ) => this._reflectionMapper.GetTypeSymbol( type );
    }
}