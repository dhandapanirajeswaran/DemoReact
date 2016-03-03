using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface IFactory
    {
		T Create<T>(CreationMethod method = CreationMethod.New, params object[] parameters);
		T Create<T>(CreationMethod method = CreationMethod.New) where T : new();
    }

    public enum CreationMethod {
		New,
        Activator,
        ServiceLocator
    }

	public class Factory : IFactory
    {
		public T Create<T>(CreationMethod method)
			where T : new()
		{
			T instance;

			switch (method)
			{
				default:
				case CreationMethod.New:
					instance = new T();
					break;
				case CreationMethod.Activator:
					instance = Activator.CreateInstance<T>();
					break;
				case CreationMethod.ServiceLocator:
					instance = ServiceLocator.Current.GetInstance<T>();
					break;
			}

			return instance;
		}

        public T Create<T>(CreationMethod method, params object[] parameters)
        {
			T instance;

            switch(method)
            {
				default:
				case CreationMethod.Activator:
					instance = (T)Activator.CreateInstance(typeof(T), parameters);
					break;
				case CreationMethod.ServiceLocator:
					instance = ServiceLocator.Current.GetInstance<T>();
					break;
				case CreationMethod.New:
					throw new NotSupportedException();
            }

			return instance;
        }
    }
}
