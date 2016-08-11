﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using FaithEngage.Core.PluginManagers.Files.Interfaces;
using FaithEngage.Core.Config;
using System.Linq;
using FaithEngage.Core.RepoInterfaces;
using FaithEngage.Core.Exceptions;
using System.Security;

namespace FaithEngage.Core.PluginManagers.Files
{
    public class PluginFileManager : IPluginFileManager
    {
        private readonly IPluginFileInfoRepository _repo;
        private readonly IConverterFactory<PluginFileInfo, PluginFileInfoDTO> _dtoFac;
        private readonly IPluginFileInfoFactory _factory;
		private DirectoryInfo _tempFolder;
		private DirectoryInfo _pluginsFolder;
        public PluginFileManager (IPluginFileInfoRepository repo, IConverterFactory<PluginFileInfo, PluginFileInfoDTO> dtoFac, IPluginFileInfoFactory factory) 
        {
            _repo = repo;
            _dtoFac = dtoFac;
            _factory = factory;

			_tempFolder = _factory.TempFolder;
			_pluginsFolder = factory.PluginsFolder;
        }

        public void DeleteAllFilesForPlugin (Guid pluginId)
        {
            var path = _factory.GetBasePluginPath (pluginId);
			if (Directory.Exists(path)) 
				doFileAction(path, p => Directory.Delete(p, true));
            try{
                _repo.DeleteAllFilesForPlugin (pluginId);
            }catch(RepositoryException){
                return;
            }
        }

        public void DeleteFile (Guid fileId)
        {
            var pFileInfo = GetFile (fileId);
			if (pFileInfo.FileInfo.Exists) 
				doFileAction(pFileInfo, p => p.FileInfo.Delete());
            try{
                _repo.DeleteFileRecord (fileId);
			} catch (RepositoryException){}
        }

        public IList<FileInfo> ExtractZipToTempFolder (ZipArchive zipArchive, Guid key)
        {
            
			DirectoryInfo folder = doFileAction(key, p=>  _tempFolder.CreateSubdirectory(p.ToString()));
			doFileAction(folder, p => zipArchive.ExtractToDirectory(p.FullName));
			IList<FileInfo> list = doFileAction(folder, p=> p.EnumerateFiles("*",SearchOption.AllDirectories).ToList());
            return list;
        }

		public void FlushTempFolder(Guid key)
		{
			_tempFolder.EnumerateDirectories(key.ToString())
		   .ToList()
		   .ForEach(p =>
				{
					try
					{
						p.Delete(true);
					}
					catch (Exception ex)
					{
						throwFileException(ex, p.FullName);
					}
				});
        }

        public void FlushTempFolder ()
        {
			var dirs = doFileAction(_tempFolder, p=> p.EnumerateDirectories ());
            foreach(var dir in dirs){
                try {
                    dir.Delete (true);
                } catch (Exception) {
                    continue;
                }
            }
        }

        public PluginFileInfo GetFile (Guid fileId)
        {
            PluginFileInfoDTO dto = null;
            try {
                dto = _repo.GetFileInfo (fileId);
            } catch (RepositoryException ex) {
                throw new RepositoryException ("There was a problem obtaining the file record from the db.", ex);
            }
            if (dto == null) return null;
            var pfile = _factory.Convert (dto);
            return pfile;

        }

        public IDictionary<Guid, PluginFileInfo> GetFilesForPlugin (Guid pluginId)
        {
			IList<PluginFileInfoDTO> dtos;
			try
			{
				dtos = _repo.GetAllFilesForPlugin(pluginId);
			}
			catch (RepositoryException ex)
			{
				throw new RepositoryException("There was a problem obtaining the file records from the db.", ex);
			}
			if (dtos == null) return null;
            var dict = dtos.ToDictionary (p => p.FileId, p => _factory.Convert (p));
            return dict;
        }

        public void RenameFile (Guid fileId, string newRelativePath)
        {
            var file = GetFile (fileId);
			if (file == null) 
				throw new InvalidFileException(fileId.ToString(), $"The file with the id {fileId.ToString()} does not exist in either the db or on the filesystem.");
			var oldFileName = file.FileInfo.FullName;
			var newPath = _factory.GetRenamedPath(file, newRelativePath);
			var dirName = Path.GetDirectoryName(newPath);
			if (!Directory.Exists(dirName)) doFileAction(dirName, p=> Directory.CreateDirectory(p));
			file.FileInfo = doFileAction(newPath, p => file.FileInfo.CopyTo(newPath));
			doFileAction(oldFileName, p => File.Delete(p));
			var dto = _dtoFac.Convert(file);
			try
			{
				_repo.UpdateFile(dto);
			}
			catch (RepositoryException ex)
			{
				throw new RepositoryException("There was a problem updating the db.", ex);
			}

        }

        public void StoreFilesForPlugin (IList<FileInfo> files, Guid pluginId, bool overWrite = false)
        {
			foreach(var file in files)
            {
                if(file.Exists){
                    
					var newPath = Path.Combine (_factory.GetBasePluginPath (pluginId), file.Name);
                    var dirPath = Path.GetDirectoryName (newPath);
					var parentDir = Directory.CreateDirectory (dirPath);
                    var savedFile = file.CopyTo (newPath, overWrite);
                    var pfile = _factory.Create (savedFile, pluginId);
                    var dto = _dtoFac.Convert (pfile);
                    _repo.SaveFileInfo (dto);
                }
            }
        }


		private void throwFileException(Exception ex, object subject)
		{
			try { throw ex; }
			catch (IOException)
			{
				throw new PluginFileException($"IO Exception encountered regarding {subject.ToString()}", ex);
			}
			catch (UnauthorizedAccessException)
			{
				throw new PluginFileException($"Unauthorized Access on {subject.ToString()}", ex);
			}
			catch (SecurityException)
			{
				throw new PluginFileException($"Unauthorized Access on {subject.ToString()}", ex);
			}
		}

		private Tresult doFileAction<Tinput, Tresult>(Tinput source, Func<Tinput, Tresult> func)
		{
			Tresult result = default(Tresult);
			try
			{
				result = func(source);
			}
			catch (Exception ex)
			{
				throwFileException(ex, source);
			}
			return result;
		}

		private void doFileAction<Tinput>(Tinput source, Action<Tinput> func)
		{
			try
			{
				func(source);
			}
			catch (Exception ex)
			{
				throwFileException(ex, source);
			}
		}
    }
}

