// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Implemented by the linker.
    /// </summary>
    internal interface ICodeTransformationContextImpl
    {
        void AddMark( CodeTransformationOperator @operator, SyntaxNode? operand );

        void Decline();

        T GetState<T>();

        void SetState<T>(T state);
    }
}