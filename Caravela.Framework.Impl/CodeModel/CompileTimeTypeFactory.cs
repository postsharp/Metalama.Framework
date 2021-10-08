// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class CompileTimeTypeFactory : IService
    {
        private readonly ConcurrentDictionary<string, Type> _instances = new( StringComparer.Ordinal );

        public Type Get( ITypeSymbol symbol )
            => symbol switch
            {
                IDynamicTypeSymbol => typeof(object),
                IArrayTypeSymbol { ElementType: IDynamicTypeSymbol } => typeof(object[]),
                _ => this.Get( DocumentationCommentId.CreateReferenceId( symbol ), symbol.ToDisplayString() )
            };

        public Type Get( string documentationId, string fullName )
            => this._instances.GetOrAdd( documentationId, id => CompileTimeType.CreateFromDocumentationId( id, fullName ) );
    }
}