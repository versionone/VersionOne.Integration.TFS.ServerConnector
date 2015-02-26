using NUnit.Framework;
using Rhino.Mocks;
using VersionOne.Integration.TFS.ServerConnector.Filters;
using VersionOne.SDK.APIClient;

namespace VersionOne.Integration.TFS.ServerConnector.Tests.Filters
{
	[TestFixture]
	public class FilterTester
	{
		private readonly MockRepository _repository = new MockRepository();

		private IAssetType _assetType;
		private IAttributeDefinition _definition;

		[SetUp]
		public void SetUp()
		{
			_assetType = _repository.StrictMock<IAssetType>();
			_definition = _repository.StrictMock<IAttributeDefinition>();
		}

		[Test]
		public void CreateEmptyFilter()
		{
			var filter = Filter.Empty();
			var result = filter.GetFilter(_assetType);

			Assert.AreEqual(false, result.HasTerms);
		}

		[Test]
		public void CreateFilter()
		{
			const string filterToken = "(Type='Custom_BaF_Status%3a1047'|Type!='Custom_BaF_Status%3a1048')";
			var filter = Filter.Or("Custom_BaFstatus2").Equal("Custom_BaF_Status:1047").NotEqual("Custom_BaF_Status:1048");

			Expect.Call(_definition.Token).Repeat.Twice().Return("Type");
			Expect.Call(_assetType.GetAttributeDefinition(null)).IgnoreArguments().Repeat.Twice().Return(_definition);

			_repository.ReplayAll();
			var result = filter.GetFilter(_assetType);
			var token = result.Token;
			_repository.VerifyAll();

			Assert.AreEqual(true, result.HasTerms);
			Assert.AreEqual(filterToken, token);
		}
	}
}