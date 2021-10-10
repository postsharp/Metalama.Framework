// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Kinds of <see cref="IDeclaration"/>.
    /// </summary>
    [CompileTimeOnly]
    public enum DeclarationKind
    {
        /// <summary>
        /// Not a valid declaration represented by <see cref="IDeclaration"/>.
        /// </summary>
        None,

        /// <summary>
        /// <see cref="ICompilation"/>.
        /// </summary>
        Compilation,

        /// <summary>
        /// <see cref="INamedType"/>.
        /// </summary>
        NamedType,

        /// <summary>
        /// <see cref="IMethod"/>.
        /// </summary>
        Method,

        /// <summary>
        /// <see cref="IProperty"/>.
        /// </summary>
        Property,

        /// <summary>
        /// <see cref="IProperty"/> (fields are represented as properties).
        /// </summary>
        Field,

        /// <summary>
        /// <see cref="IEvent"/>.
        /// </summary>
        Event,

        /// <summary>
        /// <see cref="IParameter"/>.
        /// </summary>
        Parameter,

        /// <summary>
        /// <see cref="ITypeParameter"/>.
        /// </summary>
        GenericParameter,

        /// <summary>
        /// <see cref="IAttribute"/>.
        /// </summary>
        Attribute,

        /// <summary>
        /// <see cref="IManagedResource"/>.
        /// </summary>
        ManagedResource,

        /// <summary>
        /// <see cref="IConstructor"/>.
        /// </summary>
        Constructor,

        /// <summary>
        /// A reference assembly, implementing <see cref="IAssembly"/>. Note
        /// that the current assembly is represented by <see cref="ICompilation"/> that inherits <see cref="IAssembly"/>, but the
        /// <see cref="DeclarationKind"/> for the current compilation is <see cref="Compilation"/> and not <see cref="AssemblyReference"/>. 
        /// </summary>
        AssemblyReference,

        /// <summary>
        /// <see cref="INamespace"/>.
        /// </summary>
        Namespace
    }
}