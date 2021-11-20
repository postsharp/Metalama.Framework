// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.CodeFixes
{
    public delegate Task CodeFixAsyncAction( ICodeFixBuilder builder );

    [CompileTimeOnly]
    [InternalImplement]
    public interface ICodeFixBuilder
    {
        CancellationToken CancellationToken { get; }

        Task AddAttributeAsync( IDeclaration targetDeclaration, AttributeConstruction attribute );

        Task RemoveAttributeAsync( IDeclaration declaration, INamedType attributeType );

        Task ApplyLiveTemplateAsync<TTarget>( TTarget declaration, ILiveTemplate<TTarget> liveTemplate )
            where TTarget : class, IDeclaration;
    }
}