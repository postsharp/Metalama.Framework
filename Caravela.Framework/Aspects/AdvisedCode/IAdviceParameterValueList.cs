// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Aspects.AdvisedCode
{
    [CompileTimeOnly]
    public interface IAdviceParameterValueList
    {
        [return: RunTimeOnly]
        dynamic ToArray();

        [return: RunTimeOnly]
        dynamic ToValueTuple();
    }
}