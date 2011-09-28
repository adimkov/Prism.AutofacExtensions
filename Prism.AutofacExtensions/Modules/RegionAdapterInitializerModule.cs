// ---------------------------------------------------------------------------------
// <copyright file="RegionAdapterInitializerModule.cs" author="Anton Dimkov">
//   Copyright (c) Anton Dimkov 2011. All rights reserved.  
// </copyright>
// <summary>
//  Module to register region adapters.
// </summary>
// ---------------------------------------------------------------------------------

namespace Microsoft.Practices.Prism.AutofacExtensions.Modules
{
    using Autofac;

    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// Module to register region adapters.
    /// </summary>
    public class RegionAdapterInitializerModule : Module
    {
        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
#if SILVERLIGHT
            builder.RegisterType<TabControlRegionAdapter>();
#endif
            builder.RegisterType<SelectorRegionAdapter>();
            builder.RegisterType<ItemsControlRegionAdapter>();
            builder.RegisterType<ContentControlRegionAdapter>();
        }
    }
}