// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Metalama.TestFramework
{
    internal static class SyntaxTreeStructureVerifier
    {
        /// <summary>
        /// Checks for "hidden" problems in a <see cref="SyntaxTree"/>, i.e. problems where the _text_
        /// of the source code is valid, but the semantic syntax tree is not.
        /// </summary>
        public static bool VerifyMetaSyntax( Compilation compilation, IServiceProvider serviceProvider )
        {
            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                var actualSyntaxFactory = syntaxTree.GetRoot().ToSyntaxFactoryDebug( compilation, serviceProvider );

                var parsedFromText = CSharpSyntaxTree.ParseText( syntaxTree.GetRoot().ToString(), encoding: Encoding.UTF8 )
                    .GetRoot()
                    .ToSyntaxFactoryDebug( compilation, serviceProvider );

                if ( parsedFromText != actualSyntaxFactory )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks for "hidden" problems in a <see cref="SyntaxTree"/>, i.e. problems where the _text_
        /// of the source code is valid, but the semantic syntax tree is not.
        /// </summary>
        public static bool Verify( Compilation compilation, [NotNullWhen( false )] out DiagnosticList? diagnostics )
        {
            diagnostics = null;

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                var parsedFromText = CSharpSyntaxTree.ParseText(
                        syntaxTree.GetRoot().ToString(),
                        path: syntaxTree.FilePath,
                        encoding: Encoding.UTF8,
                        options: (CSharpParseOptions) syntaxTree.Options )
                    .GetRoot();

                foreach ( var diagnostic in parsedFromText.GetDiagnostics() )
                {
                    if ( diagnostic.Severity == DiagnosticSeverity.Error )
                    {
                        (diagnostics ??= new DiagnosticList()).Report( diagnostic );
                    }
                }
            }

            if ( diagnostics == null )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}