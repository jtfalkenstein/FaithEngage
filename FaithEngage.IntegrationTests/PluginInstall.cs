﻿using System;
using NUnit.Framework;
using FaithEngage.Core.PluginManagers.Files.Interfaces;
using FaithEngage.Core.PluginManagers.Interfaces;
using FaithEngage.Core.Containers;
using FaithEngage.Core.Bootstrappers;
using FaithEngage.Core.PluginManagers;
using FaithEngage.Core.Config;
using FakeItEasy;
using FaithEngage.Core.RepoInterfaces;
using System.IO.Compression;
using System.IO;
using System.Collections.Generic;
using FaithEngage.Core.PluginManagers.Files;

namespace FaithEngage.IntegrationTests
{
    [TestFixture]
    public class PluginInstall
    {
        private IContainer _container;
        private IBootList _bootlist;
        private IPluginManager _mgr;



        public class config : IConfigManager
        {
            public string PluginsFolderPath { get { return "PLUGINS";} set { return; } }

            public string TempFolderPath { get { return "TEMP";} set { return; } }

            public string GetValue (string key)
            {
                throw new NotImplementedException ();
            }

            public string SetValue (string Key, string value)
            {
                throw new NotImplementedException ();
            }
        }
        public class repo : IPluginRepository
        {
            public void Delete (Guid pluginId)
            {
                throw new NotImplementedException ();
            }

            public List<PluginDTO> GetAll ()
            {
                throw new NotImplementedException ();
            }

            public PluginDTO GetById (Guid pluginId)
            {
                throw new NotImplementedException ();
            }

            public Guid Register (PluginDTO plugin)
            {
                return Guid.Empty;
            }

            public void Update (PluginDTO plugin)
            {
                throw new NotImplementedException ();
            }
        }
        public class fileRepo : IPluginFileInfoRepository
        {
            private List<PluginFileInfoDTO> _dtos = new List<PluginFileInfoDTO> ();
            public void DeleteAllFilesForPlugin (Guid pluginId)
            {
                throw new NotImplementedException ();
            }

            public void DeleteFileRecord (Guid fileId)
            {
                throw new NotImplementedException ();
            }

            public IList<PluginFileInfoDTO> GetAllFilesForPlugin (Guid pluginId)
            {
                return _dtos;
            }

            public PluginFileInfoDTO GetFileInfo (Guid fileId)
            {
                throw new NotImplementedException ();
            }

            public void SaveFileInfo (PluginFileInfoDTO dto)
            {
                _dtos.Add (dto);
            }

            public void UpdateFile (PluginFileInfoDTO dto)
            {
                throw new NotImplementedException ();
            }
        }

        [SetUp]
        public void Init()
        {
            _container = new IocContainer ();
            _bootlist = new BootList (_container);
            _container.Register<IConfigManager, config> ();
            _container.Register<IPluginRepository, repo> ();
            _container.Register<IPluginFileInfoRepository, fileRepo> ();
            _bootlist.Load<PluginBootstrapper> ();
            Console.Write(_bootlist.RegisterAllDependencies (true));

        }

        [Test]
        public void InstallPlugin_StubbedRepoAndConfig()
        {
            _mgr = _container.Resolve<PluginManager> ();
            using(var zipFile = ZipFile.OpenRead(Path.Combine ("TestingFiles", "pluginZip.zip")))
            {
                _mgr.Install (zipFile);
            }
        }

    }
}

