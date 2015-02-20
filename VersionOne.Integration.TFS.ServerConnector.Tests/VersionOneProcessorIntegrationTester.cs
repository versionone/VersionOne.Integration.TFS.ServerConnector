using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VersionOne.SDK.APIClient;
using VersionOne.ServerConnector.Entities;
using VersionOne.ServerConnector.Filters;
using VersionOne.ServiceHost.Core.Configuration;
using VersionOne.ServiceHost.Core.Logging;

namespace VersionOne.ServerConnector.Tests
{
	[TestFixture]
	[Ignore("Integration tests")]
	public class VersionOneProcessorIntegrationTester : BaseIntegrationTester
	{
		[Test]
		public void GetWorkitemsByProjectId()
		{
			const string storyName = "Story 1";
			const string defectName = "Defect 1";
			const string storyDescription = "Description for story";
			const string defectDescription = "Description for defect";
			const string featureGroupName = "Feature Name";
			const string teamName = "Team Name";
			const string sprintName = "Sprint Name";

			var schedule = AssetDisposer.CreateAndRegisterForDisposal(() => CreateSchedule("Schedule for TEST"));
			var scope = AssetDisposer.CreateAndRegisterForDisposal(() => CreateProject("_INTEG_TEST", schedule.Oid, RootProjectToken));
			var sprint = AssetDisposer.CreateAndRegisterForDisposal(() => CreateSprint(sprintName, schedule.Oid));
			var featureGroup = AssetDisposer.CreateAndRegisterForDisposal(() => CreateFeatureGroup(featureGroupName, scope.Oid, null));
			var team = AssetDisposer.CreateAndRegisterForDisposal(() => CreateTeam(teamName));
			var story = AssetDisposer.CreateAndRegisterForDisposal(() => CreateStory(storyName, storyDescription, scope.Oid, featureGroup.Oid, team.Oid, sprint.Oid));
			var defect = AssetDisposer.CreateAndRegisterForDisposal(() => CreateDefect(defectName, defectDescription, scope.Oid, featureGroup.Oid, team.Oid, sprint.Oid));

			var workitems = V1Processor.GetPrimaryWorkitems(Filter.Equal(VersionOneProcessor.ScopeType, scope.Oid.Token));
			Assert.AreEqual(2, workitems.Count, "Incorrect number of workitems.");

			var persistedStory = workitems.FirstOrDefault(w => w.Name.Equals(storyName));
			Assert.IsNotNull(persistedStory);
			Assert.AreEqual(VersionOneProcessor.StoryType, persistedStory.TypeName);
			Assert.AreEqual(storyDescription, persistedStory.Description);
			Assert.AreEqual(story.Oid.Momentless.Token, persistedStory.Id);
			Assert.AreEqual(featureGroupName, persistedStory.FeatureGroupName);
			Assert.AreEqual(teamName, persistedStory.Team);
			Assert.AreEqual(sprintName, persistedStory.SprintName);
			Assert.IsNotNull(persistedStory.Order);

			var persistedDefect = workitems.FirstOrDefault(w => w.Name.Equals(defectName));
			Assert.IsNotNull(persistedDefect);
			Assert.AreEqual(VersionOneProcessor.DefectType, persistedDefect.TypeName);
			Assert.AreEqual(defectDescription, persistedDefect.Description);
			Assert.AreEqual(defect.Oid.Momentless.Token, persistedDefect.Id);
			Assert.AreEqual(featureGroupName, persistedDefect.FeatureGroupName);
			Assert.AreEqual(teamName, persistedDefect.Team);
			Assert.AreEqual(sprintName, persistedDefect.SprintName);
			Assert.IsNotNull(persistedDefect.Order);
		}

		[Test]
		public void GetWorkitems()
		{
			const string storyName = "Retrieve workitems along with owners - a story";
			const string defectName = "Retrieve workitems along with owners - a defect";
			const string storyDescription = "Description for story";
			const string defectDescription = "Description for defect";

			var scope = AssetDisposer.CreateAndRegisterForDisposal(() => CreateProject("_INTEG_TEST", Oid.Null, RootProjectToken));
			var story = AssetDisposer.CreateAndRegisterForDisposal(() => CreateStory(storyName, storyDescription, scope.Oid, Oid.Null, Oid.Null, Oid.Null));
			var defect = AssetDisposer.CreateAndRegisterForDisposal(() => CreateDefect(defectName, defectDescription, scope.Oid, Oid.Null, Oid.Null, Oid.Null));

			var workitems = V1Processor.GetWorkitems(VersionOneProcessor.StoryType, Filter.Equal(Entity.NameProperty, storyName));
			Assert.IsTrue(workitems.Any(x => x.Id.Equals(story.Oid.Momentless.Token)));
			Assert.IsFalse(workitems.Any(x => x.Id.Equals(defect.Oid.Momentless.Token)));

			workitems = V1Processor.GetWorkitems(VersionOneProcessor.DefectType, Filter.Equal(Entity.NameProperty, defectName));
			Assert.IsTrue(workitems.Any(x => x.Id.Equals(defect.Oid.Momentless.Token)));
			Assert.IsFalse(workitems.Any(x => x.Id.Equals(story.Oid.Momentless.Token)));
		}

