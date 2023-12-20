// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Metalama.Framework.Diagnostics.Severity;

namespace Metalama.Framework.Engine.Linking
{
    public static class AspectLinkerDiagnosticDescriptors
    {
        // Reserved range 650-699

        private const string _category = "Metalama.Linker";

        internal static readonly DiagnosticDefinition<ISymbol>
            CannotInvokeAnotherInstanceBaseRequired = new(
                "LAMA0650",
                "Can't invoke member, because correct invocation would require a base call on an instance other than this.",
                "Can't invoke member '{0}', because correct invocation would require a base call on an instance other than this.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(StatementSyntax Statement, INamedType Type)>
            CannotAddStatementToPrimaryConstructor = new(
                "LAMA0651",
                "Statement can't be added as an initializer to primary constructor.",
                "The statement '{0}' can't be added as an initializer to a primary constructor on type '{1}'. Only simple assignment is supported.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string Expression, INamedType Type, string Explanation)>
            CannotAssignToExpressionFromPrimaryConstructor = new(
                "LAMA0652",
                "Expression can't be used as the assignment target for an initializer of a primary constructor.",
                "The expression '{0}' can't be used as the assignment target for an initializer of a primary constructor on type '{1}'. {2}",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(DeclarationKind DeclarationKind, IFieldOrProperty Member, INamedType Type, string[] Aspects)>
            CannotAssignToMemberMoreThanOnceFromPrimaryConstructor = new(
                "LAMA0653",
                "Member can't be used as the assignment target for an initializer of a primary constructor more than once.",
                "The {0} '{1}' can't be used as the assignment target for an initializer of a primary constructor on type '{2}' more than once. The problematic aspects are: {3}.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, ISymbol TargetDeclaration)>
            DeclarationMustBeInlined = new(
                "LAMA0699",
                "Declaration must be inlined.",
                "Version of declaration '{1} provided by '{0}' cannot be inlined. It is not currently possible to generate non-inlined code for this declaration.",
                _category,
                Error );
    }
}