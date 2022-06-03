// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    // The only class that should use this factory is SystemTypeResolver.
    internal class CompileTimeTypeFactory : IService
    {
        private readonly ConcurrentDictionary<string, Type> _instances = new( StringComparer.Ordinal );

        public Type Get( ITypeSymbol symbol )
        {
            return symbol switch
            {
                IDynamicTypeSymbol => throw new AssertionFailedException(),
                IArrayTypeSymbol { ElementType: IDynamicTypeSymbol } => throw new AssertionFailedException(),
                _ => this.Get( symbol.GetSymbolId(), symbol.GetReflectionName().AssertNotNull() )
            };
        }

        public Type Get( SymbolId symbolKey, string fullMetadataName )
        {
            return this._instances.GetOrAdd( symbolKey.ToString(), id => CompileTimeType.CreateFromSymbolId( new SymbolId( id ), fullMetadataName ) );
        }

        public Type Get( SymbolId symbolKey, IReadOnlyDictionary<string, IType>? substitutions, bool ignoreAssemblyKey )
        {
            var compilation = SyntaxBuilder.CurrentImplementation.Compilation.GetCompilationModel();
            var originalSymbol = (ITypeSymbol) symbolKey.Resolve( compilation.RoslynCompilation, ignoreAssemblyKey ).AssertNotNull();

            if ( substitutions != null && substitutions.Count > 0 )
            {
                var originalType = compilation.Factory.GetIType( originalSymbol );
                var rewriter = new TypeParameterRewriter( substitutions );
                var rewrittenTypeSymbol = rewriter.Visit( originalType ).GetSymbol();

                return this.Get( SymbolId.Create( rewrittenTypeSymbol ), rewrittenTypeSymbol.GetReflectionName()! );
            }
            else
            {
                return this.Get( symbolKey, originalSymbol.GetReflectionName()! );
            }
        }

        internal class TypeParameterRewriter : TypeRewriter
        {
            private readonly IReadOnlyDictionary<string, IType> _substitutions;

            public TypeParameterRewriter( IReadOnlyDictionary<string, IType> substitutions )
            {
                this._substitutions = substitutions;
            }

            public static TypeRewriter Get( BoundTemplateMethod template )
            {
                return template.Template.TemplateClassMember.TypeParameters.All( x => !x.IsCompileTime )
                    ? Null
                    : new TemplateTypeRewriter( template );
            }

            internal override ITypeInternal Visit( TypeParameter typeParameter )
            {
                if ( this._substitutions.TryGetValue( typeParameter.Name, out var substitution ) )
                {
                    return (ITypeInternal) substitution;
                }
                else
                {
                    return typeParameter;
                }
            }
        }
    }
}