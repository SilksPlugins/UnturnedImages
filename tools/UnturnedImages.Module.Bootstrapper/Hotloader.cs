using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace UnturnedImages.Module.Bootstrapper
{
    // Credit to the OpenMod project for this Hotloader helper
    // https://github.com/openmod/OpenMod
    public static class Hotloader
    {
        private static readonly Dictionary<string, Assembly> Assemblies;

        /// <summary>
        /// Defines if hotloading is enabled.
        /// </summary>
        public static bool Enabled { get; set; }

        static Hotloader()
        {
            Assemblies = new Dictionary<string, Assembly>();
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private static Assembly? OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return GetAssembly(args.Name);
        }

        public static Assembly LoadAssembly(byte[] assemblyData)
        {
            return LoadAssembly(assemblyData, assemblySymbols: null);
        }

        public static Assembly LoadAssembly(byte[] assemblyData, byte[]? assemblySymbols)
        {
            if (!Enabled)
            {
                return Assembly.Load(assemblyData, assemblySymbols);
            }

            using var input = new MemoryStream(assemblyData, writable: false);
            using var output = new MemoryStream();

            var modCtx = ModuleDef.CreateModuleContext();
            var module = ModuleDefMD.Load(input, modCtx);

            var isMono = Type.GetType("Mono.Runtime") != null;
            var isStrongNamed = module.Assembly.PublicKey != null;

            if (!isMono && isStrongNamed)
            {
                // Don't hotload strong-named assemblies unless mono
                // Will cause FileLoadException's if not mono
                return Assembly.Load(assemblyData, assemblySymbols);
            }

            var realFullname = module.Assembly.FullName;

            if (Assemblies.ContainsKey(realFullname))
            {
                Assemblies.Remove(realFullname);
            }

            var guid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);
            var name = $"{module.Assembly.Name}-{guid}";

            module.Assembly.Name = name;
            module.Assembly.PublicKey = null;
            module.Assembly.HasPublicKey = false;

            module.Write(output);
            output.Seek(offset: 0, SeekOrigin.Begin);

            var newAssemblyData = output.ToArray();
            var assembly = Assembly.Load(newAssemblyData, assemblySymbols);
            Assemblies.Add(realFullname, assembly);
            return assembly;
        }

        public static Assembly? GetAssembly(string fullname)
        {
            if (Assemblies.TryGetValue(fullname, out var assembly))
            {
                return assembly;
            }

            var name = ReflectionExtensions.GetVersionIndependentName(fullname);

            foreach (var kv in Assemblies)
            {
                if (ReflectionExtensions.GetVersionIndependentName(kv.Key).Equals(name))
                {
                    return kv.Value;
                }
            }

            return null;
        }

        public static void ClearLoadedAssemblies()
        {
            Assemblies.Clear();
        }
    }
}