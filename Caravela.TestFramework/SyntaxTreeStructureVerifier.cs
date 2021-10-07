// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using Xunit;

namespace Caravela.TestFramework
{
    internal static class SyntaxTreeStructureVerifier
    {
        /// <summary>
        /// Checks for "hidden" problems in a <see cref="SyntaxTree"/>, i.e. problems where the _text_
        /// of the source code is valid, but the semantic syntax tree is not.
        /// </summary>
        public static void Verify( Compilation compilation, IServiceProvider serviceProvider )
        {
            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                var actualSyntaxFactory = syntaxTree.GetRoot().ToSyntaxFactoryDebug( compilation, serviceProvider );

                var parsedFromText = CSharpSyntaxTree.ParseText( syntaxTree.GetRoot().ToString() )
                    .GetRoot()
                    .ToSyntaxFactoryDebug( compilation, serviceProvider );

                Assert.Equal( parsedFromText, actualSyntaxFactory );
            }
        }
    }
}