using System;
using System.Text.RegularExpressions;

namespace UnturnedImages.Module.Bootstrapper
{
    public static class ReflectionExtensions
    {
        public static string GetVersionIndependentName(string assemblyName)
        {
            return GetVersionIndependentName(assemblyName, out _);
        }

        private static readonly Regex VersionRegex = new("Version=(?<version>.+?), ", RegexOptions.Compiled);
        public static string GetVersionIndependentName(string assemblyName, out string extractedVersion)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException(nameof(assemblyName));
            }

            var match = VersionRegex.Match(assemblyName);
            extractedVersion = match.Groups[1].Value;
            return VersionRegex.Replace(assemblyName, string.Empty);
        }
    }
}