		[Test]
		public void GetClosedWorkitemsByProjectId()
		{
			const string storyName = "Story 1";
			const string defectName = "Defect 1";
			const string storyDescription = "Description for story";
			const string defectDescription = "Description for defect";
			const string featureGroupName = "Feature Name";
			const string teamName = "Team Name";
			const string sprintName = "Sprint Name";

			var assetSchedule = AssetDisposer.CreateAndRegisterForDisposal(() => CreateSchedule("Schedule for TEST"));
			var assetScope = AssetDisposer.CreateAndRegisterForDisposal(() => CreateProject("_INTEG_TEST", assetSchedule.Oid, RootProjectToken));
			var assetSprint = AssetDisposer.CreateAndRegisterForDisposal(() => CreateSprint(sprintName, assetSchedule.Oid));
			var assetFeatureGroup = AssetDisposer.CreateAndRegisterForDisposal(() => CreateFeatureGroup(featureGroupName, assetScope.Oid, null));
			var assetTeam = AssetDisposer.CreateAndRegisterForDisposal(() => CreateTeam(teamName));
			var assetStory = AssetDisposer.CreateAndRegisterForDisposal(() => CreateStory(storyName, storyDescription, assetScope.Oid, assetFeatureGroup.Oid, assetTeam.Oid, assetSprint.Oid));
			AssetDisposer.CreateAndRegisterForDisposal(() => CreateDefect(defectName, defectDescription, assetScope.Oid, assetFeatureGroup.Oid, assetTeam.Oid, assetSprint.Oid));

			ExecuteOperation(assetStory, "Inactivate");

			var filter = GroupFilter.And(Filter.Closed(true), Filter.Equal(Workitem.ScopeProperty, assetScope.Oid.Token));
			var workitems = V1Processor.GetPrimaryWorkitems(filter);
			Assert.AreEqual(1, workitems.Count, "Incorrect number of workitems.");

			var story = workitems.FirstOrDefault(w => w.Name.Equals(storyName));
			Assert.IsNotNull(story);
			Assert.AreEqual("Story", story.TypeName);
			Assert.AreEqual(storyDescription, story.Description);
			Assert.AreEqual(assetStory.Oid.Momentless.Token, story.Id);
			Assert.AreEqual(featureGroupName, story.FeatureGroupName);
			Assert.AreEqual(teamName, story.Team);
			Assert.AreEqual(sprintName, story.SprintName);
			Assert.IsNotNull(story.Order);

			var defect = workitems.FirstOrDefault(w => w.Name.Equals(defectName));
			Assert.IsNull(defect);

			ExecuteOperation(assetStory, VersionOneProcessor.ReactivateOperation);
		}

		[Test]
		public void UpdateWorkitem()
		{
			const string storyName = "Story 1 update";
			const string storyDescription = "Description for story";
			const string url = "http://aaaa.com";
			var reference = Guid.NewGuid().ToString();

			var assetScope = AssetDisposer.CreateAndRegisterForDisposal(() => CreateProject("_INTEG_TEST", null, RootProjectToken));
			AssetDisposer.CreateAndRegisterForDisposal(() => CreateStory(storyName, storyDescription, assetScope.Oid, null, null, null));
			var filter = GroupFilter.And(Filter.Closed(false), Filter.Equal(Workitem.ScopeProperty, assetScope.Oid.Token));
			var workitems = V1Processor.GetPrimaryWorkitems(filter);
			var story = workitems.FirstOrDefault(w => w.Name.Equals(storyName));
			Assert.IsNotNull(story);
			Assert.AreEqual(null, story.Reference);

			story.Reference = reference;
			V1Processor.Save(story);
			workitems = V1Processor.GetPrimaryWorkitems(filter);
			V1Processor.AddLinkToEntity(story, new Link(url, "Lkk test link"));
			story = workitems.FirstOrDefault(w => w.Name.Equals(storyName));
			Assert.IsNotNull(story);
			Assert.AreEqual(reference, story.Reference);
		}

