using MonoDevelop.Core;
using MonoDevelop.Core.Assemblies;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MonoDevelop.MicroFramework
{
	public class MicroFrameworkBackend : MsNetFrameworkBackend
	{
		string frameworkVersion;
		FilePath frameworkLocation;

		public override bool SupportsRuntime(TargetRuntime runtime)
		{
			return true;//TODO: Do some checking?
		}

		protected override void Initialize(TargetRuntime runtime, TargetFramework framework)
		{
			if(framework.Id.Identifier != ".NETMicroFramework")
				throw new InvalidOperationException(string.Format("Only .NETMicroFramework is supported but got " + framework.Id));

			base.Initialize(runtime, framework);
			frameworkVersion = framework.Id.Version;

			this.frameworkLocation = @"C:\Program Files (x86)\Microsoft .NET Micro Framework";
			if(!Directory.Exists(this.frameworkLocation))
				throw new InvalidOperationException(".NET Micro Framework SDK not found at:" + this.frameworkLocation);
		}

		public override bool IsInstalled
		{
			get { return true; }
		}

		public override IEnumerable<string> GetToolsPaths()
		{
			yield return frameworkLocation;
			yield return frameworkLocation.Combine(frameworkVersion);
			foreach(var f in base.GetToolsPaths())
				yield return f;
		}

		public override IEnumerable<string> GetFrameworkFolders()
		{
			//TODO: How does this work... What happens if target device is BigEndian... :S
			if(double.Parse(frameworkVersion, CultureInfo.InvariantCulture) > 4)//.Net MF 4.1 and forward has le and be dirs
			{
				yield return frameworkLocation.Combine("v" + frameworkVersion + "\\Assemblies\\le");
				yield return frameworkLocation.Combine("v" + frameworkVersion + "\\Assemblies\\be");
			}
			else
				yield return frameworkLocation.Combine("v" + frameworkVersion + "\\Assemblies");

		}
	}
}
