// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Aspects.AdvisedCode
{
    public interface IAdviceProperty : IProperty, IAdviceFieldOrProperty
    {
        new IAdviceParameterList Parameters { get; }
    }
}