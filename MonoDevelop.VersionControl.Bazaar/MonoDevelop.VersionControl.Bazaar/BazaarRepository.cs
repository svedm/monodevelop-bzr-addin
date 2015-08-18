using System;
using System.Collections.Generic;

using MonoDevelop.Core;
using System.IO;
using System.Linq;
using Gtk;

namespace MonoDevelop.VersionControl.Bazaar
{
	public class BazaarRepository : UrlBasedRepository
	{
		private Dictionary<string,string> tempfiles;
		private Dictionary<FilePath,VersionInfo> statusCache;

		public string LocalBasePath { get; set; }

		public BazaarVersionControl Bazaar
		{
			get { return (BazaarVersionControl)VersionControlSystem; }
		}

		public BazaarRepository()
		{
			Init();
		}

		~BazaarRepository ()
		{
			foreach (string tmpfile in tempfiles.Values)
			{
				if (File.Exists(tmpfile))
				{
					File.Delete(tmpfile);
				}
			}
		}

		public BazaarRepository(BazaarVersionControl vcs, string url)
			: base(vcs)
		{
			Init();
			Url = url;
		}

		private void Init()
		{
			Url = "";
			tempfiles = new Dictionary<string,string>();
			statusCache = new Dictionary<FilePath, VersionInfo>();
		}

		#region implemented abstract members of Repository

		public override string GetBaseText(FilePath localFilePath)
		{
			string localFile = localFilePath.FullPath;

			try
			{
				return Bazaar.GetTextAtRevision(localFile, new BazaarRevision(this, BazaarRevision.HEAD));
			}
			catch (Exception e)
			{
				LoggingService.LogError("Error getting base text", e);
			}

			return localFile;
		}

		protected override Revision[] OnGetHistory(FilePath localFile, Revision since)
		{
			if (null == LocalBasePath)
			{
				LocalBasePath = GetLocalBasePath(localFile.FullPath);
			}
			return Bazaar.GetHistory(this, localFile.FullPath, since);
		}

		protected override IEnumerable<VersionInfo> OnGetVersionInfo(IEnumerable<FilePath> paths, bool getRemoteStatus)
		{
			foreach (var localPath in paths)
			{
				statusCache[localPath] = Bazaar.GetVersionInfo(this, localPath.FullPath, getRemoteStatus);
			}

			return statusCache.Select(s => s.Value);
		}

		protected override VersionInfo[] OnGetDirectoryVersionInfo(FilePath localDirectory, bool getRemoteStatus, bool recursive)
		{
			VersionInfo[] versions = Bazaar.GetDirectoryVersionInfo(this, localDirectory.FullPath, getRemoteStatus, recursive);
			if (null != versions)
			{
				foreach (VersionInfo version in versions)
				{
					statusCache[version.LocalPath] = version;
				}
			}
			return versions;
		}

		protected override Repository OnPublish(string serverPath, FilePath localPath, FilePath[] files, string message, IProgressMonitor monitor)
		{
			serverPath = string.Format("{0}{1}{2}", Url, Url.EndsWith("/") ? string.Empty : "/", serverPath);
			Bazaar.StoreCredentials(serverPath);
			Bazaar.Push(serverPath, localPath.FullPath, false, false, false, monitor);

			return new BazaarRepository(Bazaar, serverPath);
		}

