// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Sdk;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Pipeline
{
    internal class AspectWeaverHelper : IAspectWeaverHelper
    {
        private readonly ReflectionMapper _reflectionMapper;

        public AspectWeaverHelper( IServiceProvider serviceProvider, Compilation compilation )
        {
            this._reflectionMapper = serviceProvider.GetService<ReflectionMapperFactory>().GetInstance( compilation );
        }

        public ITypeSymbol? GetTypeSymbol( Type type ) => this._reflectionMapper.GetTypeSymbol( type );
    }
}