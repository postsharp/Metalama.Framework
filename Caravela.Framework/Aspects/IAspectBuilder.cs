using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Aspects
{

    /// <summary>
    /// An object by the <see cref="IAspect{T}.Initialize"/> method of the aspect to provide advices and child
    /// aspects. This is a weakly-typed variant of the <see cref="IAspectBuilder{T}"/> interface.
    /// </summary>
    public interface IAspectBuilder : IDiagnosticSink
    {
        /// <summary>
        /// Gets the declaration to which the aspect was added.
        /// </summary>
        ICodeElement TargetDeclaration { get; }

        /// <summary>
        /// Gets an object that exposes methods that allow to create advices.
        /// </summary>
        IAdviceFactory AdviceFactory { get; }

        /// <summary>
        /// Skips the application of the aspect to the code. Any provided advice is ignored, but provided children aspects
        /// and diagnostics are preserved. 
        /// </summary>
        /// <remarks>
        /// Note that reporting an error using
        /// <see cref="IDiagnosticSink.ReportDiagnostic(Caravela.Framework.Diagnostics.Severity,Caravela.Framework.Diagnostics.IDiagnosticLocation?,string,string,object[])"/>
        /// automatically causes the aspect to be skipped, but, additionally, provided children aspects are ignored.
        /// </remarks>
        void SkipAspect();
    }

    /// <summary>
    /// An object by the <see cref="IAspect{T}.Initialize"/> method of the aspect to provide advices and child
    /// aspects. This is the strongly-typed variant of the <see cref="IAspectBuilder"/> interface.
    /// </summary>
    public interface IAspectBuilder<out T> : IAspectBuilder
        where T : ICodeElement
    {
        /// <summary>
        /// Gets the declaration to which the aspect was added.
        /// </summary>
        new T TargetDeclaration { get; }
    }
}