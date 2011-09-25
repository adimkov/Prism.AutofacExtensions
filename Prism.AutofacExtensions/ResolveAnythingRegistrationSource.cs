using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;

namespace Microsoft.Practices.Prism.AutofacExtensions
{
	public class ResolveAnythingRegistrationSource: IRegistrationSource
	{
		public IEnumerable<IComponentRegistration> RegistrationsFor( Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
			var ts = service as TypedService;
			if (ts != null && !ts.ServiceType.IsAbstract && ts.ServiceType.IsClass)
			{
				var rb = RegistrationBuilder.ForType(ts.ServiceType);
				return new[] { RegistrationBuilder.CreateRegistration(rb) };
			}

			return Enumerable.Empty<IComponentRegistration>();
		}

		public bool IsAdapterForIndividualComponents
		{
			get { return true; }
		}
	}
	
}