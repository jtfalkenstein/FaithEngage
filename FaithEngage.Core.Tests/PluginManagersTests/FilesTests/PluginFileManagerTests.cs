﻿using System;
using NUnit.Framework;
using FaithEngage.Core.RepoInterfaces;
using FaithEngage.Core.PluginManagers.Files;
using FaithEngage.Core.PluginManagers.Files.Interfaces;
using FakeItEasy;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace FaithEngage.Core.Tests
{
    [TestFixture]
    public class PluginFileManagerTests
    {
        private IPluginFileInfoRepository _repo;
        private IConverterFactory<PluginFileInfo, PluginFileInfoDTO> _dtoFac;
        private IPluginFileInfoFactory _factory;
        private DirectoryInfo _tempFolder;
        private DirectoryInfo _pluginFolder;
        private PluginFileManager _mgr;
        [SetUp]
        public void init ()
        {
            _repo = A.Fake<IPluginFileInfoRepository> ();
            _dtoFac = A.Fake<IConverterFactory<PluginFileInfo, PluginFileInfoDTO>> ();
            _factory = A.Fake<IPluginFileInfoFactory> ();

            _tempFolder = new DirectoryInfo ("TEMP");
            if (!_tempFolder.Exists) _tempFolder.Create ();
            _pluginFolder = new DirectoryInfo ("PLUGINS");
            if (!_pluginFolder.Exists) _pluginFolder.Create ();
            A.CallTo (() => _factory.TempFolder).Returns (_tempFolder);
            A.CallTo (() => _factory.PluginsFolder).Returns (_pluginFolder);
            A.CallTo (() => _factory.GetBasePluginPath (A<Guid>.Ignored))
             .ReturnsLazily (
                 (Guid p) => Path.Combine (_pluginFolder.FullName, p.ToString ())
               );
            _mgr = new PluginFileManager (_repo, _dtoFac, _factory);
        }

        [Test]
        public void DeleteAllFilesForPlugin_ValidPluginId ()
        {
            var id = Guid.NewGuid ();
            var subDir = _pluginFolder.CreateSubdirectory (id.ToString ());
            Assert.That (subDir.Exists);
            _mgr.DeleteAllFilesForPlugin (id);
            Assert.That (!Directory.Exists (subDir.FullName));
            A.CallTo (() => _repo.DeleteAllFilesForPlugin (id)).MustHaveHappened ();
        }

        [Test]
        public void DeleteAllFilesForPlugin_InvalidPluginId_FailsSilently ()
        {
            var id = Guid.NewGuid ();
            _mgr.DeleteAllFilesForPlugin (id);
            A.CallTo (() => _repo.DeleteAllFilesForPlugin (id)).MustHaveHappened ();
        }

        [Test]
        public void DeleteAllFilesForPlugin_RepoThrowsException_FailsSilently()
        {
            A.CallTo (() => _repo.DeleteAllFilesForPlugin (A<Guid>.Ignored)).Throws<Exception>();
            var id = Guid.NewGuid ();
            var subDir = _pluginFolder.CreateSubdirectory (id.ToString ());
            Assert.That (subDir.Exists);
            _mgr.DeleteAllFilesForPlugin (id);
        }

        [Test]
        public void DeleteFile_ValidFileId_ExistingFile()
        {
            var id = Guid.NewGuid ();
            var plug = Guid.NewGuid ();
            var plugFolder = _pluginFolder.CreateSubdirectory (plug.ToString ());
            var path = Path.Combine (_pluginFolder.FullName,"TestFile.txt");
            File.WriteAllText (path, "Testing This file.");
            var dto = new PluginFileInfoDTO () {
                FileId = id,
                Name = "TestFile.txt",
                PluginId = id,
                RelativePath = "TestFile.txt"
            };
            var file = new FileInfo (path);
            var pfile = new PluginFileInfo (id, file);

            A.CallTo (() => _repo.GetFileInfo (id)).Returns (dto);
            A.CallTo(()=> _factory.Convert(dto)).Returns(pfile);

            _mgr.DeleteFile (id);

            Assert.That (!File.Exists (path));

            A.CallTo (() => _repo.DeleteFileRecord (id)).MustHaveHappened ();
            plugFolder.Delete (true);
        }

        [Test]
        public void DeleteFile_NonExistingFile_FailsSilently()
        {
            var id = Guid.NewGuid ();
            var plug = Guid.NewGuid ();
            var plugFolder = _pluginFolder.CreateSubdirectory (plug.ToString ());
            var path = Path.Combine (_pluginFolder.FullName, "TestFile.txt");
            var dto = new PluginFileInfoDTO () {
                FileId = id,
                Name = "TestFile.txt",
                PluginId = id,
                RelativePath = "TestFile.txt"
            };
            var file = new FileInfo (path);
            var pfile = new PluginFileInfo (id, file);

            A.CallTo (() => _repo.GetFileInfo (id)).Returns (dto);
            A.CallTo (() => _factory.Convert (dto)).Returns (pfile);

            _mgr.DeleteFile (id);

            Assert.That (!File.Exists (path));

            A.CallTo (() => _repo.DeleteFileRecord (id)).MustHaveHappened ();
            plugFolder.Delete (true);
        }

        [Test]
        public void DeleteFile_RepoThrowsException_FailsSilently()
        {
            var id = Guid.NewGuid ();
            var plug = Guid.NewGuid ();
            var plugFolder = _pluginFolder.CreateSubdirectory (plug.ToString ());
            var path = Path.Combine (_pluginFolder.FullName, "TestFile.txt");
            File.WriteAllText (path, "Testing This file.");
            var dto = new PluginFileInfoDTO () {
                FileId = id,
                Name = "TestFile.txt",
                PluginId = id,
                RelativePath = "TestFile.txt"
            };
            var file = new FileInfo (path);
            var pfile = new PluginFileInfo (id, file);

            A.CallTo (() => _repo.GetFileInfo (id)).Returns (dto);
            A.CallTo (() => _factory.Convert (dto)).Returns (pfile);
            A.CallTo (() => _repo.DeleteFileRecord (id)).Throws<Exception>();
            _mgr.DeleteFile (id);

            Assert.That (!File.Exists (path));

            A.CallTo (() => _repo.DeleteFileRecord (id)).MustHaveHappened ();
            plugFolder.Delete (true);
        }

        [Test]
        public void ExtractZipToTempFolder_ReturnsListOfFileInfo()
        {
            using(var zipFile = ZipFile.OpenRead (Path.Combine ("TestingFiles", "pluginZip.zip")))
            {
                var id = Guid.NewGuid ();
                var list  = _mgr.ExtractZipToTempFolder (zipFile, id);
                Assert.That (list.Count, Is.EqualTo (4));
                Assert.That (list.All (p => p.Exists));
            }
            _tempFolder.EnumerateDirectories ().ToList ().ForEach (p => p.Delete (true));
        }


    }
}

