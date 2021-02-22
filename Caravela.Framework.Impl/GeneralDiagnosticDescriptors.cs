using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Caravela.Framework.Impl
{

    internal static class GeneralDiagnosticDescriptors
    {
        // Reserved range 0-99

        private const string _caravelaCategory = "Caravela";

        public static readonly DiagnosticDescriptor UncaughtException =
            new( "CR0001", "Unexpected exception in Caravela.", "Unexpected exception occurred in Caravela: {0} Exception details are in {1}.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor ErrorBuildingCompileTimeAssembly =
            new( "CR0002", "Error while building compile-time assembly.", "Error occurred while building compile-time assembly.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor AspectAppliedToIncorrectElement =
            new( "CR0003", "Aspect applied to incorrect kind of element.", "Aspect '{0}' cannot be applied to {1} '{2}', because it does not implement the '{3}' interface.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor AspectHasMoreThanOneWeaver =
            new( "CR0004", "Aspect has more than one weaver.", "Aspect '{0}' can have at most one weaver, but it has the following: {1}.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor ErrorProcessingTemplates =
            new( "CR0005", "Error while processing templates.", "Error occurred while processing templates.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor ExceptionInWeaver =
            new( "CR0006", "Exception in aspect weaver.", "Exception occurred while executing the aspect weaver '{0}': {1}", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor UnsupportedSerialization =
            new( "CR0007", "Build-time code not serializable.", "Build-time code attempted to create {0} but no serializer is registered for that type.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor CycleInSerialization =
            new( "CR0008", "A collection contains itself.", "Build-time code attempted to create a collection which contains itself: {0} ", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor MultidimensionalArray =
            new( "CR0009", "Multidimensional arrays not supported.", "Build-time array {0} has more than one dimension.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor UnsupportedDictionaryComparer =
            new( "CR0010", "Custom equality comparers not supported.", "Build-time dictionary has an equality comparer '{0}' that is not supported. Only the default comparer and predefined string comparers are supported.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor TypeNotSerializable =
            new( "CR0011", "Type not serializable.", "Build-time type value '{0}' is of a form that is not supported for serialization.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor MoreThanOneAdvicePerElement =
            new( "CR0012", "More than one advice per code element.", "'{0}' has more than one advice, which is currently not supported.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor MemberRequiresNArguments =
            new( "CR0012", "Member requires number of arguments.", "Member '{0}' requires {1} arguments.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor MemberRequiresAtLeastNArguments =
            new( "CR0013", "Member requires more arguments.", "Member '{0}' requires at least {1} arguments.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor CannotProvideInstanceForStaticMember =
            new( "CR0014", "Cannot provide instance for a static member.", "Member {0} is static, but has been used with a non-null instance.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor MustProvideInstanceForInstanceMember =
            new( "CR0015", "Has to provide instance for an instance member.", "Member {0} is not static, but has been used with a null instance.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor CannotAccessOpenGenericMember =
            new( "CR0016", "Cannot access an open generic member.", "Member {0} Cannot be accessed without specifying generic arguments.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor CannotProvideInstanceForLocalFunction =
            new( "CR0018", "Cannot provide instance for a local function.", "{0} is a local function, so it Cannot be invoked with a non-null instance.", _caravelaCategory, Error, true );

        public static readonly DiagnosticDescriptor CannotPassExpressionToByRefParameter =
            new( "CR0019", "Cannot use an expression in an out or ref parameter.",
                "Cannot pass the expression '{0}' to the '{1}' parameter of method '{2}' because the parameter is 'out' or 'ref'.", _caravelaCategory, Error,
                true );

        public static readonly DiagnosticDescriptor CannotFindType =
            new( "CR0020", "Cannot find a type", "Cannot find the type '{0}'.", _caravelaCategory, Error, true );
        
        public static readonly DiagnosticDescriptor CycleInAspectOrdering =
            new("CR0021", "A cycle was found in aspect ordering.", "A cycle was found in the specifications of aspect ordering between the following aspect part: {0}.", _caravelaCategory, Error, true);

        public static readonly DiagnosticDescriptor CannotAddChildAspectToPreviousPipelineStep = new(
            "CR0022",
            "Cannot add an aspect to a previous step of the compilation pipeline.",
            "The aspect {0} cannot add a child aspect to of type {1} because this aspect type has already been processed.",
            _caravelaCategory,
            Error,
            true );
        
        public static readonly DiagnosticDescriptor CannotAddAdviceToPreviousPipelineStep = new(
            "CR0023",
            "Cannot add an advice to a previous step of the compilation pipeline.",
            "The aspect {0} cannot add an advice to {1} because this declaration has already been processed.",
            _caravelaCategory,
            Error,
            true );
    }
}
