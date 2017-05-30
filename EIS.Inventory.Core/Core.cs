using System;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.IO;
using SimpleInjector;
using System.Reflection;

namespace EIS.Inventory.Core
{
    public static class Core
    {
        public static ICoreResolver Resolver = null;
        public static CompositionContainer Container = null;

        public static T Get<T>() where T : class
        {
            if (Resolver == null)
                throw new NullReferenceException("The ICoreResolver reference of the Core must be set");

            return Resolver.Get<T>();
        }

        public static object Get(Type type)
        {
            if (Resolver == null)
                throw new NullReferenceException("The ICoreResolver reference of the Core must be set");

            return Resolver.Get(type);
        }

        public static Container GetContainer()
        {
            if (Resolver == null)
                throw new NullReferenceException("The ICoreResolver reference of the Core must be set");

            return Resolver.Container;
        }

        public static void RegisterMEF()
        {
            // load the catalog path from the config file
            var path = ConfigurationManager.AppSettings["ExtensionPath"];
            if (path == null)
                throw new ConfigurationErrorsException("The ExtensionPath setting is missing from the appSettings section of the config file");

            //var catalog = new DirectoryCatalog(path);
            var catalog = new SafeDirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, path);
            Container = new CompositionContainer(catalog, true); // threadSafe = true
        }
    }
}
