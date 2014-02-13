using System;
using MonoDevelop.Projects;
using System.Xml;

namespace MonoDevelop.MicroFramework
{
	public class MicroFrameworkProjectBinding : DotNetProjectBinding
	{
		public override string Name
		{
			get { return ".NETMicroFramework"; }
		}

		protected override DotNetProject CreateProject(string languageName, ProjectCreateInformation info, XmlElement projectOptions)
		{
			return new MicroFrameworkProject(languageName, info, projectOptions);
		}
	}
}

