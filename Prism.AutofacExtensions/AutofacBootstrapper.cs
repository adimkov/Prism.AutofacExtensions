using System;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;

namespace Microsoft.Practices.Prism.AutofacExtensions
{
    /// <summary>
    /// Base class that provides a basic bootstrapping sequence that
    /// registers most of the Composite Application Library assets
    /// in a <see cref="IContainer"/>.
    /// </summary>
    /// <remarks>
    /// This class must be overriden to provide application specific configuration.
    /// </remarks>
    public abstract class AutofacBootstrapper : Bootstrapper
    {
        private bool _useDefaultConfiguration = true;
        
        /// <summary>
        /// Gets the default <see cref="IContainer"/> for the application.
        /// </summary>
        /// <value>The default <see cref="IContainer"/> instance.</value>
        public IContainer Container { get; private set; }

        
        /// <summary>
        /// Creates the <see cref="IContainer"/> that will be used as the default container.
        /// </summary>
        /// <returns>A new instance of <see cref="IContainer"/>.</returns>
        protected virtual IContainer CreateContainer()
        {
            return ConfigureContainer().Build();
        }

        /// <summary>
        /// Configures the <see cref="IContainer"/>. May be overwritten in a derived class to add specific
        /// type mappings required by the application.
        /// </summary>
        protected virtual ContainerBuilder ConfigureContainer()
        {
            Logger.Log(AutofacExtensionsResource.ConfiguringAutofacContainer, Category.Debug, Priority.Low);
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(Logger);
            containerBuilder.RegisterInstance(ModuleCatalog);
            containerBuilder.RegisterSource(new ResolveAnythingRegistrationSource());

            if (_useDefaultConfiguration)
            {
                containerBuilder.Register(x => new AutofacServiceLocatorAdapter(Container)).As<IServiceLocator>().SingleInstance();
                containerBuilder.RegisterInstance(Container).As<IContainer>().SingleInstance();
                containerBuilder.RegisterType<ModuleInitializer>().As<IModuleInitializer>().SingleInstance();
                containerBuilder.RegisterType<ModuleManager>().As<IModuleManager>().SingleInstance();
                containerBuilder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

                containerBuilder.RegisterModule(new RegionInitializerModule());
                containerBuilder.RegisterModule(new RegionAdapterInitializerModule());
            }

            return containerBuilder;
        }

        /// <summary>
        /// Configures the LocatorProvider for the <see cref="ServiceLocator" />.
        /// </summary>
        protected override void ConfigureServiceLocator()
        {
            ServiceLocator.SetLocatorProvider(() => Container.Resolve<IServiceLocator>());
        }

        /// <summary>
        /// Registers in the <see cref="IContainer"/> the <see cref="Type"/> of the Exceptions
        /// that are not considered root exceptions by the <see cref="ExceptionExtensions"/>.
        /// </summary>
        protected override void RegisterFrameworkExceptionTypes()
        {
            base.RegisterFrameworkExceptionTypes();
            ExceptionExtensions.RegisterFrameworkExceptionType(typeof(DependencyResolutionException));
        }

        /// <summary>
        /// Initializes the modules. May be overwritten in a derived class to use a custom Modules Catalog
        /// </summary>
        protected override void InitializeModules()
        {
            IModuleManager manager;

            try
            {
                manager = Container.Resolve<IModuleManager>();
            }
            catch (ComponentNotRegisteredException ex)
            {
                if (ex.Message.Contains("IModuleCatalog"))
                {
                    throw new InvalidOperationException(AutofacExtensionsResource.ModuleCatalogNotRegistered);
                }

                throw;
            }
            catch (DependencyResolutionException ex)
            {
                if (ex.Message.Contains("IModuleCatalog"))
                {
                    throw new InvalidOperationException(AutofacExtensionsResource.ModuleCatalogNotResolved);
                }

                throw;
            }

            manager.Run();
        }

        /// <summary>
        /// Run the bootstrapper process.
        /// </summary>
        /// <param name="runWithDefaultConfiguration">If <see langword="true"/>, registers default Composite Application Library services in the container. This is the default behavior.</param>
        public override void Run(bool runWithDefaultConfiguration)
        {
            _useDefaultConfiguration = runWithDefaultConfiguration;

            Logger = CreateLogger();
            if (Logger == null)
            {
                throw new InvalidOperationException(AutofacExtensionsResource.NullLoggerFacadeException);
            }

            Logger.Log(AutofacExtensionsResource.LoggerCreatedSuccessfully, Category.Debug, Priority.Low);

            Logger.Log(AutofacExtensionsResource.CreatingModuleCatalog, Category.Debug, Priority.Low);
            ModuleCatalog = CreateModuleCatalog();
            if (ModuleCatalog == null)
            {
                throw new InvalidOperationException(AutofacExtensionsResource.NullModuleCatalogException);
            }

            Logger.Log(AutofacExtensionsResource.ConfiguringModuleCatalog, Category.Debug, Priority.Low);
            ConfigureModuleCatalog();

            Logger.Log(AutofacExtensionsResource.CreatingAutofacContainer, Category.Debug, Priority.Low);
            Container = CreateContainer();
            if (Container == null)
            {
                throw new InvalidOperationException(AutofacExtensionsResource.NullAutofacContainerException);
            }

            Logger.Log(AutofacExtensionsResource.ConfiguringServiceLocatorSingleton, Category.Debug, Priority.Low);
            ConfigureServiceLocator();

            Logger.Log(AutofacExtensionsResource.ConfiguringRegionAdapters, Category.Debug, Priority.Low);
            ConfigureRegionAdapterMappings();

            Logger.Log(AutofacExtensionsResource.ConfiguringDefaultRegionBehaviors, Category.Debug, Priority.Low);
            ConfigureDefaultRegionBehaviors();

            Logger.Log(AutofacExtensionsResource.RegisteringFrameworkExceptionTypes, Category.Debug, Priority.Low);
            RegisterFrameworkExceptionTypes();

            Logger.Log(AutofacExtensionsResource.CreatingShell, Category.Debug, Priority.Low);
            Shell = CreateShell();
            if (Shell != null)
            {
                Logger.Log(AutofacExtensionsResource.SettingTheRegionManager, Category.Debug, Priority.Low);
                RegionManager.SetRegionManager(Shell, Container.Resolve<IRegionManager>());

                Logger.Log(AutofacExtensionsResource.UpdatingRegions, Category.Debug, Priority.Low);
                RegionManager.UpdateRegions();

                Logger.Log(AutofacExtensionsResource.InitializingShell, Category.Debug, Priority.Low);
                InitializeShell();
            }

            if (Container.IsRegistered<IModuleManager>())
            {
                Logger.Log(AutofacExtensionsResource.InitializingModules, Category.Debug, Priority.Low);
                InitializeModules();
            }

            Logger.Log(AutofacExtensionsResource.BootstrapperSequenceCompleted, Category.Debug, Priority.Low);
        }
    }
}
