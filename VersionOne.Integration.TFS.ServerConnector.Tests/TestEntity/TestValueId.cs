using VersionOne.Integration.TFS.ServerConnector.Entities;
using VersionOne.SDK.APIClient;

namespace VersionOne.Integration.TFS.ServerConnector.Tests.TestEntity
{
	public class TestValueId : ValueId
	{
		private TestValueId(Oid oid, string name) : base(oid, name) { }

		public static TestValueId Create(string name, string type, int id)
		{
			var oid = new TestOid(new TestAssetType(type), id, null);
			return new TestValueId(oid, name);
		}
	}
}