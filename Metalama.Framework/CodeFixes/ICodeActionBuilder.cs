// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Validation;
using System;
using System.Threading.Tasks;

namespace Metalama.Framework.CodeFixes
{
    /// <summary>
    /// Argument of the delegate passed to <see cref="CodeFix"/> constructor. Exposes methods that allow to modify the current solution. 
    /// </summary>
    [CompileTimeOnly]
    [InternalImplement]
    public interface ICodeActionBuilder
    {
        /// <summary>
        /// Gets the context of the current code action.
        /// </summary>
        ICodeActionContext Context { get; }

        /// <summary>
        /// Adds a custom attribute to a given declaration.
        /// </summary>
        Task<bool> AddAttributeAsync( IDeclaration targetDeclaration, AttributeConstruction attribute );

        /// <summary>
        /// Removes custom attributes of a type, given as an <see cref="INamedType"/>, from a given declaration and all contained declarations.
        /// </summary>
        Task<bool> RemoveAttributesAsync( IDeclaration targetDeclaration, INamedType attributeType );

        /// <summary>
        /// Removes custom attributes of a type, given as a reflection <see cref="Type"/>, from a given declaration and all contained declarations.
        /// </summary>
        Task<bool> RemoveAttributesAsync( IDeclaration targetDeclaration, Type attributeType );

        /// <summary>
        /// Applies an aspect to a given declaration (so that the <i>source</i> code of the given declaration is modified by the affect).
        /// </summary>
        Task<bool> ApplyAspectAsync<TTarget>( TTarget targetDeclaration, IAspect<TTarget> aspect )
            where TTarget : class, IDeclaration;
    }
}