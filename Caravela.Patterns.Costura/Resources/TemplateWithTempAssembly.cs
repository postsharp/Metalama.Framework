﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Caravela.Patterns.Costura.Templates
{
    internal static class TemplateWithTempAssembly
    {
        static object nullCacheLock = new object();
        static Dictionary<string, bool> nullCache = new Dictionary<string, bool>();

        static string tempBasePath;

        static List<string> preloadList = new List<string>();
        static List<string> preload32List = new List<string>();
        static List<string> preload64List = new List<string>();

        static Dictionary<string, string> checksums = new Dictionary<string, string>();

        static int isAttached;

#pragma warning disable 649
        private static string md5Hash;
#pragma warning restore 649

        public static void Attach()
        {
            if (Interlocked.Exchange(ref isAttached, 1) == 1)
            {
                return;
            }

            //Create a unique Temp directory for the application path.
            var prefixPath = Path.Combine(Path.GetTempPath(), "Costura");
            tempBasePath = Path.Combine(prefixPath, md5Hash);

            // Preload
            var unmanagedAssemblies = IntPtr.Size == 8 ? preload64List : preload32List;
            var libList = new List<string>();
            libList.AddRange(unmanagedAssemblies);
            libList.AddRange(preloadList);
            Common.PreloadUnmanagedLibraries(md5Hash, tempBasePath, libList, checksums);

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += ResolveAssembly;
        }

        public static Assembly ResolveAssembly(object sender, ResolveEventArgs e)
        {
            lock (nullCacheLock)
            {
                if (nullCache.ContainsKey(e.Name))
                {
                    return null;
                }
            }

            var requestedAssemblyName = new AssemblyName(e.Name);

            var assembly = Common.ReadExistingAssembly(requestedAssemblyName);
            if (assembly != null)
            {
                return assembly;
            }

            Common.Log("Loading assembly '{0}' into the AppDomain", requestedAssemblyName);

            assembly = Common.ReadFromDiskCache(tempBasePath, requestedAssemblyName);
            if (assembly == null)
            {
                lock (nullCacheLock)
                {
                    nullCache[e.Name] = true;
                }

                // Handles re-targeted assemblies like PCL
                if ((requestedAssemblyName.Flags & AssemblyNameFlags.Retargetable) != 0)
                {
                    assembly = Assembly.Load(requestedAssemblyName);
                }
            }
            return assembly;
        }
    }
}
