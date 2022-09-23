// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Caching;
using System;
using System.Collections;
using System.Collections.Concurrent;

namespace Metalama.LinqPad
{
    /// <summary>
    /// Gets <see cref="FacadeObject"/> instances for any given input object.
    /// </summary>
    internal class FacadeObjectFactory
    {
#pragma warning disable CA1805 // Do not initialize unnecessarily
        private static readonly WeakCache<object, FacadeObject> _objectFacades = new();
#pragma warning restore CA1805 // Do not initialize unnecessarily
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
                return _objectFacades.GetOrAdd( instance!, i => new FacadeObject( this.GetFormatterType( i.GetType() ), i ) );
            }
        }

        public FacadeType GetFormatterType( Type type ) => _types.GetOrAdd( type, t => new FacadeType( this, t ) );
    }
}