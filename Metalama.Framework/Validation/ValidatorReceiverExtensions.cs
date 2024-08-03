// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Validation;

/// <summary>
/// Extension methods for <see cref="IValidatorReceiver{TDeclaration}"/>.
/// </summary>
[PublicAPI]
[CompileTime]
public static class ValidatorReceiverExtensions
{
    /// <summary>
    /// Selects a reference assembly in the current compilation given its assembly name.
    /// </summary>
    public static IValidatorReceiver<IAssembly> SelectReferencedAssembly( this IValidatorReceiver<ICompilation> receiver, string assemblyName )
        => receiver.SelectMany( c => c.ReferencedAssemblies.OfName( assemblyName ) );

    /// <summary>
    /// Selects all custom attributes of a given type in the current compilation. This generic overloads constructs the attribute
    /// and accepts an optional predicate to filter the attribute.
    /// </summary>
    public static IValidatorReceiver<IDeclaration> SelectDeclarationsWithAttribute<TAttribute>(
        this IValidatorReceiver<ICompilation> receiver,
        Func<TAttribute, bool>? predicate = null,
        bool includeDerivedTypes = true )
        => receiver.SelectMany( c => c.GetDeclarationsWithAttribute( predicate, includeDerivedTypes ) );

    /// <summary>
    /// Selects all custom attributes of a given type in the current compilation. This overloads
    /// accepts an optional predicate to filter the attribute.
    /// </summary>
    public static IValidatorReceiver<IDeclaration> SelectDeclarationsWithAttribute(
        this IValidatorReceiver<ICompilation> receiver,
        Type attributeType,
        Func<IAttribute, bool>? predicate = null,
        bool includeDerivedTypes = true )
        => receiver.SelectMany( c => c.GetDeclarationsWithAttribute( attributeType, predicate, includeDerivedTypes ) );

    /// <summary>
    /// Selects an <see cref="INamedType"/> in the current compilation or in a reference assembly given its reflection <see cref="Type"/>.
    /// </summary>
    public static IValidatorReceiver<INamedType> SelectReflectionType( this IValidatorReceiver<ICompilation> receiver, Type type )
        => receiver.Select( c => (INamedType) ((ICompilationInternal) c).Factory.GetTypeByReflectionType( type ) );

    /// <summary>
    /// Selects several <see cref="INamedType"/> in the current compilation or in a reference assembly given their reflection <see cref="Type"/>.
    /// </summary>
    public static IValidatorReceiver<INamedType> SelectReflectionTypes( this IValidatorReceiver<ICompilation> receiver, IEnumerable<Type> types )
        => receiver.SelectMany( c => types.Select( t => (INamedType) ((ICompilationInternal) c).Factory.GetTypeByReflectionType( t ) ) );

    /// <summary>
    /// Selects several <see cref="INamedType"/> in the current compilation or in a reference assembly given their reflection <see cref="Type"/>.
    /// </summary>
    public static IValidatorReceiver<INamedType> SelectReflectionTypes( this IValidatorReceiver<ICompilation> receiver, params Type[] types )
        => receiver.SelectReflectionTypes( (IEnumerable<Type>) types );
}