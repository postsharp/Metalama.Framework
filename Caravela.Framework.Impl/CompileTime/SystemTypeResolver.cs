// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// An implementation of <see cref="ICompileTimeTypeResolver"/> that cannot be used for user-code attributes.
    /// </summary>
    internal class SystemTypeResolver : ICompileTimeTypeResolver
    {
        public Type? GetCompileTimeType( ITypeSymbol typeSymbol, bool fallbackToMock, CancellationToken cancellationToken )
        {
            Type? ReturnNullOrMock()
            {
                if ( fallbackToMock )
                {
                    return CompileTimeType.Create( typeSymbol );
                }

                return null;
            }

            var typeName = typeSymbol.GetReflectionName();

            if ( typeSymbol.ContainingAssembly != null )
            {
                var assemblyName = typeSymbol.ContainingAssembly.Name;

                // We don't allow loading new assemblies to the AppDomain.
                if ( AppDomain.CurrentDomain.GetAssemblies().All( a => a.GetName().Name != assemblyName ) )
                {
                    return ReturnNullOrMock();
                }

                typeName += ", " + assemblyName;
            }

            var type = Type.GetType( typeName );

            if ( type == null )
            {
                return ReturnNullOrMock();
            }

            return type;
        }
    }
}