﻿using System;
using NUnit.Framework;
using FaithEngage.Core.PluginManagers.DisplayUnitPlugins.Interfaces;
using FakeItEasy;
using System.Collections.Generic;
using FaithEngage.Core.Containers;
using FaithEngage.Core.Factories;

namespace FaithEngage.Core.PluginManagers.DisplayUnitPlugins
{
    [TestFixture]
    public class DisplayUnitPluginBootstrapperTests
    {
        [Test]
        public void Execute_RegistersDependenciesAndExecutesOnPlugins()
        {
            var pinContainer = A.Fake<IDisplayUnitPluginContainer> ();
            var repoMgr = A.Fake<IDisplayUnitPluginRepoManager> ();
            var plugin = A.Fake<DisplayUnitPlugin> ();
			var regService = A.Fake<IRegistrationService>();
            var container = A.Fake<IContainer> ();
			var appFac = A.Fake<IAppFactory>();
            A.CallTo (() => repoMgr.GetAll ()).Returns (new List<DisplayUnitPlugin> () { plugin });
            A.CallTo (() => container.Resolve<IDisplayUnitPluginContainer> ()).Returns (pinContainer);
            A.CallTo(()=> container.Resolve<IDisplayUnitPluginRepoManager>()).Returns(repoMgr);
			A.CallTo(() => container.GetRegistrationService()).Returns(regService);
			A.CallTo(() => container.Resolve<IAppFactory>()).Returns(appFac);

            var pluginBooter = new DisplayUnitPluginBootstrapper ();
            pluginBooter.Execute (container);


			A.CallTo (() => plugin.RegisterDependencies (regService)).MustHaveHappened();
			A.CallTo (() => plugin.Initialize (appFac)).MustHaveHappened();
            A.CallTo (() => pinContainer.Register (plugin)).MustHaveHappened();
        }
    }
}

