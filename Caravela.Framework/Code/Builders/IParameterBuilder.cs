// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.Builders
{
    /// <summary>
    /// Allows to complete the construction of a parameter that has been created using e.g.
    /// <see cref="IMethodBuilder.AddParameter(string,Caravela.Framework.Code.IType,Caravela.Framework.Code.RefKind,Caravela.Framework.Code.TypedConstant)"/>.
    /// </summary>
    public interface IParameterBuilder : IParameter, IDeclarationBuilder
    {
        /// <remarks>
        /// Gets or sets the default value of the parameter, or  <c>default</c> if the parameter type is a struct and the default
        /// value of the parameter is the default value of the struct type.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">The parameter has no default value.</exception>
        new TypedConstant DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the parameter type.
        /// </summary>
        new IType ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the parameter ref kind.
        /// </summary>
        new RefKind RefKind { get; set; }

        /// <summary>
        /// Gets or sets of the paramter name.
        /// </summary>
        new string Name { get; set; }
    }
}