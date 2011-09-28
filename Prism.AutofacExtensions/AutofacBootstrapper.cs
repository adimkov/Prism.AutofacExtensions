// ---------------------------------------------------------------------------------
// <copyright file="AutofacBootstrapper.cs" author="Anton Dimkov">
//   Copyright (c) Anton Dimkov 2011. All rights reserved.  
// </copyright>
// <summary>
//  Base class that provides a basic bootstrapping sequence that registers most of the Composite Application Library assets in a IContainer.
// </summary>
// ---------------------------------------------------------------------------------

namespace Microsoft.Practices.Prism.AutofacExtensions
{
    using System;
    using Autofac;
    using Autofac.Core;
    using Autofac.Core.Registration;
    using Microsoft.Practices.Prism.AutofacExtensions.Modules;
    using Microsoft.Practices.Prism.Events;
    using Microsoft.Practices.Prism.Logging;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Base class that provides a basic bootstrapping sequence that registers most of the Composite Application Library assets in a <see cref="IContainer"/>.
    /// </summary>
    /// <remarks>
    /// This class must be overriden to provide application specific configuration.
    /// </remarks>
    public abstract class AutofacBootstrapper : Bootstrapper
    {
        /// <summary>
        /// Flag that indicating to use default container settings.
        /// </summary>
        private bool _useDefaultConfiguration = true;
        
        /// <summary>
        /// Gets the default <see cref="IContainer"/> for the application.
        /// </summary>
        /// <value>The default <see cref="IContainer"/> instance.</value>
        public IContainer Container { get; private set; }

        /// <summary>
        /// Run the bootstrapper process.
        /// </summary>
        /// <param name="runWithDefaultConfiguration">If <see langword="true"/>, registers default Composite Application Library services in the container. This is the default behavior.</param>
        public override void Run(bool runWithDefaultConfiguration)
        {
            this._useDefaultConfiguration = runWithDefaultConfiguration;

            this.Logger = this.CreateLogger();
            if (this.Logger == null)
            {
                throw new InvalidOperationException(AutofacExtensionsResource.NullLoggerFacadeException);
            }

            this.Logger.Log(AutofacExtensionsResource.LoggerCreatedSuccessfully, Category.Debug, Priority.Low);

            this.Logger.Log(AutofacExtensionsResource.CreatingModuleCatalog, Category.Debug, Priority.Low);
            this.ModuleCatalog = this.CreateModuleCatalog();
            if (this.ModuleCatalog == null)
            {
                throw new InvalidOperationException(AutofacExtensionsResource.NullModuleCatalogException);
            }

            this.Logger.Log(AutofacExtensionsResource.ConfiguringModuleCatalog, Category.Debug, Priority.Low);
            this.ConfigureModuleCatalog();

            this.Logger.Log(AutofacExtensionsResource.CreatingAutofacContainer, Category.Debug, Priority.Low);
            this.Container = this.CreateContainer();
            if (this.Container == null)
            {
                throw new InvalidOperationException(AutofacExtensionsResource.NullAutofacContainerException);
            }

            this.Logger.Log(AutofacExtensionsResource.ConfiguringServiceLocatorSingleton, Category.Debug, Priority.Low);
            this.ConfigureServiceLocator();

            this.Logger.Log(AutofacExtensionsResource.ConfiguringRegionAdapters, Category.Debug, Priority.Low);
            this.ConfigureRegionAdapterMappings();

            this.Logger.Log(AutofacExtensionsResource.ConfiguringDefaultRegionBehaviors, Category.Debug, Priority.Low);
            this.ConfigureDefaultRegionBehaviors();

            this.Logger.Log(AutofacExtensionsResource.RegisteringFrameworkExceptionTypes, Category.Debug, Priority.Low);
            this.RegisterFrameworkExceptionTypes();

            this.Logger.Log(AutofacExtensionsResource.CreatingShell, Category.Debug, Priority.Low);
            this.Shell = this.CreateShell();
            if (this.Shell != null)
            {
                this.Logger.Log(AutofacExtensionsResource.SettingTheRegionManager, Category.Debug, Priority.Low);
                RegionManager.SetRegionManager(this.Shell, this.Container.Resolve<IRegionManager>());

                this.Logger.Log(AutofacExtensionsResource.UpdatingRegions, Category.Debug, Priority.Low);
                RegionManager.UpdateRegions();

                this.Logger.Log(AutofacExtensionsResource.InitializingShell, Category.Debug, Priority.Low);
                this.InitializeShell();
            }

            if (this.Container.IsRegistered<IModuleManager>())
            {
                this.Logger.Log(AutofacExtensionsResource.InitializingModules, Category.Debug, Priority.Low);
                this.InitializeModules();
            }

            this.Logger.Log(AutofacExtensionsResource.BootstrapperSequenceCompleted, Category.Debug, Priority.Low);
        }

        /// <summary>
        /// Creates the <see cref="IContainer"/> that will be used as the default container.
        /// </summary>
        /// <returns>A new instance of <see cref="IContainer"/>.</returns>
        protected virtual IContainer CreateContainer()
        {
            return ConfigureContainer().Build();
        }

        /// <summary>
        /// Configures the <see cref="IContainer"/>. May be overwritten in a derived class to add specific type mappings required by the application.
        /// </summary>
        /// <returns>The <see cref="ContainerBuilder"/> of <see cref="IContainer"/>.</returns>
        protected virtual ContainerBuilder ConfigureContainer()
        {
            Logger.Log(AutofacExtensionsResource.ConfiguringAutofacContainer, Category.Debug, Priority.Low);
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(Logger);
            containerBuilder.RegisterInstance(ModuleCatalog);
            containerBuilder.RegisterSource(new AutowiringRegistrationSource());

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
        /// Initializes the modules. May be overwritten in a derived class to use a custom Modules Catalog.
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
    }
}
