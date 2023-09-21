// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Options;

/// <summary>
/// Base interface for hierarchical options. Hierarchical options are options exposed and consumed by
/// aspects and fabrics. Users can typically set options at different levels of the code level: for the
/// whole project, for a namespace, for a type, or for a member, as decided by the aspect author.
/// </summary>
/// <remarks>
/// <para>
///  Users typically set options from a fabric using the <see cref="IAspectReceiver{TDeclaration}.SetOptions{TOptions}"/>
/// method of the <see cref="IAmender{T}.Outbound"/> object.
/// </para>
/// <para>
///  Implementations of this class must be immutable. An instance of this object represents a <i>layer</i> of options,
///  and these layers are merged by the <see cref="IOverridable.OverrideWith"/> method. Therefore, all properties should typically be nullable or
///  support another representation of being unset.
/// </para>
/// <para>
/// Aspects typically use the <see cref="IAspectBuilder{TAspectTarget}.GetOptions{T}"/> method of the <see cref="IAspectBuilder{TAspectTarget}"/>
/// to get the options applying to the current aspect instance. This method is also exposed on the <see cref="IAspectInstance"/> interface.
/// This service is also exposed by the <see cref="DeclarationExtensions.Enhancements{T}"/> method, but the result does not include the options
/// set by the aspect itself.
/// </para>
/// <para>
/// Classes that implement this interface must implement the <see cref="IHierarchicalOptions{T}"/> generic interface where <c>T</c> is
/// the type of declarations for which the user is allowed to set the options. Typically, a single class would implement several instances
/// of this instance. For instance, for an option affecting a method-level aspect, a good practice is to implement this interface
/// for <see cref="ICompilation"/>, <see cref="INamespace"/>, <see cref="INamedType"/> and <see cref="IMethod"/>.
/// </para>
/// <para>
/// Classes that implement this interface can be annotated with the <see cref="HierarchicalOptionsAttribute"/> custom attribute,
/// which allows authors to customize the inheritance mechanisms of the option.
/// </para>
/// </remarks>
public interface IHierarchicalOptions : IOverridable, ICompileTimeSerializable
{
    /// <summary>
    /// Gets the default options from the current project. 
    /// </summary>
    /// <param name="project">The current project.</param>
    /// <returns>The default options for the given project.</returns>
    /// <remarks>
    /// <para>
    ///  If the aspect supports parameters supplied as MSBuild project properties, the implementation of this
    ///  method should read these properties and assign their values to the returned object.
    /// </para>
    /// </remarks>
    IHierarchicalOptions GetDefaultOptions( IProject project );
}

/// <summary>
/// An interface, derived from the non-generic <see cref="IHierarchicalOptions"/>, that means that the options can be set
/// on the type of declarations specified by the generic parameter.
/// </summary>
/// <typeparam name="T">The type of declarations on which the options can be set or read.</typeparam>
/// <remarks>
///  See the remarks for the non-generic <see cref="IHierarchicalOptions"/> interface.
/// </remarks>
/// <seealso cref="IHierarchicalOptions"/>
public interface IHierarchicalOptions<in T> : IHierarchicalOptions
    where T : class, IDeclaration { }