using SDG.Framework.Modules;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnturnedImages.Module.Bootstrapper
{
    // Credit to the OpenMod project for this bootstrapper module
    // https://github.com/openmod/OpenMod
    public class BootstrapperModule : IModuleNexus
    {
        private IModuleNexus? _bootstrappedModule;
        public static BootstrapperModule? Instance { get; private set; }
        private static string? _selfLocation;

        private Assembly? OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            UnturnedLog.info("Resolving assembly: " + args.Name + " from " + args.RequestingAssembly.FullName);

            return null;
        }

        public void initialize()
        {
            Instance = this;

            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

                if (string.IsNullOrEmpty(_selfLocation))
                {
                    _selfLocation = Path.GetFullPath(typeof(BootstrapperModule).Assembly.Location);
                }

                var unturnedImagesModuleDir = Path.GetDirectoryName(_selfLocation)!;
                Assembly? moduleAssembly = null;

                Hotloader.Enabled = true;
                Hotloader.ClearLoadedAssemblies();

                foreach (var assemblyFilePath in Directory.GetFiles(unturnedImagesModuleDir, "*.dll",
                    SearchOption.TopDirectoryOnly))
                {
                    if (assemblyFilePath == _selfLocation)
                    {
                        continue;
                    }

                    var fileName = Path.GetFileName(assemblyFilePath);
                    var assemblyData = File.ReadAllBytes(assemblyFilePath);

                    if (fileName.Equals("UnturnedImages.Module.dll"))
                    {
                        moduleAssembly = Hotloader.LoadAssembly(File.ReadAllBytes(assemblyFilePath));
                    }
                    else
                    {
                        Assembly.Load(assemblyData);
                    }
                }

                if (moduleAssembly == null)
                {
                    throw new Exception("Failed to find UnturnedImages module assembly!");
                }

                ICollection<Type> types;
                try
                {
                    types = moduleAssembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(d => d != null).ToList();
                }

                var moduleType = types.SingleOrDefault(d => d.Name.Equals("UnturnedImagesModule"));
                if (moduleType == null)
                {
                    throw new Exception($"Failed to find UnturnedImagesModule class in {moduleAssembly}!");
                }

                _bootstrappedModule = (IModuleNexus)Activator.CreateInstance(moduleType);
                _bootstrappedModule.initialize();
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
            }
        }

        public void shutdown()
        {
            _bootstrappedModule?.shutdown();
            Instance = null;
        }
    }
}
