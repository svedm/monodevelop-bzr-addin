using System;
using MonoDevelop.Components.Commands;
using System.Collections.Generic;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using System.IO;

namespace MonoDevelop.VersionControl.Bazaar
{
	internal class BranchCommand : CommandHandler
	{
		protected override void Update(CommandInfo info)
		{
			BazaarVersionControl bvc = null;

			foreach (VersionControlSystem vcs in VersionControlService.GetVersionControlSystems ())
				if (vcs is BazaarVersionControl)
					bvc = (BazaarVersionControl)vcs;

			info.Visible = (null != bvc && bvc.IsInstalled);
		}

		protected override void Run()
		{
			var bsd = new BranchSelectionDialog(new List<string>(), string.Empty, Environment.GetFolderPath(Environment.SpecialFolder.Personal), true, false, false, false);
			try
			{
				if ((int)Gtk.ResponseType.Ok == bsd.Run() && !string.IsNullOrEmpty(bsd.SelectedLocation))
				{
					string branchLocation = bsd.SelectedLocation,
					branchName = GetLastChunk(branchLocation),
					localPath = Path.Combine(bsd.LocalPath, (string.Empty == branchName) ? "branch" : branchName);
					BazaarTask worker = new BazaarTask();
					worker.Description = string.Format("Branching from {0} to {1}", branchLocation, localPath);
					worker.Operation = delegate
					{
						DoBranch(branchLocation, localPath, worker.ProgressMonitor);
					};
					worker.Start();
				}
			}
			finally
			{
				bsd.Destroy();
			}
		}

		delegate bool ProjectCheck(string path);

		/// <summary>
		/// Performs a bzr branch
		/// </summary>
		/// <param name="location">
		/// A <see cref="System.String"/>: The from location
		/// </param>
		/// <param name="localPath">
		/// A <see cref="System.String"/>: The to location
		/// </param>
		/// <param name="monitor">
		/// A <see cref="IProgressMonitor"/>: The progress monitor to be used
		/// </param>
		private static void DoBranch(string location, string localPath, IProgressMonitor monitor)
		{
			BazaarVersionControl bvc = null;

			foreach (VersionControlSystem vcs in VersionControlService.GetVersionControlSystems ())
				if (vcs is BazaarVersionControl)
					bvc = (BazaarVersionControl)vcs;

			if (null == bvc || !bvc.IsInstalled)
				throw new Exception("Bazaar is not installed");

			// Branch
			bvc.Branch(location, localPath, monitor);

			// Search for solution/project file in local branch;
			// open if found
			string[] list = System.IO.Directory.GetFiles(localPath);

			ProjectCheck[] checks =
				{
				delegate (string path)
				{
					return path.EndsWith(".mds");
				},
				delegate (string path)
				{
					return path.EndsWith(".mdp");
				},
				MonoDevelop.Projects.Services.ProjectService.IsWorkspaceItemFile
			};

			foreach (ProjectCheck check in checks)
			{
				foreach (string file in list)
				{
					if (check(file))
					{
						Gtk.Application.Invoke(delegate (object o, EventArgs ea)
							{
								IdeApp.Workspace.OpenWorkspaceItem(file);
							});
						return;
					}// found a project file
				}// on each file
			}// run check
		}

		/// <summary>
		/// Gets the last chunk of a branch location
		/// </summary>
		/// <param name="branchLocation">
		/// A <see cref="System.String"/>: The branch location to chunk
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>: The last nonempty chunk of branchLocation
		/// </returns>
		private static string GetLastChunk(string branchLocation)
		{
			string[] chunks = null,
			separators = { "/", Path.DirectorySeparatorChar.ToString() };
			string chunk = string.Empty;

			foreach (string separator in separators)
			{
				if (branchLocation.Contains(separator))
				{
					chunks = branchLocation.Split('/');
					for (int i = chunks.Length - 1; i >= 0; --i)
					{
						if (string.Empty != (chunk = chunks[i].Trim()))
							return chunk;
					}// accept last non-empty chunk
				}
			}// check each separation scheme

			return string.Empty;
		}
	}
}