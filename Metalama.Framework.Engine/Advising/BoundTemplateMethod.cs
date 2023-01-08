// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.Engine.Advising;

internal sealed class BoundTemplateMethod
{
    public TemplateMember<IMethod> Template { get; }

    public BoundTemplateMethod( TemplateMember<IMethod> template, object?[] templateArguments )
    {
        this.Template = template;
        this.TemplateArguments = templateArguments;

#if DEBUG
        if ( template.Declaration.MethodKind is MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove && templateArguments.Length != 1 )
        {
            throw new AssertionFailedException( $"'{template.Declaration}' is an accessor the the template has '{templateArguments.Length}' arguments." );
        }
#endif
    }

    public object?[] TemplateArguments { get; }

    public object?[] GetTemplateArgumentsForMethod( IHasParameters signature )
    {
        Invariant.Assert(
            this.Template.TemplateClassMember.RunTimeParameters.Length == 0 ||
            this.Template.TemplateClassMember.RunTimeParameters.Length == signature.Parameters.Count );

        var newArguments = (object?[]) this.TemplateArguments.Clone();

        for ( var index = 0; index < this.Template.TemplateClassMember.RunTimeParameters.Length; index++ )
        {
            var runTimeParameter = this.Template.TemplateClassMember.RunTimeParameters[index];
            newArguments[runTimeParameter.SourceIndex] = SyntaxFactory.IdentifierName( signature.Parameters[index].Name );
        }

        return newArguments;
    }
}