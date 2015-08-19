using System;
using MonoDevelop.VersionControl.Tests;
using NUnit.Framework;
using MonoDevelop.Core;
using System.IO;
using MonoDevelop.VersionControl.Bazaar;
using MonoDevelop.VersionControl;
using System.Linq;

namespace MonoDevelopVersionControl.Bazaar.Tests
{
	[TestFixture]
	public class BaseBazaarRepositoryTests : BaseRepoUtilsTest
	{
		#region implemented abstract members of BaseRepoUtilsTest

		[SetUp]
		public override void Setup()
		{
			RemotePath = new FilePath (FileService.CreateTempDirectory () + Path.DirectorySeparatorChar);
			LocalPath = new FilePath (FileService.CreateTempDirectory () + Path.DirectorySeparatorChar);
			Directory.CreateDirectory (RemotePath.FullPath + "repo.git");
			RemoteUrl = "file://" + (Platform.IsWindows ? "/" : "") + RemotePath.FullPath;
			var bc = new BazaarCLibClient();
			bc.Init(RemoteUrl);
			bc.Checkout(RemotePath, LocalPath, new BazaarRevision(new BazaarRepository(), BazaarRevision.NONE), true, null);

			Repo = GetRepo(LocalPath, RemoteUrl);
			ModifyPath(Repo, ref LocalPath);
			DotDir = ".bzr";
		}
		protected override NUnit.Framework.Constraints.IResolveConstraint IsCorrectType()
		{
			return Is.InstanceOf<BazaarRepository>();
		}
		protected override void TestValidUrl()
		{
			var repo2 = (BazaarRepository)Repo;
			Assert.IsTrue (repo2.IsUrlValid ("bzr://github.com:80/mono/monodevelop"));
			Assert.IsTrue (repo2.IsUrlValid ("bzr+ssh://user@host.com:80/mono/monodevelop"));
			Assert.IsTrue (repo2.IsUrlValid ("http://github.com:80/mono/monodevelop"));
			Assert.IsTrue (repo2.IsUrlValid ("file:///mono/monodevelop"));
		}
		protected override void TestDiff()
		{
			string difftext = @"--- a/testfile
+++ b/testfile
@@ -0,0 +1 @@
+text
\ No newline at end of file
";
			if (Platform.IsWindows)
				difftext = difftext.Replace ("\r\n", "\n");
			Assert.AreEqual (difftext, Repo.GenerateDiff (LocalPath + "testfile", Repo.GetVersionInfo (LocalPath + "testfile", VersionInfoQueryFlags.IgnoreCache)).Content);
		}

		protected override MonoDevelop.VersionControl.Revision GetHeadRevision()
		{
			var repo2 = (BazaarRepository)Repo;
			return new BazaarRevision(repo2, BazaarRevision.HEAD);
		}

		protected override void BlameExtraInternals(MonoDevelop.VersionControl.Annotation[] annotations)
		{
			for (int i = 0; i < 2; i++) {
				Assert.IsTrue (annotations [i].HasEmail);
				Assert.AreEqual (Author, annotations [i].Author);
				Assert.AreEqual (String.Format ("<{0}>", Email), annotations [i].Email);
			}
			Assert.IsTrue (annotations [2].HasDate);
		}

		protected override MonoDevelop.VersionControl.Repository GetRepo(string path, string url)
		{
			return new BazaarRepository((BazaarVersionControl)VersionControlService.GetVersionControlSystems().First (id => id.Name == "Bazaar"), url);
		}
		#endregion
	}
}