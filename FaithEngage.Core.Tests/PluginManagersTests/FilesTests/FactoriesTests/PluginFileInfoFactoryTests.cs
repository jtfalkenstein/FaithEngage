using System;
using NUnit.Framework;
using FaithEngage.Core.Config;
using FakeItEasy;
using System.IO;
using FaithEngage.Core.Tests;
using FaithEngage.Core.Exceptions;

namespace FaithEngage.Core.PluginManagers.Files.Factories
{
    public class PluginFileInfoFactoryTests
    {
        private IConfigManager _config;

        [SetUp]
        public void Init()
        {
            _config = A.Fake<IConfigManager> ();
            A.CallTo (() => _config.PluginsFolderPath).Returns ("folder\\plugins");
            A.CallTo (() => _config.TempFolderPath).Returns ("folder/temp");
        }

        [Test]
        public void Ctor_PopulatesAndCreatesFolders(){
            var expectedP = Path.Combine ("folder", "plugins");
            var expectedT = Path.Combine ("folder", "temp");

            if (Directory.Exists (expectedP)) Directory.Delete (expectedP, true);
            if (Directory.Exists (expectedT)) Directory.Delete (expectedT, true);

            Assert.That (Directory.Exists (expectedP), Is.Not.True);
            Assert.That (Directory.Exists (expectedT), Is.Not.True);

            var fac = new PluginFileInfoFactory (_config);

            Assert.That (fac.PluginsFolder.FullName.Contains (expectedP));
            Assert.That (fac.TempFolder.FullName.Contains (expectedT));
            fac.PluginsFolder.Refresh ();
            fac.TempFolder.Refresh ();
            Assert.That (fac.PluginsFolder.Exists);
            Assert.That (fac.TempFolder.Exists);

            fac.PluginsFolder.Delete (true);
            fac.TempFolder.Delete (true);
        }

        [Test]
        public void Convert_ExistantFileValidDTO_CreatesPluginFileInfo()
        {
            var plugId = Guid.NewGuid ();

            var fac = new PluginFileInfoFactory (_config);

            fac.PluginsFolder.CreateSubdirectory(plugId.ToString());

            var dto = new PluginFileInfoDTO () {
                Name = "TESTFILE.txt", FileId = Guid.NewGuid (), PluginId = plugId, RelativePath = "otherFolder\\TESTFILE.txt"
            };

            var newDir = Path.Combine (fac.PluginsFolder.FullName, plugId.ToString(), "otherFolder");
            Directory.CreateDirectory (newDir);

            var newPath = Path.Combine (newDir,"TESTFILE.txt");

            File.WriteAllText (newPath, "This is my test.");

            Assert.That (File.Exists(newPath));

            var pfileInfo = fac.Convert (dto);

            Assert.That (pfileInfo, Is.Not.Null);
            Assert.That (pfileInfo.FileId, Is.EqualTo(dto.FileId));
            Assert.That (pfileInfo.FileInfo.Exists);
            Assert.That (pfileInfo.PluginId, Is.EqualTo (plugId));

            Directory.Delete (newDir, true);
        }

		[Test]
		public void Convert_NonExistantFile_ReturnsNull()
		{
			var dto = new PluginFileInfoDTO()
			{
				Name = "TESTFILE.txt",
				FileId = Guid.NewGuid(),
				PluginId = Guid.NewGuid(),
				RelativePath = "otherFolder\\TESTFILE.txt"
			};

			var fac = new PluginFileInfoFactory(_config);

			var newDir = Path.Combine(fac.PluginsFolder.FullName, dto.PluginId.ToString(), "otherFolder");

			Directory.CreateDirectory(newDir);
			var newPath = Path.Combine(newDir, "TESTFILE.txt");

			Assert.That(!File.Exists(newPath));

			var pfile = fac.Convert(dto);

			Assert.That(pfile, Is.Null);

		}

        [Test]
        public void Convert_NotFullDto_Fails()
        {
            var fac = new PluginFileInfoFactory (_config);
            var dto = new PluginFileInfoDTO();
            var e = TestHelpers.TryGetException(()=> fac.Convert (dto));
            Assert.That (e, Is.Not.Null);
            Assert.That (e, Is.InstanceOf<FactoryException> ());
        }

        [Test]
        public void Create_ExistantFile_CreatesPInfoFile()
        {
            var plugId = Guid.NewGuid ();

            var fac = new PluginFileInfoFactory (_config);

            fac.PluginsFolder.CreateSubdirectory (plugId.ToString ());

            var newDir = Path.Combine (fac.PluginsFolder.FullName, plugId.ToString (), "otherFolder");
            Directory.CreateDirectory (newDir);

            var newPath = Path.Combine (newDir, "TESTFILE.txt");

            File.WriteAllText (newPath, "This is my test.");

            Assert.That (File.Exists (newPath));

            var fileInfo = new FileInfo (newPath);

            var pfile = fac.Create (fileInfo, plugId);

            Assert.That (pfile, Is.Not.Null);
            Assert.That (pfile.FileInfo == fileInfo);
            Assert.That (pfile.PluginId == plugId);

            Directory.Delete (newDir, true);
        }

        [Test]
        public void Create_NonExistantFile_createsPInfoFile()
        {
            var plugId = Guid.NewGuid ();

            var fac = new PluginFileInfoFactory (_config);
            var newDir = Path.Combine (fac.PluginsFolder.FullName, plugId.ToString (), "otherFolder");
            var newPath = Path.Combine (newDir, "TESTFILE.txt");

            Assert.That (!File.Exists (newPath));

            var fileInfo = new FileInfo (newPath);

            var pfile = fac.Create (fileInfo, plugId);

            Assert.That (pfile, Is.Not.Null);
            Assert.That (pfile.FileInfo == fileInfo);
            Assert.That (pfile.PluginId == plugId);

        }

        [Test]
        public void GetBasePluginPath_ObtainsValidDirectory()
        {
            var fac = new PluginFileInfoFactory (_config);

            var plugId = Guid.NewGuid ();

            var expectedString = $"{fac.PluginsFolder.FullName}{Path.DirectorySeparatorChar}{plugId}";

            var returnString = fac.GetBasePluginPath (plugId);
            Assert.That (expectedString == returnString);
        }

        [Test]
        public void GetRenamedPath_PluginFile_ObtainsValidPath()
        {
            var plugId = Guid.NewGuid ();
            var fac = new PluginFileInfoFactory (_config);
            fac.PluginsFolder.CreateSubdirectory (plugId.ToString ());
            var newDir = Path.Combine (fac.PluginsFolder.FullName, plugId.ToString (), "otherFolder");
            Directory.CreateDirectory (newDir);
            var newPath = Path.Combine (newDir, "TESTFILE.txt");
            var fileInfo = new FileInfo (newPath);
            var pfile = fac.Create (fileInfo, plugId);

            var ds = Path.DirectorySeparatorChar;
            var basePath = fac.GetBasePluginPath (plugId);

            var expectedString = $"{basePath}{ds}testing{ds}TestFile.text";
            var returnPath = fac.GetRenamedPath (pfile, "testing\\TestFile.text");

            Assert.That (expectedString == returnPath);
        }

    }
}

