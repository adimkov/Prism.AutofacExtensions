using Autofac;
using Microsoft.Practices.Prism.Regions;

namespace Microsoft.Practices.Prism.AutofacExtensions
{
	public class RegionAdapterInitializerModule : Module
	{
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