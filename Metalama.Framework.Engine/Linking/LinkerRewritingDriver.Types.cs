// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LinkerRewritingDriver
    {
        public ClassDeclarationSyntax RewriteClass(
            ClassDeclarationSyntax typeDeclaration,
            INamedTypeSymbol symbol )
        {
            if (this.LateTransformationRegistry.HasRemovedPrimaryConstructor(symbol))
            {
                typeDeclaration =
                    typeDeclaration.PartialUpdate(
                        parameterList: default( ParameterListSyntax ),
                        baseList:
                            typeDeclaration.BaseList != null
                            ? typeDeclaration.BaseList.WithTypes(
                                SeparatedList(
                                typeDeclaration.BaseList.Types.SelectAsArray( b =>
                                    b switch
                                    {
                                        PrimaryConstructorBaseTypeSyntax pc => SimpleBaseType( pc.Type ),
                                        _ => b
                                    } ) ) )
                            : default );
            }

            return typeDeclaration;
        }
    }
}