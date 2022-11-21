// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Transformations
{
    internal sealed class OverrideIndexerTransformation : OverrideIndexerBaseTransformation
    {
        public BoundTemplateMethod? GetTemplate { get; }

        public BoundTemplateMethod? SetTemplate { get; }

        public OverrideIndexerTransformation(
            Advice advice,
            IIndexer overriddenDeclaration,
            BoundTemplateMethod? getTemplate,
            BoundTemplateMethod? setTemplate,
            IObjectReader tags )
            : base( advice, overriddenDeclaration, tags )
        {
            this.GetTemplate = getTemplate;
            this.SetTemplate = setTemplate;
        }

        public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
        {
            var templateExpansionError = false;
            BlockSyntax? getAccessorBody = null;

            if ( this.OverriddenDeclaration.GetMethod != null )
            {
                if ( this.GetTemplate != null )
                {
                    templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                        context,
                        this.GetTemplate,
                        this.OverriddenDeclaration.GetMethod,
                        out getAccessorBody );
                }
                else
                {
                    getAccessorBody = this.CreateIdentityAccessorBody( context, SyntaxKind.GetAccessorDeclaration );
                }
            }
            else
            {
                getAccessorBody = null;
            }

            BlockSyntax? setAccessorBody = null;

            if ( this.OverriddenDeclaration.SetMethod != null )
            {
                if ( this.SetTemplate != null )
                {
                    templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                        context,
                        this.SetTemplate,
                        this.OverriddenDeclaration.SetMethod,
                        out setAccessorBody );
                }
                else
                {
                    setAccessorBody = this.CreateIdentityAccessorBody( context, SyntaxKind.SetAccessorDeclaration );
                }
            }
            else
            {
                setAccessorBody = null;
            }

            if ( templateExpansionError )
            {
                // Template expansion error.
                return Enumerable.Empty<InjectedMember>();
            }

            return base.GetInjectedMembersImpl( context, getAccessorBody, setAccessorBody );
        }
    }
}