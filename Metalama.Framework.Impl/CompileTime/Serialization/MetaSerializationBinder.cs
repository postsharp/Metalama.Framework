// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Binds types to names and names to types. Used by the <see cref="MetaFormatter"/>.
    /// </summary>
    public class MetaSerializationBinder
    {
        // TODO: Remove.
        // private readonly IReflectionBindingManagerService reflectionBindingManagerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaSerializationBinder"/> class.
        /// </summary>
        public MetaSerializationBinder()
        {
            // TODO: Remove.
            // if ( PostSharpEnvironment.IsPostSharpRunning )
            // {
            //    this.reflectionBindingManagerService = PostSharpEnvironment.CurrentProject.GetService<IReflectionBindingManagerService>();
            // }
        }

        /// <summary>
        /// Gets a <see cref="Type"/> given a type name and an assembly name.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>The required <see cref="Type"/>.</returns>
        public virtual Type BindToType( string typeName, string assemblyName )
        {
            return Type.GetType( ReflectionHelper.GetAssemblyQualifiedTypeName( typeName, assemblyName ) );
        }

        /// <summary>
        /// Gets the name and the assembly name of a given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <param name="typeName">At output, the name of <paramref name="type"/>.</param>
        /// <param name="assemblyName">At output, the name of <paramref name="assemblyName"/>.</param>
        public virtual void BindToName( Type type, out string typeName, out string assemblyName )
        {
            typeName = type.FullName;

            // TODO: Remove.
            // if ( this.reflectionBindingManagerService != null )
            // {
            //    assemblyName = this.reflectionBindingManagerService.ResolveAssembly(type) ?? type.GetAssembly().FullName;
            // }
            // else
            // {
            assemblyName = type.Assembly.FullName;

            // }
        }
    }
}