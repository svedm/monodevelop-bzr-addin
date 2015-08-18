using System;
using System.Collections.Generic;
using System.IO;
using MonoDevelop.Core;

namespace MonoDevelop.VersionControl.Bazaar
{
	public abstract class BazaarClient : IBazaarClient
	{

		/// <summary>
		/// Stores descriptions of list kinds
		/// </summary>
		protected static Dictionary<ListKind, string> listKinds = new Dictionary<ListKind, string> {
			{ ListKind.All, string.Empty },
			{ ListKind.Directory, "directory" },
			{ ListKind.File, "file" },
			{ ListKind.Symlink, "symlink" } 
		};// listKinds

		/// <summary>
		/// Indexes branch types
		/// </summary>
		protected static Dictionary<string, BranchType> branchTypes = new Dictionary<string, BranchType> {
			{ "parent", BranchType.Parent },
			{ "push", BranchType.Push },
			{ "submit", BranchType.Submit },
			{ "public", BranchType.Public }
		};// branchTypes

		protected static Dictionary<string,ItemStatus> longStatuses = new Dictionary<string, ItemStatus> {
			{ "unknown", ItemStatus.Unversioned },
			{ "unversioned", ItemStatus.Unversioned },
			{ "modified", ItemStatus.Modified },
			{ "added", ItemStatus.Added },
			{ "removed", ItemStatus.Deleted }
		};// longStatuses

		#region IBazaarClient implementation 

		public virtual bool CheckInstalled ()
		{
			try { 
				string v = Version; 
				if (!string.IsNullOrEmpty (v)) {
					string[] tokens = v.Split ('.');
					int major = int.Parse (tokens[0]),
					minor = int.Parse (tokens[1]);
					return (3 <= major || (2 == major && 1 <= minor));
				}
			}
			catch { }

			return false;
		}// CheckInstalled

		public virtual bool IsVersioned (string path)
		{
			try {
				IList<LocalStatus> statuses = Status (path, null);
				// System.Console.WriteLine ("IsVersioned: Got back {0} statuses for {1}", statuses.Count, path);
				// if (0 < statuses.Count){ System.Console.WriteLine ("{0} {1}", Path.GetFullPath (path), statuses[0].Filename); }
				if (1 != statuses.Count || 
					!Path.GetFullPath (path).EndsWith (statuses[0].Filename)) {
					return true;
				}// versioned directory

				// System.Console.WriteLine ("Status: {0}", statuses[0].Status);
				return statuses[0].Status != ItemStatus.Unversioned;
			} catch (BazaarClientException) {}

			return false;
		}// IsVersioned

		public abstract string Version{ get; }
		public abstract IList<string> List (string path, bool recurse, ListKind kind);
		public abstract IList<LocalStatus> Status (string path, BazaarRevision revision);
		public abstract string GetPathUrl (string path);
		public abstract void Update (string localPath, bool recurse, IProgressMonitor monitor);
		public abstract void Revert (string localPath, bool recurse, IProgressMonitor monitor, BazaarRevision toRevision);
		public abstract void Add (string localPath, bool recurse, IProgressMonitor monitor);
		public abstract void Checkout (string url, string targetLocalPath, BazaarRevision rev, bool recurse, IProgressMonitor monitor);
		public abstract void Branch (string branchLocation, string localPath, IProgressMonitor monitor);
		public abstract string GetTextAtRevision (string path, BazaarRevision rev);
		public abstract BazaarRevision[] GetHistory (BazaarRepository repo, string localFile, BazaarRevision since);
		public abstract void Merge (string mergeLocation, string localPath, bool remember, bool overwrite, BazaarRevision start, BazaarRevision end, IProgressMonitor monitor);
		public abstract void Push (string pushLocation, string localPath, bool remember, bool overwrite, IProgressMonitor monitor);
		public abstract void DPush (string pushLocation, string localPath, bool remember, MonoDevelop.Core.IProgressMonitor monitor);
		public abstract void Pull (string pullLocation, string localPath, bool remember, bool overwrite, IProgressMonitor monitor);
		public abstract void Commit (ChangeSet changeSet, IProgressMonitor monitor);
		public abstract DiffInfo[] Diff (string basePath, string[] files);
		public abstract DiffInfo[] Diff (string path, BazaarRevision fromRevision, BazaarRevision toRevision);
		public abstract void Remove (string path, bool force, IProgressMonitor monitor);
		public abstract void Resolve (string path, bool recurse, IProgressMonitor monitor);
		public abstract Dictionary<string, BranchType> GetKnownBranches (string path);
		public abstract void StoreCredentials (string url);
		public abstract void Init (string path);
		public abstract void Ignore (string path);
		public abstract bool IsBound (string path);
		public abstract string GetBoundBranch (string path);
		public abstract void Bind (string branchUrl, string localPath, IProgressMonitor monitor);
		public abstract void Unbind (string localPath, IProgressMonitor monitor);
		public abstract void Uncommit (string localPath, MonoDevelop.Core.IProgressMonitor monitor);
		public abstract Annotation[] GetAnnotations (string localPath);
		public abstract void Export (string localPath, string exportPath, IProgressMonitor monitor);

		#endregion 

		//		public static string ListKindToString (ListKind kind) {
		//			switch (kind) {
		//			case ListKind.Directory:
		//				return "directory";
		//			case ListKind.File:
		//				return "file";
		//			case ListKind.Symlink:
		//				return "symlink";
		//			}// switch
		//
		//			return string.Empty;
		//		}// ListKindToString

		public static RevisionAction ConvertAction (ItemStatus status) {
			switch (status) {
				case ItemStatus.Added:
					return RevisionAction.Add;
				case ItemStatus.Modified:
					return RevisionAction.Modify;
				case ItemStatus.Deleted:
					return RevisionAction.Delete;
				case ItemStatus.Replaced:
					return RevisionAction.Replace;
			}// switch

			return RevisionAction.Other;
		}// ConvertAction

		public static string GetLocalBasePath (string localPath) {
			if (null == localPath){ return string.Empty; }
			if (Directory.Exists (Path.Combine (localPath, ".bzr"))){ return localPath; }

			return GetLocalBasePath (Path.GetDirectoryName (localPath));
		}// GetLocalBasePath

		public static readonly string[] ExportExtensions = {
			".tar",
			".tar.gz",
			".tgz",
			".tar.bz2",
			".tbz2",
			".zip"
		};

		/// <summary>
		/// Determines whether a given path is a valid export target.
		/// </summary>
		public virtual bool IsValidExportPath (string path) {
			if (!string.IsNullOrEmpty (path)) {
				if (Directory.Exists (path) && !IsVersioned (path)) {
					return true;
				}// existing, nonversioned directory

				if (0 == Path.GetExtension (path).Length && !File.Exists (path)) {
					return true;
				}// new directory

				foreach (string extension in ExportExtensions) {
					if (path.EndsWith (extension, StringComparison.OrdinalIgnoreCase)) {
						return true;
					}
				}// supported archive format
			}

			return false;
		}// IsValidExportPath

		public abstract bool IsMergePending (string localPath);
		public abstract bool CanRebase ();
	}
}

