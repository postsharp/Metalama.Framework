using Caravela.Framework.Aspects;
using System;

namespace Caravela.Patterns.Costura
{
    /// <summary>
    /// Add <c>[assembly: CosturaAspect]</c> anywhere in your source code to ensure that all references are packed into
    /// your main output assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class CosturaAspect : Attribute, IAspect
    {
        /// <summary>
        /// Returns true if all assemblies except excluded assemblies should be embedded. If this is false,
        /// then only assemblies specifically included should be embedded.
        /// </summary>
        public bool OptOut => IncludeAssemblies == null || IncludeAssemblies.Length == 0;

        /// <summary>
        /// If true, then .pdb files are also embedded. Default true.
        /// </summary>
        public bool IncludeDebugSymbols { get; set; }

        /// <summary>
        /// If true, then the assemblies embedded into the main assembly won't be compressed. Default false ("do compress").
        /// </summary>
        public bool DisableCompression { get; set; }

        /// <summary>
        /// This option doesn't work. If it did, it would control whether
        /// embedded assemblies are placed in the output folder anyway, even
        /// though they aren't necessary anymore.
        /// </summary>
        public bool DisableCleanup { get; set; }

        /// <summary>
        /// If true, then Packer will bootstrap itself in your assembly's module initializer and you don't need to
        /// call <see cref="PackerUtility.Initialize"/>. Default true ("load automatically").
        /// </summary>
        //public bool LoadAtModuleInit { get; set; }

        /// <summary>
        /// This will copy embedded files to disk before loading them into
        /// memory. This is helpful for some scenarios that expected an
        /// assembly to be loaded from a physical file. For example, if some
        /// code checks the assembly's assembly location. Default false.
        /// </summary>
        public bool CreateTemporaryAssemblies { get; set; }

        /// <summary>
        /// This add-in will by default use assemblies with a name
        /// like 'resources.dll' as a satellite resource and prepend
        /// the output path. This flag disables that behavior.
        /// Be advised, that DLL project assembly names ending
        /// with '.resources' (resulting in *.resources.dll) will
        /// lead to errors when this flag set to false. Default false.
        /// </summary>
        public bool IgnoreSatelliteAssemblies { get; set; }

        /// <summary>
        /// A list of assembly names to embed. Do not include .exe or .dll
        /// in the names. Can use wildcards at the end of the name for
        /// partial matching. If you don't set this, all Copy Local references
        /// are embedded.
        /// </summary>
        public string[] IncludeAssemblies { get; set; }

        /// <summary>
        /// A list of assembly names to exclude from embedding.
        /// Can use wildcards for partial assembly name matching.
        /// For example System.* will exclude all assemblies that start with System..
        /// Wildcards may only be used at the end of an entry so
        /// for example, System.*.Private.* would not work.
        /// Do not include .exe or .dll in the names.
        /// </summary>
        public string[] ExcludeAssemblies { get; set; }

        /// <summary>
        /// Mixed-mode assemblies cannot be loaded the same way
        /// as managed assemblies. Use this property for those assemblies instead.
        /// </summary>
        public string[] Unmanaged32Assemblies { get; set; }

        /// <summary>
        /// Mixed-mode assemblies cannot be loaded the same way
        /// as managed assemblies. Use this property for those assemblies instead.
        /// </summary>
        public string[] Unmanaged64Assemblies { get; set; }

        /// <summary>
        /// Native libraries can be loaded by this add-in automatically.
        /// To include a native library include it in your project as an
        /// Embedded Resource in a folder called costura32 or costura64
        /// depending on the bittyness of the library.
        /// Optionally you can also specify the order that preloaded
        /// libraries are loaded. When using temporary assemblies
        /// from disk mixed mode assemblies are also preloaded.
        /// </summary>
        public string[] PreloadOrder { get; set; }
    }
}