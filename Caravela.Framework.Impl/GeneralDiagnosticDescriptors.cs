﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Caravela.Framework.Impl
{
    internal static class GeneralDiagnosticDescriptors
    {
        // Reserved range 0-99

        private const string _category = "Caravela.General";

        public static readonly StrongDiagnosticDescriptor<(string Message, string File)> UncaughtException =
            new( "CR0001", "Unexpected exception in Caravela.", "Unexpected exception occurred in Caravela: {0} Exception details are in {1}.", _category,
                 Error );

        public static readonly StrongDiagnosticDescriptor<(IType AspectType, CodeElementKind CodeElementKind, ICodeElement CodeElement, IType InterfaceType)>
            AspectAppliedToIncorrectElement =
                new( "CR0003", "Aspect applied to incorrect kind of element.",
                     "Aspect '{0}' cannot be applied to {1} '{2}', because this aspect does not implement the '{3}' interface.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<(IType AspectType, string Weavers)> AspectHasMoreThanOneWeaver =
            new( "CR0004", "Aspect has more than one weaver.", "Aspect '{0}' can have at most one weaver, but it has the following: {1}.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<(IType WeaverType, string Exception)> ExceptionInWeaver =
            new( "CR0006", "Exception in aspect weaver.", "Exception occurred while executing the aspect weaver '{0}': {1}", _category, Error );

        public static readonly StrongDiagnosticDescriptor<(ICodeElement Member, int ArgumentsCount)> MemberRequiresNArguments =
            new( "CR0012", "Member requires number of arguments.", "Member '{0}' requires {1} arguments.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<(ICodeElement Member, int ArgumentsCount)> MemberRequiresAtLeastNArguments =
            new( "CR0013", "Member requires more arguments.", "Member '{0}' requires at least {1} arguments.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<IMember> CannotProvideInstanceForStaticMember =
            new( "CR0014", "Cannot provide instance for a static member.", "Member {0} is static, but has been used with a non-null instance.", _category,
                 Error );

        public static readonly StrongDiagnosticDescriptor<IMember> MustProvideInstanceForInstanceMember =
            new( "CR0015", "Has to provide instance for an instance member.", "Member {0} is not static, but has been used with a null instance.", _category,
                 Error );

        public static readonly StrongDiagnosticDescriptor<IMember> CannotAccessOpenGenericMember =
            new( "CR0016", "Cannot access an open generic member.", "Member {0} Cannot be accessed without specifying generic arguments.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<IMember> CannotProvideInstanceForLocalFunction =
            new( "CR0018", "Cannot provide instance for a local function.", "{0} is a local function, so it Cannot be invoked with a non-null instance.",
                 _category, Error );

        public static readonly StrongDiagnosticDescriptor<(string Expression, string ParameterName, IMember Method)> CannotPassExpressionToByRefParameter =
            new( "CR0019", "Cannot use an expression in an out or ref parameter.",
                 "Cannot pass the expression '{0}' to the '{1}' parameter of method '{2}' because the parameter is 'out' or 'ref'.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<string> CannotFindType =
            new( "CR0020", "Cannot find a type", "Cannot find the type '{0}'.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<string> CycleInAspectOrdering =
            new( "CR0021", "A cycle was found in aspect ordering.",
                 "A cycle was found in the specifications of aspect ordering between the following aspect part: {0}.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<(IType ParentType, IType ChildType)> CannotAddChildAspectToPreviousPipelineStep = new(
            "CR0022",
            "Cannot add an aspect to a previous step of the compilation pipeline.",
            "The aspect {0} cannot add a child aspect to of type {1} because this aspect type has already been processed.",
            _category,
            Error );

        public static readonly StrongDiagnosticDescriptor<(IType AspectType, ICodeElement Target)> CannotAddAdviceToPreviousPipelineStep = new(
            "CR0023",
            "Cannot add an advice to a previous step of the compilation pipeline.",
            "The aspect {0} cannot add an advice to {1} because this declaration has already been processed.",
            _category,
            Error );
    }
}