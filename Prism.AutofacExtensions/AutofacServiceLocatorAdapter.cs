using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Practices.ServiceLocation;

namespace Microsoft.Practices.Prism.AutofacExtensions
{
	public class AutofacServiceLocatorAdapter : ServiceLocatorImplBase
	{
		private readonly IContainer _container;

		/// <summary>
		/// Initializes a new instance of <see cref="AutofacServiceLocatorAdapter"/>.
		/// </summary>
		/// <param name="container">The <seealso cref="IContainer"/> that will be used
		/// by the <see cref="DoGetInstance"/> and <see cref="DoGetAllInstances"/> methods.</param>
		[CLSCompliant(false)]
		public AutofacServiceLocatorAdapter(IContainer container)
		{
			_container = container;
		}

		/// <summary>
		/// Resolves the instance of the requested service.
		/// </summary>
		/// <param name="serviceType">Type of instance requested.</param>
		/// <param name="key">Name of registered service you want. May be null.</param>
		/// <returns>The requested service instance.</returns>
		protected override object DoGetInstance(Type serviceType, string key)
		{
			if (key == null)
			{
				return _container.Resolve(serviceType);
			}
			return _container.ResolveNamed(key, serviceType);
		}

		/// <summary>
		/// Resolves all the instances of the requested service.
		/// </summary>
		/// <param name="serviceType">Type of service requested.</param>
		/// <returns>Sequence of service instance objects.</returns>
		protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
		{
			var type = typeof(IEnumerable<>).MakeGenericType(new[] { serviceType });
			return ((IEnumerable)_container.Resolve(type)).Cast<object>();
		}	
	}
}