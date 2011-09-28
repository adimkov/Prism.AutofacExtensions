// ---------------------------------------------------------------------------------
// <copyright file="AutowiringRegistrationSource.cs" author="Anton Dimkov">
//   Copyright (c) Anton Dimkov 2011. All rights reserved.  
// </copyright>
// <summary>
//  Automatically register in container required a class
// </summary>
// ---------------------------------------------------------------------------------

namespace Microsoft.Practices.Prism.AutofacExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Autofac.Builder;
    using Autofac.Core;

    /// <summary>
    /// Automatically register in container required a class.
    /// </summary>
    public class AutowiringRegistrationSource : IRegistrationSource
    {
        /// <summary>
        /// Gets whether the registrations provided by this source are 1:1 adapters on top
        /// of other components (I.e. like Meta, Func or Owned.)
        /// </summary>
        public bool IsAdapterForIndividualComponents
        {
            get { return true; }
        }

        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>
        /// Registrations providing the service.
        /// </returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var ts = service as TypedService;
            if (ts != null && !ts.ServiceType.IsAbstract && ts.ServiceType.IsClass)
            {
                var rb = RegistrationBuilder.ForType(ts.ServiceType);
                return new[] { RegistrationBuilder.CreateRegistration(rb) };
            }

            return Enumerable.Empty<IComponentRegistration>();
        }
    }
}