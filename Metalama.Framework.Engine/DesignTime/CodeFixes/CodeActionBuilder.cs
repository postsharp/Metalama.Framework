// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.DesignTime.CodeFixes.Implementations;
using System;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes;

/// <summary>
/// The implementation of <see cref="ICodeActionBuilder"/>, passed to user code.
/// </summary>
internal sealed class CodeActionBuilder : ICodeActionBuilder
{
    private readonly CodeActionContext _context;

    public CodeActionBuilder( CodeActionContext context )
    {
        this._context = context;
    }

    public ICodeActionContext Context => this._context;

    public Task AddAttributeAsync( IDeclaration targetDeclaration, AttributeConstruction attribute )
        => new AddAttributeCodeAction( targetDeclaration, attribute ).ExecuteAsync( this._context );

    public Task RemoveAttributesAsync( IDeclaration targetDeclaration, INamedType attributeType )
        => new RemoveAttributeCodeAction( targetDeclaration, attributeType ).ExecuteAsync( this._context );

    public Task RemoveAttributesAsync( IDeclaration targetDeclaration, Type attributeType )
        => this.RemoveAttributesAsync(
            targetDeclaration,
            (INamedType) targetDeclaration.Compilation.GetCompilationModel().Factory.GetTypeByReflectionType( attributeType ) );

    public Task ApplyAspectAsync<TTarget>( TTarget targetDeclaration, IAspect<TTarget> aspect )
        where TTarget : class, IDeclaration
        => new ApplyAspectCodeAction<TTarget>( targetDeclaration, aspect ).ExecuteAsync( this._context );

    public Task ChangeAccessibilityAsync( IMemberOrNamedType targetMember, Accessibility accessibility )
        => new ChangeVisibilityCodeAction( targetMember, accessibility ).ExecuteAsync( this._context );
}