		[Test]
		public void CreateLinkForWorkitem()
		{
			const string storyName = "Story 1 update";
			const string storyDescription = "Description for story";
			const string url = "http://aaaa.com";
			const string title = "Lkk test link";
			const bool onMenu = true;

			var assetScope = AssetDisposer.CreateAndRegisterForDisposal(() => CreateProject("_INTEG_TEST", null, RootProjectToken));
			AssetDisposer.CreateAndRegisterForDisposal(() => CreateStory(storyName, storyDescription, assetScope.Oid, null, null, null));
			var filter = GroupFilter.And(Filter.Closed(false), Filter.Equal(Workitem.ScopeProperty, assetScope.Oid.Token));
			var workitems = V1Processor.GetPrimaryWorkitems(filter);
			var story = workitems.FirstOrDefault(w => storyName.Equals(w.Name));

			V1Processor.AddLinkToEntity(story, new Link(url, "Lkk test link", onMenu));
			var links = V1Processor.GetWorkitemLinks(story, Filter.Equal(Link.UrlProperty, url));
			Assert.AreEqual(1, links.Count, "Incorrect link number.");
			Assert.AreEqual(url, links[0].Url, "Incorrect url.");
			Assert.AreEqual(title, links[0].Title, "Incorrect title.");
			Assert.AreEqual(onMenu, links[0].OnMenu, "Incorrect on menu status.");
		}

		[Test]
		public void CreateStatus()
		{
			const string newStatusName = "Integration Test Status";
			var newStatus = V1Processor.CreateWorkitemStatus(newStatusName);

			var asset = AssetDisposer.CreateAndRegisterForDisposal(() => LoadAsset(Oid.FromToken(newStatus.Token, MetaModel), VersionOneProcessor.WorkitemStatusType, "Name"));
			var nameDef = asset.AssetType.GetAttributeDefinition("Name");
			Assert.AreEqual(newStatusName, asset.GetAttribute(nameDef).Value);
		}

		[Test]
		public void CreatePriority()
		{
			const string priorityName = "Integration Test Priority";
			var priority = V1Processor.CreateWorkitemPriority(priorityName);

			var asset = AssetDisposer.CreateAndRegisterForDisposal(() => LoadAsset(Oid.FromToken(priority.Token, MetaModel), VersionOneProcessor.WorkitemStatusType, "Name"));
			var nameDef = asset.AssetType.GetAttributeDefinition("Name");
			Assert.AreEqual(priorityName, asset.GetAttribute(nameDef).Value);
		}

		[Test]
		public void GetWorkitemStatuses()
		{
			var statuses = V1Processor.GetWorkitemStatuses();
			Assert.IsTrue(statuses.Count > 0);

			var statusNames = statuses.Select(x => x.Name.ToLower()).ToList();
			Assert.IsTrue(statusNames.Contains("done"));
			Assert.IsTrue(statusNames.Contains("future"));
		}

		[Test]
		public void GetWorkitemPriorities()
		{
			var priorities = V1Processor.GetWorkitemPriorities();
			Assert.IsTrue(priorities.Count > 0);

			var priorityNames = priorities.Select(x => x.Name.ToLower()).ToList();
			Assert.IsTrue(priorityNames.Contains("low"));
			Assert.IsTrue(priorityNames.Contains("medium"));
			Assert.IsTrue(priorityNames.Contains("high"));
		}

		[Test]
		public void GetBuildRunStatuses()
		{
			var statuses = V1Processor.GetBuildRunStatuses();
			Assert.IsTrue(statuses.Count > 0);

			var statusNames = statuses.Select(x => x.Name).ToList();
			Assert.IsTrue(statusNames.Contains("Passed"));
			Assert.IsTrue(statusNames.Contains("Failed"));
		}

		[Test]
		public void SetWorkitemPriority()
		{
			const string storyName = "Story 1";
			const string storyDescription = "Description for story";

			var scope = AssetDisposer.CreateAndRegisterForDisposal(() => CreateProject("_INTEG_TEST", null, RootProjectToken));
			AssetDisposer.CreateAndRegisterForDisposal(() => CreateStory(storyName, storyDescription, scope.Oid, null, null, null));
			var filter = GroupFilter.And(Filter.Closed(false), Filter.Equal(Workitem.ScopeProperty, scope.Oid.Token));
			var workitems = V1Processor.GetPrimaryWorkitems(filter);
			var priorities = V1Processor.GetWorkitemPriorities();

			var persistedStory = workitems.FirstOrDefault(w => w.Name.Equals(storyName));
			Assert.IsNotNull(persistedStory);
			Assert.AreEqual(null, persistedStory.PriorityToken);
			persistedStory.PriorityToken = priorities[0].Token;
			V1Processor.Save(persistedStory);

			workitems = V1Processor.GetPrimaryWorkitems(filter);
			persistedStory = workitems.FirstOrDefault(w => w.Name.Equals(storyName));
			Assert.IsNotNull(persistedStory);
			Assert.AreEqual(priorities[0].Token, persistedStory.PriorityToken);
		}

