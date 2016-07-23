﻿using System;
using System.Collections.Generic;
using FaithEngage.Core.Containers;
using FaithEngage.Core.ActionProcessors.Interfaces;
using FaithEngage.Core.ActionProcessors;
using FaithEngage.Core.Factories;

namespace FaithEngage.Core
{
    public class ActionProcessorsBootstrapper : IBootstrapper
    {
        public void Execute (IAppFactory container)
        {
        }

        public void LoadBootstrappers (IBootList bootstrappers)
        {
        }

        public void RegisterDependencies (IRegistrationService rs)
        {
            rs.Register<ICardActionProcessor, CardActionProcessor> (LifeCycle.Singleton);
        }
    }
}

