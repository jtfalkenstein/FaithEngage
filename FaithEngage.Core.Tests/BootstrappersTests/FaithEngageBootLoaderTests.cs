﻿using System;
using NUnit.Framework;
using FaithEngage.Core.Containers;
using FakeItEasy;
using FaithEngage.Core.Factories;
using System.Collections.Generic;

namespace FaithEngage.Core.Bootstrappers
{
    [TestFixture]
    public class FaithEngageBootLoaderTests
    {
        private IContainer _container = A.Fake<IContainer> ();

        [Test]
        public void Execute_ActivatesAppFactory()
        {
            var feBooter = new FaithEngageBootLoader ();
            feBooter.Execute (_container);

            A.CallTo (() => _container.Resolve<IAppFactory> ()).MustHaveHappened();
        }

        [Test]
        public void RegisterDependencies_RegistersDependencies ()
        {
            var feBooter = new FaithEngageBootLoader ();
            feBooter.RegisterDependencies (_container);

            A.CallTo (() => _container.Register<IAppFactory, AppFactory> ()).MustHaveHappened();
        }

        [Test]
        public void LoadBootstrappers_AddsOtherBootstrappersToList()
        {
            var list = new List<IBootstrapper> ();
            var feBooter = new FaithEngageBootLoader ();

            feBooter.LoadBootstrappers (list);

            Assert.That (list.Count, Is.GreaterThanOrEqualTo (5));
        }


    }
}

