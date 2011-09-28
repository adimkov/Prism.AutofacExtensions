// ---------------------------------------------------------------------------------
// <copyright file="RegionInitializerModule.cs" author="Anton Dimkov">
//   Copyright (c) Anton Dimkov 2011. All rights reserved.  
// </copyright>
// <summary>
//  Module to register region services.
// </summary>
// ---------------------------------------------------------------------------------

namespace Microsoft.Practices.Prism.AutofacExtensions.Modules
{
    using Autofac;

    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// Module to register region services.
    /// </summary>
    public class RegionInitializerModule : Module
    {
        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RegionManager>().As<IRegionManager>().SingleInstance();
            builder.RegisterType<RegionAdapterMappings>().SingleInstance();
            builder.RegisterType<RegionViewRegistry>().As<IRegionViewRegistry>().SingleInstance();
            builder.RegisterType<RegionBehaviorFactory>().As<IRegionBehaviorFactory>().SingleInstance();
            builder.RegisterType<RegionNavigationJournalEntry>().As<IRegionNavigationJournalEntry>();
            builder.RegisterType<RegionNavigationJournal>().As<IRegionNavigationJournal>();
            builder.RegisterType<RegionNavigationService>().As<IRegionNavigationService>();
            builder.RegisterType<RegionNavigationContentLoader>().As<IRegionNavigationContentLoader>().SingleInstance();
        }
    }
}