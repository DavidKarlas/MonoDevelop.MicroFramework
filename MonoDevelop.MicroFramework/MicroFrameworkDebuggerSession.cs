using Microsoft.SPOT.Debugger;
using Microsoft.SPOT.Debugger.WireProtocol;
using Mono.Debugging.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonoDevelop.MicroFramework
{
	class MicroFrameworkDebuggerSession : DebuggerSession, IDisposable
	{
		public MicroFrameworkDebuggerSession()
		{
		}

		private Engine engine;

		protected override void OnRun(DebuggerStartInfo startInfo)
		{
			var mfStartInfo = startInfo as MicroFrameworkDebuggerStartInfo;
			if(mfStartInfo == null)//This should never happen...
				throw new InvalidOperationException();
			var command = mfStartInfo.MFCommand;
			using(var deployEngine = new Engine((command.Target as MicroFrameworkExecutionTarget).PortDefinition))
			{
				deployEngine.Start();
				var listOfAseemblies = new ArrayList();

				//TODO: Check if this is robust enough will "be" and "le" really always be in output folder?
				string dir = command.OutputDirectory;
				if(deployEngine.IsTargetBigEndian)
					dir = Path.Combine(dir, "be");
				else
					dir = Path.Combine(dir, "le");

				string[] files = Directory.GetFiles(dir, "*.pe");
				foreach(var v in files)
				{
					using(var fs = new FileStream(v, FileMode.Open))
					{
						byte[] data = new byte[fs.Length];
						fs.Read(data, 0, data.Length);
						listOfAseemblies.Add(data);
					}
				}

				deployEngine.Deployment_Execute(listOfAseemblies, false, (str) =>
				{
					OnDebuggerOutput(false, "Deploy: " + str + Environment.NewLine);
				});
				deployEngine.RebootDevice(Engine.RebootOption.RebootClrWaitForDebugger);
			}

			int retries = 0;
			while(true)//TODO: More proper solution...
			{
				engine = new Engine((command.Target as MicroFrameworkExecutionTarget).PortDefinition);
				engine.OnMessage += engine_OnMessage;
				engine.Start();
				if(engine.TryToConnect(2, 3000))
				{
					break;
				}
				else if(retries < 5)
				{
					engine.RebootDevice(Engine.RebootOption.RebootClrWaitForDebugger);
					engine.Dispose();
					retries++;
				}
				else
				{
					engine.Dispose();
					throw new Exception("Failed to connect to device.");
				}
			}
			OnStarted();
		}

		void engine_OnMessage(IncomingMessage msg, string text)
		{
			OnDebuggerOutput(false, text);
		}

		protected override void OnAttachToProcess(long processId)
		{
			throw new NotImplementedException();
		}

		protected override void OnDetach()
		{
			throw new NotImplementedException();
		}

		protected override void OnSetActiveThread(long processId, long threadId)
		{
			throw new NotImplementedException();
		}

		protected override void OnStop()
		{
			engine.PauseExecution();//TODO: This returns boolean...

			//var thread = process.GetFirstThread();

//            OnTargetEvent(new TargetEventArgs(TargetEventType.TargetStopped)
//            {
//            });

		}

		protected override void OnExit()
		{
			engine.Stop();
			engine.Dispose();
		}

		protected override void OnStepLine()
		{
			throw new NotImplementedException();
		}

		protected override void OnNextLine()
		{
			throw new NotImplementedException();
		}

		protected override void OnStepInstruction()
		{
			throw new NotImplementedException();
		}

		protected override void OnNextInstruction()
		{
			throw new NotImplementedException();
		}

		protected override void OnFinish()
		{
			throw new NotImplementedException();
		}

		protected override void OnContinue()
		{
			engine.ResumeExecution();//TODO: This returns boolean...
		}

		protected override BreakEventInfo OnInsertBreakEvent(BreakEvent breakEvent)
		{
			//Breakpoint bp = breakEvent as Breakpoint;
			//if (bp == null)
			//    throw new NotSupportedException();

			//engine.SetBreakpoints(new Commands.Debugging_Execution_BreakpointDef[1]{
			//    new Commands.Debugging_Execution_BreakpointDef()
			//    {
			//         m_IP=(uint)bp.Column
			//    }
			//});

			return new BreakEventInfo();
		}

		protected override void OnRemoveBreakEvent(BreakEventInfo eventInfo)
		{
			throw new NotImplementedException();
		}

		protected override void OnUpdateBreakEvent(BreakEventInfo eventInfo)
		{
			throw new NotImplementedException();
		}

		protected override void OnEnableBreakEvent(BreakEventInfo eventInfo, bool enable)
		{
			throw new NotImplementedException();
		}

		protected override ThreadInfo[] OnGetThreads(long processId)
		{
			//var mfThreads = engine.GetThreads();
			//var threads = new ThreadInfo[mfThreads.Count];

			//for (int i = 0; i < threads.Length; i++)
			//{
			//    //threads[i]=new ThreadInfo(fakeProcess.Id,)
			//}
			throw new NotImplementedException();
		}

		protected override ProcessInfo[] OnGetProcesses()
		{
			throw new NotImplementedException();
		}

		protected override Backtrace OnGetThreadBacktrace(long processId, long threadId)
		{
			throw new NotImplementedException();
		}
	}
}
