using Microsoft.SPOT.Debugger;
using MonoDevelop.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoDevelop.MicroFramework
{
	class MicroFrameworkExecutionTarget:ExecutionTarget
	{
		public PortDefinition PortDefinition { get; private set; }

		public MicroFrameworkExecutionTarget(PortDefinition portDefinition)
		{
			this.PortDefinition = portDefinition;
		}

		public override string Name
		{
			get { return PortDefinition.DisplayName; }
		}

		public override string Id
		{
			get { return PortDefinition.UniqueId.ToString(); }
		}
	}
}