		protected override void OnUpdate(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
		{
			foreach (FilePath localPath in localPaths)
			{
				Bazaar.Update(localPath.FullPath, recurse, monitor);
			}
		}

		protected override void OnCommit(ChangeSet changeSet, IProgressMonitor monitor)
		{
			Bazaar.Commit(changeSet, monitor);
		}

		protected override void OnCheckout(FilePath targetLocalPath, Revision rev, bool recurse, IProgressMonitor monitor)
		{
			Bazaar.StoreCredentials(Url);
			BazaarRevision brev = (null == rev) ? new BazaarRevision(this, BazaarRevision.HEAD) : (BazaarRevision)rev;
			Bazaar.Checkout(Url, targetLocalPath.FullPath, brev, recurse, monitor);
		}

		protected override void OnRevert(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
		{
			foreach (FilePath localPath in localPaths)
			{
				Bazaar.Revert(localPath.FullPath, recurse, monitor, new BazaarRevision(this, BazaarRevision.HEAD));
			}
		}

		protected override void OnRevertRevision(FilePath localPath, Revision revision, IProgressMonitor monitor)
		{
			if (IsModified(BazaarRepository.GetLocalBasePath(localPath)))
			{
				MessageDialog md = new MessageDialog(null, DialogFlags.Modal, 
					                   MessageType.Question, ButtonsType.YesNo, 
					                   GettextCatalog.GetString("You have uncommitted local changes. Revert anyway?"));
				try
				{
					if ((int)ResponseType.Yes != md.Run())
					{
						return;
					}
				}
				finally
				{
					md.Destroy();
				}
			}

			BazaarRevision brev = (BazaarRevision)revision;
			string localPathStr = localPath.FullPath;
			Bazaar.Merge(localPathStr, localPathStr, false, true, brev, (BazaarRevision)(brev.GetPrevious()), monitor);
		}

		protected override void OnRevertToRevision(FilePath localPath, Revision revision, IProgressMonitor monitor)
		{
			if (IsModified(BazaarRepository.GetLocalBasePath(localPath)))
			{
				MessageDialog md = new MessageDialog(null, DialogFlags.Modal, 
					                   MessageType.Question, ButtonsType.YesNo, 
					                   GettextCatalog.GetString("You have uncommitted local changes. Revert anyway?"));
				try
				{
					if ((int)ResponseType.Yes != md.Run())
					{
						return;
					}
				}
				finally
				{
					md.Destroy();
				}
			}

			BazaarRevision brev = (null == revision) ? new BazaarRevision(this, BazaarRevision.HEAD) : (BazaarRevision)revision;
			Bazaar.Revert(localPath.FullPath, true, monitor, brev);
		}

		protected override void OnAdd(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
		{
			foreach (FilePath localPath in localPaths)
			{
				Bazaar.Add(localPath.FullPath, recurse, monitor);
			}
		}

		protected override void OnDeleteFiles(FilePath[] localPaths, bool force, IProgressMonitor monitor, bool keepLocal)
		{
			foreach (FilePath localPath in localPaths)
			{
				Bazaar.Remove(localPath.FullPath, force, monitor);
			}
		}

		protected override void OnDeleteDirectories(FilePath[] localPaths, bool force, IProgressMonitor monitor, bool keepLocal)
		{
			foreach (FilePath localPath in localPaths)
			{
				Bazaar.Remove(localPath.FullPath, force, monitor);
			}
		}

		protected override string OnGetTextAtRevision(FilePath repositoryPath, Revision revision)
		{
			BazaarRevision brev = (null == revision) ? new BazaarRevision(this, BazaarRevision.HEAD) : (BazaarRevision)revision;
			return Bazaar.GetTextAtRevision(repositoryPath.FullPath, brev);
		}

		protected override RevisionPath[] OnGetRevisionChanges(Revision revision)
		{
			return Bazaar.Status(this.RootPath, (BazaarRevision)revision)
				.Where(s => s.Status != ItemStatus.Unchanged && s.Status != ItemStatus.Ignored)
				.Select(status => new RevisionPath(Path.Combine(RootPath, status.Filename), ConvertAction(status.Status), status.Status.ToString())).ToArray();
		}

		protected override void OnIgnore(FilePath[] localPath)
		{
			foreach (var path in localPath)
			{
				Bazaar.Ignore(path.FullPath);
			}
		}

		protected override void OnUnignore(FilePath[] localPath)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region implemented abstract members of UrlBasedRepository

		public override string[] SupportedProtocols
		{
			get { return BazaarVersionControl.protocols; }
		}

		#endregion

		private static RevisionAction ConvertAction(ItemStatus status)
		{
			switch (status)
			{
				case ItemStatus.Added:
					return RevisionAction.Add;
				case ItemStatus.Modified:
					return RevisionAction.Modify;
				case ItemStatus.Deleted:
					return RevisionAction.Delete;
			}

			return RevisionAction.Other;
		}

		internal bool IsVersioned(FilePath localPath)
		{
			if (string.IsNullOrEmpty(GetLocalBasePath(localPath.FullPath)))
			{
				return false;
			}

			var info = GetCachedVersionInfo(localPath, false);
			return (null != info && info.IsVersioned);
		}

		private VersionInfo GetCachedVersionInfo(FilePath localPath, bool getRemoteStatus)
		{
			VersionInfo status = null;
			if (statusCache.ContainsKey(localPath))
			{
				status = statusCache[localPath];
			}
			else
			{
				status = GetVersionInfo(localPath, getRemoteStatus ? VersionInfoQueryFlags.IncludeRemoteStatus : VersionInfoQueryFlags.None);
			}
			return status;
		}

		public virtual bool IsConflicted(FilePath localFile)
		{
			if (string.IsNullOrEmpty(GetLocalBasePath(localFile.FullPath)))
			{
				return false;
			}

			VersionInfo info = GetCachedVersionInfo(localFile, false);
			return (null != info && info.IsVersioned && (0 != (info.Status & VersionStatus.Conflicted)));
		}

		public virtual bool IsModified(FilePath localFile)
		{
			if (string.IsNullOrEmpty(GetLocalBasePath(localFile.FullPath)))
			{
				return false;
			}

			VersionInfo info = GetCachedVersionInfo(localFile, false);
			return (null != info && info.IsVersioned && info.HasLocalChanges);
		}

		public virtual bool IsBound(FilePath localPath)
		{
			return Bazaar.IsBound(localPath.FullPath);
		}

		public virtual void Resolve(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
		{
			foreach (FilePath localPath in localPaths)
				Bazaar.Resolve(localPath.FullPath, recurse, monitor);
		}

		public virtual bool CanPull(FilePath localPath)
		{
			return Directory.Exists(localPath.FullPath) && IsVersioned(localPath);
		}

		public virtual void Pull(string pullLocation, FilePath localPath, bool remember, bool overwrite, IProgressMonitor monitor)
		{
			Bazaar.StoreCredentials(pullLocation);
			Bazaar.Pull(pullLocation, localPath.FullPath, remember, overwrite, monitor);
		}

		public virtual bool CanMerge(FilePath localPath)
		{
			return Directory.Exists(localPath.FullPath) && IsVersioned(localPath);
		}

		public virtual void Merge(string mergeLocation, FilePath localPath, bool remember, bool overwrite, IProgressMonitor monitor)
		{
			Bazaar.StoreCredentials(mergeLocation);
			Bazaar.Merge(mergeLocation, localPath.FullPath, remember, overwrite, new BazaarRevision(this, BazaarRevision.NONE), new BazaarRevision(this, BazaarRevision.NONE), monitor);
		}

		public virtual string GetBoundBranch(FilePath localPath)
		{
			return Bazaar.GetBoundBranch(localPath.FullPath);
		}

		public virtual bool CanBind(FilePath localPath)
		{
			return Directory.Exists(localPath.FullPath) && !IsBound(localPath);
		}

		public virtual void Bind(string branchUrl, FilePath localPath, IProgressMonitor monitor)
		{
			Bazaar.Bind(branchUrl, localPath.FullPath, monitor);
		}

		public virtual bool CanUnbind(FilePath localPath)
		{
			return Directory.Exists(localPath.FullPath) && IsBound(localPath);
		}

		public virtual void Unbind(FilePath localPath, IProgressMonitor monitor)
		{
			Bazaar.Unbind(localPath.FullPath, monitor);
		}

		public virtual bool CanUncommit(FilePath localPath)
		{
			return Directory.Exists(localPath.FullPath) && IsVersioned(localPath);
		}

		public virtual void Uncommit(FilePath localPath, IProgressMonitor monitor)
		{
			Bazaar.Uncommit(localPath.FullPath, monitor);
		}

		public virtual void Push(string pushLocation, FilePath localPath, bool remember, bool overwrite, bool omitHistory, IProgressMonitor monitor)
		{
			Bazaar.StoreCredentials(pushLocation);
			Bazaar.Push(pushLocation, localPath.FullPath, remember, overwrite, omitHistory, monitor);
		}

		public virtual void Export(FilePath localPath, FilePath exportLocation, IProgressMonitor monitor)
		{
			Bazaar.Export(localPath.FullPath, exportLocation.FullPath, monitor);
		}

		public virtual Dictionary<string, BranchType> GetKnownBranches(FilePath localPath)
		{
			return Bazaar.GetKnownBranches(localPath.FullPath);
		}

		public static string GetLocalBasePath(string localPath)
		{
			if (null == localPath)
			{
				return string.Empty;
			}
			if (Directory.Exists(Path.Combine(localPath, ".bzr")))
			{
				return localPath;
			}

			return GetLocalBasePath(Path.GetDirectoryName(localPath));
		}

		public bool CanResolve(FilePath path)
		{
			return IsConflicted(path);
		}
	}
}