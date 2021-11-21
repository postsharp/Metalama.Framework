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
    [CompileTimeOnly]
    [InternalImplement]
    public interface ICodeFixBuilder
    {
        CancellationToken CancellationToken { get; }

        Task<bool> AddAttributeAsync( IDeclaration targetDeclaration, AttributeConstruction attribute );

        Task<bool> RemoveAttributeAsync( IDeclaration declaration, INamedType attributeType );

        Task<bool> ApplyAspectAsync<TTarget>( TTarget targetDeclaration, IAspect<TTarget> aspect )
            where TTarget : class, IDeclaration;
    }
}