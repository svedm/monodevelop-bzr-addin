using System;
using System.Collections.Generic;

using MonoDevelop.Core;
using System.IO;

namespace MonoDevelop.VersionControl.Bazaar
{
	public class BazaarRepository : UrlBasedRepository
	{
		private Dictionary<string,string> tempfiles;
		private Dictionary<FilePath,VersionInfo> statusCache;

		public BazaarVersionControl Bazaar {
			get { return (BazaarVersionControl) VersionControlSystem; }
		}

		public BazaarRepository ()
		{
			Init ();
		}

		public BazaarRepository (BazaarVersionControl vcs,string url) : base (vcs)
		{
			Init ();
			Url = url;
		}

		private void Init ()
		{
			tempfiles = new Dictionary<string,string>();
			statusCache = new Dictionary<FilePath, VersionInfo>();
		}

		#region implemented abstract members of Repository

		public override string GetBaseText(FilePath localFile)
		{
			throw new NotImplementedException();
		}

		protected override Revision[] OnGetHistory(FilePath localFile, Revision since)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<VersionInfo> OnGetVersionInfo(IEnumerable<FilePath> paths, bool getRemoteStatus)
		{
			throw new NotImplementedException();
		}

		protected override VersionInfo[] OnGetDirectoryVersionInfo(FilePath localDirectory, bool getRemoteStatus, bool recursive)
		{
			throw new NotImplementedException();
		}

		protected override Repository OnPublish(string serverPath, FilePath localPath, FilePath[] files, string message, IProgressMonitor monitor)
		{
			throw new NotImplementedException();
		}

		protected override void OnUpdate(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
		{
			throw new NotImplementedException();
		}

		protected override void OnCommit(ChangeSet changeSet, IProgressMonitor monitor)
		{
			throw new NotImplementedException();
		}

		protected override void OnCheckout(FilePath targetLocalPath, Revision rev, bool recurse, IProgressMonitor monitor)
		{
			throw new NotImplementedException();
		}

		protected override void OnRevert(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
		{
			throw new NotImplementedException();
		}

		protected override void OnRevertRevision(FilePath localPath, Revision revision, IProgressMonitor monitor)
		{
			throw new NotImplementedException();
		}

		protected override void OnRevertToRevision(FilePath localPath, Revision revision, IProgressMonitor monitor)
		{
			throw new NotImplementedException();
		}

		protected override void OnAdd(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
		{
			throw new NotImplementedException();
		}

		protected override void OnDeleteFiles(FilePath[] localPaths, bool force, IProgressMonitor monitor, bool keepLocal)
		{
			throw new NotImplementedException();
		}

		protected override void OnDeleteDirectories(FilePath[] localPaths, bool force, IProgressMonitor monitor, bool keepLocal)
		{
			throw new NotImplementedException();
		}

		protected override string OnGetTextAtRevision(FilePath repositoryPath, Revision revision)
		{
			throw new NotImplementedException();
		}

		protected override RevisionPath[] OnGetRevisionChanges(Revision revision)
		{
			throw new NotImplementedException();
		}

		protected override void OnIgnore(FilePath[] localPath)
		{
			throw new NotImplementedException();
		}

		protected override void OnUnignore(FilePath[] localPath)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region implemented abstract members of UrlBasedRepository

		public override string[] SupportedProtocols
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		private VersionInfo GetCachedVersionInfo (FilePath localPath, bool getRemoteStatus)
		{
			VersionInfo status = null;
			if (statusCache.ContainsKey (localPath)) {
				status = statusCache[localPath];
			} else {
				status = GetVersionInfo (localPath, getRemoteStatus ? VersionInfoQueryFlags.IncludeRemoteStatus : VersionInfoQueryFlags.None);
			}
			return status;
		}

		public virtual bool IsConflicted (FilePath localFile)
		{
			if (string.IsNullOrEmpty (GetLocalBasePath (localFile.FullPath))) {
				return false;
			}

			VersionInfo info = GetCachedVersionInfo (localFile, false);
			return (null != info && info.IsVersioned && (0 != (info.Status & VersionStatus.Conflicted)));
		}

		public virtual void Resolve (FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
		{
			foreach (FilePath localPath in localPaths)
				Bazaar.Resolve (localPath.FullPath, recurse, monitor);
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
			return IsConflicted (path);
		}
	}

}