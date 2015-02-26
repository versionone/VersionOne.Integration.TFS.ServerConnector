using VersionOne.SDK.APIClient;

namespace VersionOne.Integration.TFS.ServerConnector.Tests.TestEntity
{
	public class TestOid : Oid
	{
		public TestOid(IAssetType assetType, int id, int? moment)
			: base(assetType, id, moment)
		{
		}

	}
}
