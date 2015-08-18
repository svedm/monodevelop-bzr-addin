using System;

namespace MonoDevelop.VersionControl.Bazaar
{
	public class BazaarRevision : Revision
	{
		public readonly string Rev;

		public RevisionPath[] ChangedFiles	{ get;	set; }

		public static readonly string HEAD = "-1";
		public static readonly string FIRST = "1";
		public static readonly string NONE = "NONE";

		public BazaarRevision(Repository repo, string rev)
			: base(repo)
		{
			Rev = rev;
		}

		public BazaarRevision(Repository repo, string rev, DateTime time, string author, string message, RevisionPath[] changedFiles)
			: base(repo, time, author, message)
		{
			Rev = rev;
		}


		#region implemented abstract members of Revision

		public override Revision GetPrevious()
		{
			return new BazaarRevision(Repository, string.Format("before:{0}", Rev));
		}

		#endregion
		
	}
}
