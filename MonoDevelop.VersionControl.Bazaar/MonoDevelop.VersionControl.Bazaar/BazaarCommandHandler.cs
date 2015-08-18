using System;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Components.Commands;
using System.Collections.Generic;
using MonoDevelop.Core;
using System.IO;

namespace MonoDevelop.VersionControl.Bazaar
{
	public class BazaarCommandHandler : VersionControlCommandHandler
	{
		/// <summary>
		/// Determines whether the selected items can be resolved
		/// </summary>
		[CommandUpdateHandler (BazaarCommands.Resolve)]
		protected void CanResolve (CommandInfo item)
		{
			bool visible = true;

			foreach (VersionControlItem vcitem in GetItems ()) {
				if(!(visible = (vcitem.Repository is BazaarRepository &&
					((BazaarRepository)vcitem.Repository).CanResolve (vcitem.Path))))
					break;
			}

			item.Visible = visible;
		}// CanResolve

		[CommandHandler (BazaarCommands.Resolve)]
		protected void OnResolve()
		{
			List<FilePath> files = null;
			BazaarRepository repo = null;

			foreach (VersionControlItemList repolist in GetItems ().SplitByRepository ()) {
				repo = (BazaarRepository)repolist[0].Repository;
				files = new List<FilePath> (repolist.Count);
				foreach (VersionControlItem item in repolist) {
					files.Add (new FilePath (item.Path));
				}

				BazaarTask worker = new BazaarTask ();
				worker.Description = string.Format ("Resolving {0}", files[0]);
				worker.Operation = delegate{ repo.Resolve (files.ToArray (), true, worker.ProgressMonitor); };
				worker.Start ();
			}
		}// OnResolve

		/// <summary>
		/// Determines whether a pull can be performed.
		/// </summary>
		[CommandUpdateHandler (BazaarCommands.Pull)]
		protected void CanPull (CommandInfo item)
		{
			if (1 == GetItems ().Count) {
				VersionControlItem vcitem = GetItems ()[0];
				item.Visible = (vcitem.Repository is BazaarRepository &&
					((BazaarRepository)vcitem.Repository).CanPull (vcitem.Path));
			} else { item.Visible = false; }
		}// CanPull

		/// <summary>
		/// Performs a pull.
		/// </summary>
		[CommandHandler (BazaarCommands.Pull)]
		protected void OnPull()
		{
			VersionControlItem vcitem = GetItems ()[0];
			BazaarRepository repo = ((BazaarRepository)vcitem.Repository);
			Dictionary<string, BranchType> branches = repo.GetKnownBranches (vcitem.Path);
			string   defaultBranch = string.Empty,
			localPath = vcitem.IsDirectory? (string)vcitem.Path.FullPath: Path.GetDirectoryName (vcitem.Path.FullPath);

			foreach (KeyValuePair<string, BranchType> branch in branches) {
				if (BranchType.Parent == branch.Value) {
					defaultBranch = branch.Key;
					break;
				}
			}// check for parent branch

			var bsd = new BranchSelectionDialog (branches.Keys, defaultBranch, localPath, false, true, true, false);
			try {
				if ((int)Gtk.ResponseType.Ok == bsd.Run () && !string.IsNullOrEmpty (bsd.SelectedLocation)) {
					BazaarTask worker = new BazaarTask ();
					worker.Description = string.Format ("Pulling from {0}", bsd.SelectedLocation);
					worker.Operation = delegate{ repo.Pull (bsd.SelectedLocation, vcitem.Path, bsd.SaveDefault, bsd.Overwrite, worker.ProgressMonitor); };
					worker.Start ();
				}
			} finally {
				bsd.Destroy ();
			}
		}// OnPull
	}

}

