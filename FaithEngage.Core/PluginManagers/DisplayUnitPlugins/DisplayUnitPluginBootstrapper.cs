﻿using System;
using System.Collections.Generic;
using FaithEngage.Core.Containers;
using FaithEngage.Core.Factories;
using FaithEngage.Core.PluginManagers.DisplayUnitPlugins.Factories;
using FaithEngage.Core.PluginManagers.DisplayUnitPlugins.Interfaces;
using FaithEngage.Core.RepoManagers;
using FaithEngage.Core.RepoInterfaces;

namespace FaithEngage.Core.PluginManagers.DisplayUnitPlugins
{
	public class DisplayUnitPluginBootstrapper : IBootstrapper
	{
        public void Execute(IContainer container)
		{
			var pluginContainer = container.Resolve<IDisplayUnitPluginContainer>();
			var repoManager = container.Resolve<IDisplayUnitPluginRepoManager>();
			var factory = container.Resolve<IAppFactory>();
			var regService = container.GetRegistrationService();
			foreach (var plugin in repoManager.GetAll()) {
				plugin.RegisterDependencies (regService);
                plugin.Initialize (factory);
                pluginContainer.Register (plugin);
			}
		}

        public void LoadBootstrappers (IList<IBootstrapper> bootstrappers)
        {
        }

        public void RegisterDependencies(IContainer container)
		{
			container.Register<IDisplayUnitPluginContainer, DisplayUnitPluginContainer>(LifeCycle.Singleton);
			container.Register<IConverterFactory<DisplayUnitPlugin,DisplayUnitPluginDTO>, DisplayUnitPluginDtoFactory>(LifeCycle.Transient);
			container.Register<IDisplayUnitPluginFactory, DisplayUnitPluginFactory>(LifeCycle.Transient);
			container.Register<IDisplayUnitPluginRepoManager, DisplayUnitPluginRepoManager>(LifeCycle.Transient);
		}

	}
}