		[Test]
		public void GetFeatureGroupsByProjectIncludingSubprojects()
		{
			const string featureGroupName1 = "Feature Name 1";
			const string featureGroupName2 = "Feature Name 2";
			const string storyName1 = "Story Name 1";
			const string storyName2 = "Story Name 2";

			var parentScope = AssetDisposer.CreateAndRegisterForDisposal(() => CreateProject("Parent Project", null, RootProjectToken));
			var childScope = AssetDisposer.CreateAndRegisterForDisposal(() => CreateProject("Parent Project", null, parentScope.Oid.Momentless.Token));
			var featureGroup1 = AssetDisposer.CreateAndRegisterForDisposal(() => CreateFeatureGroup(featureGroupName1, parentScope.Oid, null));
			AssetDisposer.CreateAndRegisterForDisposal(() => CreateStory(storyName1, "desc 1", parentScope.Oid, featureGroup1.Oid, null, null));
			var featureGroup2 = AssetDisposer.CreateAndRegisterForDisposal(() => CreateFeatureGroup(featureGroupName2, childScope.Oid, null));
			AssetDisposer.CreateAndRegisterForDisposal(() => CreateStory(storyName2, "desc 2", childScope.Oid, featureGroup2.Oid, null, null));

			var filter = Filter.Equal("Scope.ChildrenMeAndDown", parentScope.Oid.Momentless.Token);
			var result = V1Processor.GetFeatureGroups(filter, Filter.Empty());
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.Any(item => item.Name.Equals(featureGroupName1)));
			Assert.AreEqual(storyName1, result[0].Children[0].Name);

			filter = Filter.Equal("Scope.ParentMeAndUp", childScope.Oid.Momentless.Token);
			result = V1Processor.GetFeatureGroups(filter, Filter.Empty());
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.Any(item => item.Name.Equals(featureGroupName2)));
			Assert.AreEqual(storyName2, result[0].Children[0].Name);

			filter = Filter.Equal(Entity.ScopeParentAndUpProperty, parentScope.Oid.Momentless.Token);
			result = V1Processor.GetFeatureGroups(filter, Filter.Empty());
			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result.Any(item => item.Name.Equals(featureGroupName1)));
			Assert.IsTrue(result.Any(item => item.Name.Equals(featureGroupName2)));
		}

		[Test]
		public void ConnectUsingSettingsObject()
		{
			var settings = new VersionOneSettings { IntegratedAuth = false, Url = V1Url, Username = Username, Password = Password, };
			var v1Processor = new VersionOneProcessor(settings, MockRepository.Stub<ILogger>());
			v1Processor.ValidateConnection();
		}

		[Test]
		public void GetMembers()
		{
			ICollection<Member> members = V1Processor.GetMembers(Filter.Empty());
			Assert.IsTrue(members.Count > 0);
			Assert.IsTrue(members.Any(member => member.Name.Equals("admin")));
		}

		[Test]
		public void ValidationFailureWithNullLogger()
		{
			var settings = new VersionOneSettings { IntegratedAuth = false, Url = "http://example.com/", Username = Username, Password = Password, };
			var v1Processor = new VersionOneProcessor(settings, null);
			v1Processor.ValidateConnection();
		}

		[Test]
		public void LookupProjects()
		{
			const string name = "system";
			var projects = V1Processor.LookupProjects(name);

			Assert.IsTrue(projects.Count > 0);
			Assert.IsTrue(projects.Any(x => x.Id.Equals("Scope:0")));
		}

		[Test]
		public void CreateProject()
		{
			const string name = "Test project";
			var newProject = V1Processor.CreateProject(name);

			Assert.IsNotNull(newProject);
			Assert.IsNotNull(newProject.Id);
			Assert.AreEqual(name, newProject.Name);
		}

		[Test]
		public void GetCustomTextFields()
		{
			var fields = V1Processor.GetCustomTextFields("PrimaryWorkitem");

			Assert.IsTrue(fields.Count > 0);
			Assert.IsTrue(fields.Any(x => x.Name.Equals("Reference")));
		}
	}
}