// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.DeclarationBuilders
{
    /// <summary>
    /// Allows to complete the construction of a field that has been created by an advice.
    /// </summary>
    public interface IFieldBuilder : IFieldOrPropertyBuilder, IField { }
}