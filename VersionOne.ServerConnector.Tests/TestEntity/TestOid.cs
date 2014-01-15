using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Tests.TestEntity
{
	public class TestOid : Oid
	{
		public TestOid(IAssetType assetType, int id, int? moment)
			: base(assetType, id, moment)
		{
		}

	}
}
