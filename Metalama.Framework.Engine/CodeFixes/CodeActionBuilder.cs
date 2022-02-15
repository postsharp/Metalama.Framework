// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Engine.CodeFixes.Implementations;
using System;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes
{
    /// <summary>
    /// The implementation of <see cref="ICodeActionBuilder"/>, passed to user code.
    /// </summary>
    internal class CodeActionBuilder : ICodeActionBuilder
    {
        private readonly CodeActionContext _context;

        public CodeActionBuilder( CodeActionContext context )
        {
            this._context = context;
        }

        public ICodeActionContext Context => this._context;

        public Task<bool> AddAttributeAsync( IDeclaration targetDeclaration, AttributeConstruction attribute )
            => new AddAttributeCodeAction( targetDeclaration, attribute ).ExecuteAsync( this._context );

        public Task<bool> RemoveAttributesAsync( IDeclaration targetDeclaration, INamedType attributeType )
            => new RemoveAttributeCodeAction( targetDeclaration, attributeType ).ExecuteAsync( this._context );

        public Task<bool> RemoveAttributesAsync( IDeclaration targetDeclaration, Type attributeType )
            => this.RemoveAttributesAsync( targetDeclaration, (INamedType) targetDeclaration.Compilation.TypeFactory.GetTypeByReflectionType( attributeType ) );

        public Task<bool> ApplyAspectAsync<TTarget>( TTarget targetDeclaration, IAspect<TTarget> aspect )
            where TTarget : class, IDeclaration
            => new ApplyAspectCodeAction<TTarget>( targetDeclaration, aspect ).ExecuteAsync( this._context );
    }
}