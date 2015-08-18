using System;
using System.Collections.Generic;

using MonoDevelop.Core;
using System.IO;

namespace MonoDevelop.VersionControl.Bazaar
{
	public class BazaarRepository : UrlBasedRepository
	{
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
	}

}
