// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Metalama.LinqPad
{
    /// <summary>
    /// Gets <see cref="FacadeObject"/> instances for any given input object.
    /// </summary>
    internal class FacadeObjectFactory
    {
        private static readonly ConditionalWeakTable<object, FacadeObject> _objectFacades = new();
        private static readonly ConcurrentDictionary<Type, FacadeType> _types = new();

        public Func<IDeclaration, GetCompilationInfo> GetGetCompilationInfo { get; }

        public FacadeObjectFactory( Func<IDeclaration, GetCompilationInfo>? workspaceExpression = null )
        {
            this.GetGetCompilationInfo = workspaceExpression ?? (_ => new GetCompilationInfo( "workspace", false ));
        }

        internal FacadeObject? GetFacade( object? instance )
        {
            var isInlineType = instance == null || instance is IEnumerable || instance is string || instance.GetType().IsPrimitive
                               || (instance.GetType().Assembly.FullName is { } fullName
                                   && fullName.StartsWith( "LINQPad", StringComparison.OrdinalIgnoreCase ));

            if ( isInlineType )
            {
                return null;
            }
            else
            {
                if ( !_objectFacades.TryGetValue( instance!, out var proxy ) )
                {
                    proxy = new FacadeObject( this.GetFormatterType( instance!.GetType() ), instance );
                    _objectFacades.AddOrUpdate( instance, proxy );
                }

                return proxy;
            }
        }

        public FacadeType GetFormatterType( Type type ) => _types.GetOrAdd( type, t => new FacadeType( this, t ) );
    }
}