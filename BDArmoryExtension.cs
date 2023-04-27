using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HullBreach
{
    internal static class BDArmoryExtensions
    {
        internal static Type PartExtensions;
        private static bool isInstalled;
        static BDArmoryExtensions()
        {
            try
            {
                PartExtensions = AssemblyLoader.loadedAssemblies
                     .Where(a => a.name.Contains("BDArmory")).SelectMany(a => a.assembly.GetExportedTypes())
                     .SingleOrDefault(t => t.FullName == "BDArmory.Extensions.PartExtensions");
                isInstalled = true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
                isInstalled = false;
            }
        }


        internal static bool BDArmoryIsInstalled()
        {
            return isInstalled;
        }
    }
}
