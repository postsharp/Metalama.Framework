// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Extension methods for <see cref="IAspectReceiver{TDeclaration}"/>.
/// </summary>
[PublicAPI]
[CompileTime]
public static class AspectReceiverExtensions
{
    /// <summary>
    /// Selects a reference assembly in the current compilation given its assembly name.
    /// </summary>
    public static IAspectReceiver<IAssembly> SelectReferencedAssembly( this IAspectReceiver<ICompilation> receiver, string assemblyName )
        => receiver.SelectMany( c => c.ReferencedAssemblies.OfName( assemblyName ) );

    /// <summary>
    /// Selects an <see cref="INamedType"/> in the current compilation or in a reference assembly given its reflection <see cref="Type"/>.
    /// </summary>
    public static IAspectReceiver<INamedType> SelectReflectionType( this IAspectReceiver<ICompilation> receiver, Type type )
        => receiver.Select( c => (INamedType) ((ICompilationInternal) c).Factory.GetTypeByReflectionType( type ) );

    /// <summary>
    /// Selects several <see cref="INamedType"/> in the current compilation or in a reference assembly given their reflection <see cref="Type"/>.
    /// </summary>
    public static IAspectReceiver<INamedType> SelectReflectionTypes( this IAspectReceiver<ICompilation> receiver, IEnumerable<Type> types )
        => receiver.SelectMany( c => types.Select( t => (INamedType) ((ICompilationInternal) c).Factory.GetTypeByReflectionType( t ) ) );

    /// <summary>
    /// Selects several <see cref="INamedType"/> in the current compilation or in a reference assembly given their reflection <see cref="Type"/>.
    /// </summary>
    public static IAspectReceiver<INamedType> SelectReflectionTypes( this IAspectReceiver<ICompilation> receiver, params Type[] types )
        => receiver.SelectReflectionTypes( (IEnumerable<Type>) types );
}