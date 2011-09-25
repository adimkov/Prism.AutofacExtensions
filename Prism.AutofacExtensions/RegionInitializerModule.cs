using Autofac;
using Microsoft.Practices.Prism.Regions;

namespace Microsoft.Practices.Prism.AutofacExtensions
{
	public class RegionInitializerModule : Module
	{
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