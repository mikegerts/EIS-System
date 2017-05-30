using System;
using SimpleInjector;

namespace EIS.Inventory.Core
{
    public interface ICoreResolver
    {
        T Get<T>() where T : class;

        object Get(Type type);      

        Container Container { get; }
    }

    public class CoreResolver : ICoreResolver
    {
        private readonly Container _container;

        public CoreResolver(Container container)
        {
            _container = container;
        }

        public T Get<T>() where T : class
        {
            return _container.GetInstance<T>();
        }

        public object Get(Type type)
        {
            return _container.GetInstance(type);
        }

        public Container Container
        {
            get { return _container; }
        }
    }
}
