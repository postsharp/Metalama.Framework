using Caravela.Framework.Code;
using Caravela.Framework.Project;

namespace Caravela.Framework.Aspects
{
    public interface ITemplateContext
    {
        /// <summary>
        /// Gets the method metadata, or the accessor if this is a template for a field, property or event.
        /// </summary>
        /// <remarks>
        /// To invoke the method, use <see cref="IMethod.Invoke"/>,
        /// e.g. <c>OverrideMethodContext.Method.Invoke(1, 2, 3);</c>
        /// </remarks>
        IMethod Method { get; }

        /// <summary>
        /// Gets the target field or property, or null if the advice does not target a field or a property.
        /// </summary>
        //IProperty? Property { get; }

        /// <summary>
        /// Gets the target event, or null if the advice does not target an event.
        /// </summary>
        //IEvent? Event { get; }

        /// <summary>
        /// Gets the list parameters of <see cref="Method"/>.
        /// </summary>
        IAdviceParameterList Parameters { get; }

        // Gets the project configuration.
        //IProject Project { get; }

        /// <summary>
        /// Gets the code model of current type including the introductions of the current aspect type.
        /// </summary>
        IType Type { get; }

        /// <summary>
        /// Gets the code model of the whole compilation.
        /// </summary>
        ICompilation Compilation { get; }

        /// <summary>
        /// Gives access to the current type including members introduced by the current aspect.
        /// Both instance and static members are made accessible. For instance members, 
        /// the <c>this</c> instance is assumed.
        /// </summary>
        //dynamic This { get; }

        /// <summary>
        /// Gives access to the current type in the state it was before the current aspect.
        /// </summary>
        //dynamic Base { get; }

        // Gets the properties that were passed by the aspect initializer.
        //IReadOnlyDictionary<string, object> Properties { get; }
    }

    
}
