using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Glimpse.Core.Framework;
using G = Glimpse;

namespace SmartStore.Glimpse.Infrastructure
{

    public class GlimpseServiceLocator : IServiceLocator
    {
        private static readonly Assembly[] s_scannableAssemblies = new Assembly[]
        {
            typeof(G.Settings).Assembly,
            typeof(G.AspNet.AspNetServiceLocator).Assembly,
            typeof(G.Mvc.Model.MvcDisplayModel).Assembly,
            //typeof(G.Ado.Initialize).Assembly,
            //typeof(G.EF.Initialize).Assembly,
            typeof(GlimpseServiceLocator).Assembly
        };

        private static string BaseDirectory
        {
            get
            {
                AppDomainSetup setupInformation = AppDomain.CurrentDomain.SetupInformation;
                if (!string.Equals(setupInformation.ShadowCopyFiles, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return AppDomain.CurrentDomain.BaseDirectory;
                }
                return Path.Combine(setupInformation.CachePath, setupInformation.ApplicationName);
            }
        }

        public ICollection<T> GetAllInstances<T>() where T : class
        {
            var baseDir = BaseDirectory;

            var instances = new List<T>();
            foreach (var assembly in s_scannableAssemblies)
            {
                var probePath = Path.Combine(baseDir, assembly.GetName().Name + ".dll");
                var probeAssembly = Assembly.LoadFrom(probePath);

                Type[] types;
                try
                {
                    types = probeAssembly.GetTypes();
                }
                catch
                {
                    types = Enumerable.Empty<Type>().ToArray();
                }

                var concreteTypes = from type in types
                                    where ((typeof(T).IsAssignableFrom(type) && !type.IsInterface) && !type.IsAbstract)
                                    select type;

                foreach (var type in concreteTypes)
                {
                    try
                    {
                        T item = (T)Activator.CreateInstance(type);
                        instances.Add(item);
                    }
                    catch
                    {
                        continue;
                    }
                }

            }

            return instances.Any() ? instances : null;
        }

        public T GetInstance<T>() where T : class
        {
            // let Glimpse do the work
            return null;
        }
    }

}
