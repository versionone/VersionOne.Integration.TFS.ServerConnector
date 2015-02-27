using VersionOne.SDK.APIClient;

﻿namespace VersionOne.Integration.Tfs.ServerConnector.Tests.TestEntity
{
	public class TestOid : Oid
	{
		public TestOid(IAssetType assetType, int id, int? moment)
			: base(assetType, id, moment)
		{
		}

	}
}
