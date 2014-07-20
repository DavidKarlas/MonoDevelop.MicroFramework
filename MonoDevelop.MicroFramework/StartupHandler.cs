using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using Microsoft.Win32;
using System.Reflection;
using System.IO;

namespace MonoDevelop.MicroFramework
{
	class StartupHandler : CommandHandler
	{
		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if(!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			// If the destination directory doesn't exist, create it. 
			if(!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach(FileInfo file in files)
			{
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			// If copying subdirectories, copy them and their contents to new location. 
			if(copySubDirs)
			{
				foreach(DirectoryInfo subdir in dirs)
				{
					string temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}

		protected override void Run()
		{
			if(Platform.IsMac)
			{
				string addInFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:", "");
				var registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\.NETMicroFramework\\v4.3");
				if(registryKey.GetValue("InstallRoot") == null)
				{
					registryKey.SetValue("BuildNumber", "1");
					registryKey.SetValue("RevisionNumber", "0");
					registryKey.SetValue("InstallRoot", Path.Combine(addInFolder, "files", "sdk"));
				}

				if(!Directory.Exists("/Library/Frameworks/Mono.framework/External/xbuild-frameworks/.NETMicroFramework"))
				{
					DirectoryCopy(Path.Combine(addInFolder, "files", "xbuild-frameworks"), "/Library/Frameworks/Mono.framework/External/xbuild-frameworks/", true);
				}

				if(!Directory.Exists("/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild/Microsoft/.NET Micro Framework"))
				{
					DirectoryCopy(Path.Combine(addInFolder, "files", "xbuild"),
						"/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild/", true);
				}
			}
		}
	}
}

