using System;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Components.Commands;
using System.Collections.Generic;
using MonoDevelop.Core;
using System.IO;
using Gtk;

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

		/// <summary>
		/// Determines whether a merge can be performed.
		/// </summary>
		[CommandUpdateHandler (BazaarCommands.Merge)]
		protected void CanMerge (CommandInfo item)
		{
			if (1 == GetItems ().Count) {
				VersionControlItem vcitem = GetItems ()[0];
				item.Visible = (vcitem.Repository is BazaarRepository &&
					((BazaarRepository)vcitem.Repository).CanMerge (vcitem.Path));
			} else { item.Visible = false; }
		}// CanMerge

		/// <summary>
		/// Performs a merge.
		/// </summary>
		[CommandHandler (BazaarCommands.Merge)]
		protected void OnMerge()
		{
			VersionControlItem vcitem = GetItems ()[0];
			BazaarRepository repo = ((BazaarRepository)vcitem.Repository);
			Dictionary<string, BranchType> branches = repo.GetKnownBranches (vcitem.Path);
			string   defaultBranch = string.Empty,
			localPath = vcitem.IsDirectory? (string)vcitem.Path.FullPath: Path.GetDirectoryName (vcitem.Path.FullPath);

			if (repo.IsModified (BazaarRepository.GetLocalBasePath (vcitem.Path.FullPath))) {
				var md = new MessageDialog (null, DialogFlags.Modal, 
					MessageType.Question, ButtonsType.YesNo, 
					GettextCatalog.GetString ("You have uncommitted local changes. Merge anyway?"));
				try {
					if ((int)ResponseType.Yes != md.Run ()) {
						return;
					}
				} finally {
					md.Destroy ();
				}
			}// warn about uncommitted changes

			foreach (KeyValuePair<string, BranchType> branch in branches) {
				if (BranchType.Parent == branch.Value) {
					defaultBranch = branch.Key;
					break;
				}
			}// check for parent branch

			var bsd = new BranchSelectionDialog (branches.Keys, defaultBranch, localPath, false, true, false, false);
			try {
				if ((int)Gtk.ResponseType.Ok == bsd.Run () && !string.IsNullOrEmpty (bsd.SelectedLocation)) {
					BazaarTask worker = new BazaarTask ();
					worker.Description = string.Format ("Merging from {0}", bsd.SelectedLocation);
					worker.Operation = delegate{ repo.Merge (bsd.SelectedLocation, vcitem.Path, bsd.SaveDefault, true, worker.ProgressMonitor); };
					worker.Start ();
				}
			} finally {
				bsd.Destroy ();
			}
		}// OnMerge

		/// <summary>
		/// Determines whether a new repository can be created for the selected item
		/// </summary>
		[CommandUpdateHandler (BazaarCommands.Init)]
		protected void CanInit (CommandInfo item)
		{
			if (1 == GetItems ().Count) {
				VersionControlItem vcitem = GetItems ()[0];
				if (vcitem.WorkspaceObject is Solution && null == vcitem.Repository) {
					item.Visible = true;
					return;
				}
			} 
			item.Visible = false;
		}// CanInit

		/// <summary>
		/// Initializes a new repository and adds the current solution.
		/// </summary>
		[CommandHandler (BazaarCommands.Init)]
		protected void OnInit()
		{
			BazaarVersionControl bvc = null;
			BazaarRepository repo = null;
			VersionControlItem vcitem = GetItems ()[0];
			string path = vcitem.Path;
			List<FilePath> addFiles = null;
			Solution solution = (Solution)vcitem.WorkspaceObject;

			foreach (VersionControlSystem vcs in VersionControlService.GetVersionControlSystems ())
				if (vcs is BazaarVersionControl)
					bvc = (BazaarVersionControl)vcs;

			if (null == bvc || !bvc.IsInstalled)
				throw new Exception ("Can't use bazaar");

			bvc.Init (path);

			repo = new BazaarRepository (bvc, string.Format("file://{0}", path));
			addFiles = GetAllFiles (solution);

			repo.Add (addFiles.ToArray (), false, null);
			solution.NeedsReload = true;
		}// OnInit

		/// <summary>
		/// Returns a list of all files relevant to the solution
		/// </summary>
		private static List<FilePath> GetAllFiles (Solution s) {
			List<FilePath> files = new List<FilePath> ();

			files.Add (s.FileName);
			foreach (Solution child in s.GetAllSolutions ()) {
				if (s != child)
					files.AddRange (GetAllFiles (child));
			}// recurse!
			foreach (Project project in s.GetAllProjects ()) {
				files.Add (project.FileName);
				foreach (ProjectFile pfile in project.Files) {
					files.Add (pfile.FilePath);
				}// add project file
			}// add project files

			return files;
		}// GetAllFiles

		/// <summary>
		/// Ignores a file
		/// </summary>
		[CommandHandler (BazaarCommands.Ignore)]
		protected void OnIgnore()
		{
			VersionControlItem vcitem = GetItems ()[0];
			((BazaarRepository)vcitem.Repository).Ignore (new [] { vcitem.Path });
		}// OnIgnore

		[CommandUpdateHandler (BazaarCommands.Bind)]
		protected void CanBind (CommandInfo item)
		{
			if (1 == GetItems ().Count) {
				VersionControlItem vcitem = GetItems ()[0];
				if (vcitem.Repository is BazaarRepository) {
					item.Visible = ((BazaarRepository)vcitem.Repository).CanBind (vcitem.Path);
					return;
				}
			} 
			item.Visible = false;
		}// CanBind

		/// <summary>
		/// Binds a file
		/// </summary>
		[CommandHandler (BazaarCommands.Bind)]
		protected void OnBind()
		{
			VersionControlItem vcitem = GetItems ()[0];
			BazaarRepository repo = (BazaarRepository)vcitem.Repository;
			string boundBranch = repo.GetBoundBranch (vcitem.Path);

			var bsd = new BranchSelectionDialog (new string[]{boundBranch}, boundBranch, vcitem.Path.FullPath, false, false, false, false);
			try {
				if ((int)Gtk.ResponseType.Ok == bsd.Run () && !string.IsNullOrEmpty (bsd.SelectedLocation)) {
					BazaarTask worker = new BazaarTask ();
					worker.Description = string.Format ("Binding to {0}", bsd.SelectedLocation);
					worker.Operation = delegate{ repo.Bind (bsd.SelectedLocation, vcitem.Path, worker.ProgressMonitor); };
					worker.Start ();
				}
			} finally {
				bsd.Destroy ();
			}
		}// OnBind

		[CommandUpdateHandler (BazaarCommands.Unbind)]
		protected void CanUnbind (CommandInfo item)
		{
			if (1 == GetItems ().Count) {
				VersionControlItem vcitem = GetItems ()[0];
				if (vcitem.Repository is BazaarRepository) {
					item.Visible = ((BazaarRepository)vcitem.Repository).CanUnbind (vcitem.Path);
					return;
				}
			} 
			item.Visible = false;
		}// CanUnbind

		/// <summary>
		/// Unbinds a file
		/// </summary>
		[CommandHandler (BazaarCommands.Unbind)]
		protected void OnUnbind()
		{
			VersionControlItem vcitem = GetItems ()[0];
			BazaarRepository repo = (BazaarRepository)vcitem.Repository;

			BazaarTask worker = new BazaarTask ();
			worker.Description = string.Format ("Unbinding {0}", vcitem.Path);
			worker.Operation = delegate{ repo.Unbind (vcitem.Path, worker.ProgressMonitor); };
			worker.Start ();
		}// OnUnbind

	}
}

