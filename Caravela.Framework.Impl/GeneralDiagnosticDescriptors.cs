using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Caravela.Framework.Impl
{
    static class GeneralDiagnosticDescriptors
    {
        // Reserved range 0-99

        private const string caravelaCategory = nameof( Caravela );

        public static DiagnosticDescriptor UncaughtException =
            new( "CR0001", "Unexpected exception in Caravela.", "Unexpected exception occurred in Caravela: {0}", caravelaCategory, Error, true );
        public static DiagnosticDescriptor ErrorBuildingCompileTimeAssembly =
            new( "CR0002", "Error while building compile-time assembly.", "Error occurred while building compile-time assembly.", caravelaCategory, Error, true );
        public static DiagnosticDescriptor AspectAppliedToIncorrectElement =
            new( "CR0003", "Aspect applied to incorrect kind of element.", "Aspect {0} cannot be applied to element {1}, because it is a {2}.", caravelaCategory, Error, true );
        public static DiagnosticDescriptor AspectHasMoreThanOneWeaver =
            new( "CR0004", "Aspect has more than one weaver.", "Aspect {0} can have at most one weaver, but it has the following: {1}.", caravelaCategory, Error, true );
        public static DiagnosticDescriptor ErrorProcessingTemplates =
            new( "CR0004", "Error while processing templates.", "Error occurred while processing templates.", caravelaCategory, Error, true );
        public static DiagnosticDescriptor ExceptionInWeaver =
            new( "CR0005", "Exception in aspect weaver.", "Exception ocurred while executing the aspect weaver {0}: {1}", caravelaCategory, Error, true );   
        public static DiagnosticDescriptor UnsupportedSerialization =
            new( "CR0006", "Build-time code not serializable.", "Build-time code attempted to create {0} but no serializer is registered for that type.", caravelaCategory, Error, true );
        public static DiagnosticDescriptor CycleInSerialization =
            new( "CR0007", "A collection contains itself.", "Build-time code attempted to create a collection which contains itself: {0} ", caravelaCategory, Error, true );
        public static DiagnosticDescriptor MultidimensionalArray =
            new( "CR0008", "Multidimensional arrays not supported.", "Build-time array {0} has more than one dimension.", caravelaCategory, Error, true );
        public static DiagnosticDescriptor UnsupportedDictionaryComparer =
            new( "CR0009", "Custom equality comparers not supported.", "Build-time dictionary has an equality comparer {0} which is not supported. Only the default comparer and predefined string comparers are supported.", caravelaCategory, Error, true );
        
        public static DiagnosticDescriptor MoreThanOneAdvicePerElement =
            new( "CR0099", "More than one advice per code element.", "'{0}' has more than one advice, which is currently not supported.", caravelaCategory, Error, true );
    }
}
