using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using VersionOne.SDK.APIClient;
using VersionOne.ServerConnector.Entities;
using VersionOne.ServerConnector.Filters;
using VersionOne.ServerConnector.Tests.TestEntity;
using VersionOne.ServiceHost.Core.Logging;

namespace VersionOne.ServerConnector.Tests
{
	[TestFixture]
	public class VersionOneProcessorTester
	{
		private TestVersionOneProcessor _processor;
		private IServices _mockServices;
		private IMetaModel _mockMetaModel;
		private IQueryBuilder _mockQueryBuilder;

		private readonly MockRepository _repository = new MockRepository();

		[SetUp]
		public void SetUp()
		{
			var logger = _repository.Stub<ILogger>();
			_mockServices = _repository.StrictMock<IServices>();
			_mockMetaModel = _repository.StrictMock<IMetaModel>();
			_mockQueryBuilder = _repository.StrictMock<IQueryBuilder>();

			_processor = new TestVersionOneProcessor(null, logger);
			_processor.ConnectTest(_mockServices, _mockMetaModel, _mockQueryBuilder);
		}

		[Test]
		public void AddLinkToWorkitem()
		{
			const string url = "http://qqq.com";
			const string title = "Url title";
			var workitemAsset = new Asset(new TestOid(new TestAssetType("Workitem"), 100, null));
			var workitem = new TestWorkitem(workitemAsset, null);
			var link = new Link(url, title);
			var linkAsset = new TestAssetType("Link");
			var asset = new Asset(new TestOid(new TestAssetType("Link"), 10, null));

			Expect.Call(_mockMetaModel.GetAssetType(VersionOneProcessor.LinkType)).Return(linkAsset);
			Expect.Call(_mockQueryBuilder.Query(string.Empty, Filter.Empty())).IgnoreArguments().Return(new AssetList());
			Expect.Call(_mockServices.New(null, null)).IgnoreArguments().Return(asset);
			Expect.Call(() => _mockServices.Save(asset));

			_repository.ReplayAll();
			_processor.AddLinkToEntity(workitem, link);
			_repository.VerifyAll();
		}

		[Test]
		public void AddLinkToWorkitemWithExistingLink()
		{
			const string type = "Link";
			const string url = "http://qqq.com";
			const string title = "Url title";
			var workitemAsset = new Asset(new TestOid(new TestAssetType("Workitem"), 100, null));
			var workitem = new TestWorkitem(workitemAsset, null);
			var link = new Link(url, title);
			var linkAsset = new TestAssetType(type);
			var definitions = new Dictionary<string, IAttributeDefinition> {
                {Entity.NameProperty, new TestAttributeDefinition(linkAsset)},
                {Link.OnMenuProperty, new TestAttributeDefinition(linkAsset)},
                {Link.UrlProperty, new TestAttributeDefinition(linkAsset)},
            };
			var linkOid = new TestOid(new TestAssetType(type, definitions), 10, null);
			var existedLink = new Asset(linkOid);

			Expect.Call(_mockMetaModel.GetAssetType(VersionOneProcessor.LinkType)).Return(linkAsset);
			Expect.Call(_mockQueryBuilder.Query(string.Empty, Filter.Empty())).IgnoreArguments().Return(new AssetList { existedLink });

			_repository.ReplayAll();
			_processor.AddLinkToEntity(workitem, link);
			_repository.VerifyAll();
		}

		[Test]
		[Ignore("EntityFactory should be injected to simplify the overall process")]
		public void CreateWorkitem()
		{
			const string type = "Story";
			const string title = "Story Name";
			const string description = "Story description";
			const string projectToken = "Scope:0";
			const string externalFieldName = "FieldName";
			const string externalId = "externalId";
			const string externalSystemName = "External System Name";
			const string priorityId = "Priority:12";
			const string owners = "Onwer_1,Owners_2";

			var memberAssetType = new TestAssetType("Member");
			var projectAssetType = new TestAssetType("Project");
			var priorityAssetType = new TestAssetType("Priority");

			var storyAttributes = new Dictionary<string, IAttributeDefinition> {
                {"Owners", new TestAttributeDefinition(memberAssetType, true, false, false)},
            };
			var storyAssetType = new TestAssetType("Story", storyAttributes);

			var source = TestValueId.Create(externalSystemName, "Source", 333);
			var sources = new PropertyValues(new List<ValueId> { source });
			var assetStory = new Asset(storyAssetType);
			var ownersAssets = new AssetList {
                new Asset(new TestOid(new TestAssetType("Member"), 1, null)),
                new Asset(new TestOid(new TestAssetType("Member"), 2, null)),
            };
			var queryResult = new QueryResult(ownersAssets, 2, null);

			Expect.Call(_mockMetaModel.GetAssetType("Scope")).Return(projectAssetType);
			Expect.Call(_mockQueryBuilder.QueryPropertyValues(VersionOneProcessor.WorkitemSourceType)).Return(sources);
			Expect.Call(_mockMetaModel.GetAssetType("Story")).Return(storyAssetType);
			Expect.Call(_mockServices.New(storyAssetType, Oid.Null)).Return(assetStory);
			Expect.Call(_mockMetaModel.GetAssetType("Member")).Return(memberAssetType);
			Expect.Call(_mockServices.Retrieve(null)).IgnoreArguments().Return(queryResult);
			Expect.Call(_mockMetaModel.GetAssetType("Priority")).Return(priorityAssetType);
			Expect.Call(() => _mockServices.Save(assetStory));

			Expect.Call(_mockMetaModel.GetAssetType("Story")).Return(storyAssetType);
			Expect.Call(_mockQueryBuilder.Query("Story", new FilterTerm(null))).IgnoreArguments().Return(new AssetList { assetStory });
			Expect.Call(_mockQueryBuilder.ListPropertyValues).Return(null);
			Expect.Call(_mockQueryBuilder.TypeResolver).Return(null);

			_repository.ReplayAll();

			_processor.CreateWorkitem(type, title, description, projectToken,
				externalFieldName, externalId, externalSystemName,
				priorityId, owners);
			_repository.VerifyAll();
		}
	}
}