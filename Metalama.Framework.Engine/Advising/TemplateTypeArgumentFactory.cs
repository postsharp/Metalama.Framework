// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;

namespace Metalama.Framework.Engine.Advising;

internal sealed class TemplateTypeArgumentFactory
{
    public IType Type { get; }

    private readonly string _name;

    public TemplateTypeArgumentFactory( IType type, string name )
    {
        this.Type = type;
        this._name = name;
    }

    public TemplateTypeArgument Create( SyntaxGenerationContext context ) => Create( this.Type, this._name, context );

    public static TemplateTypeArgument Create( IType type, string name, SyntaxGenerationContext context )
    {
        var symbol = type.GetSymbol();
        var syntax = context.SyntaxGenerator.Type( symbol ).AssertNotNull();
        var syntaxForTypeOf = context.SyntaxGenerator.TypeOfExpression( symbol ).Type;

        return new TemplateTypeArgument( name, type, syntax, syntaxForTypeOf );
    }
}