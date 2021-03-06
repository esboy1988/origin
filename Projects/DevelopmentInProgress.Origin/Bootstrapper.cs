﻿//-----------------------------------------------------------------------
// <copyright file="Bootstrapper.cs" company="Development In Progress Ltd">
//     Copyright © 2012. All rights reserved.
// </copyright>
// <author>Grant Colley</author>
//-----------------------------------------------------------------------

using DevelopmentInProgress.Origin.RegionAdapters;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using Xceed.Wpf.AvalonDock;

namespace DevelopmentInProgress.Origin
{
    /// <summary>
    /// The Bootstrapper class is responsible for initializing an application using the Prism library.
    /// </summary>
    public class Bootstrapper : UnityBootstrapper
    {
        /// <summary>
        /// Create and returns an object that implements the Microsoft.Practices.Prism.Logging.ILoggerFacade.
        /// </summary>
        /// <returns>A new instance of the logger.</returns>
        protected override ILoggerFacade CreateLogger()
        {
            return new LoggerFacade.LoggerFacade();
        }

        /// <summary>
        /// Load, configure and return the module catalog at startup to ensure  
        /// any module dependencies are available before the module is used.
        /// </summary>
        /// <returns>The module catalog.</returns>
        protected override IModuleCatalog CreateModuleCatalog()
        {
            using (Stream xamlStream = File.OpenRead("Configuration/ModuleCatalog.xaml"))
            {
                var moduleCatalog = Microsoft.Practices.Prism.Modularity.ModuleCatalog.CreateFromXaml(xamlStream);
                return moduleCatalog;
            }
        }

        /// <summary>
        /// Load Unity configurations into the container. The unity configuration file declares  
        /// and registers services (objects) that will be available via the service locator. 
        /// </summary>
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            
            var files = from f in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Configuration"))
                        where f.ToUpper().EndsWith("UNITY.CONFIG") select f;
            foreach (string fileName in files)
            {
                var unityMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = fileName
                };

                var unityConfig = ConfigurationManager.OpenMappedExeConfiguration(unityMap, ConfigurationUserLevel.None);
                var unityConfigSection = (UnityConfigurationSection)unityConfig.GetSection("unity");
                unityConfigSection.Configure(Container);
            }
        }

        /// <summary>
        /// Map the custom region adaptors. This enables controls as prism regions.
        /// </summary>
        /// <returns>The region adapter mappings used by prism.</returns>
        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            var mappings = base.ConfigureRegionAdapterMappings();
            mappings.RegisterMapping(typeof(DockingManager), new DockingManagerRegionAdapter(ServiceLocator.Current.GetInstance<IRegionBehaviorFactory>()));
            return mappings;
        } 

        /// <summary>
        /// Override the abstract CreateShell() method to return the Shell (main window).
        /// </summary>
        /// <returns>The <see cref="DevelopmentInProgress.Origin.Shell"/> that is the main window.</returns>
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        /// <summary>
        /// Set the <see cref="DevelopmentInProgress.Origin.Shell"/> as the main window and show it.
        /// </summary>
        protected override void InitializeShell()
        {
            App.Current.MainWindow = (Window)this.Shell;
            App.Current.MainWindow.Show();
        }
    }
}