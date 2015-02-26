using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using VersionOne.Integration.TFS.ServerConnector.Entities;
using VersionOne.Integration.TFS.ServerConnector.StartupValidation;
using VersionOne.Integration.TFS.ServerConnector.Tests.TestEntity;
using VersionOne.ServiceHost.Core.Configuration;

namespace VersionOne.Integration.TFS.ServerConnector.Tests.StartupValidation
{
	[TestFixture]
	public class VersionOnePriorityValidatorTester : BaseLkkTester
	{
		[Test]
		public void Validate()
		{
			var priorities = new Dictionary<MappingInfo, MappingInfo> {
                                 { new MappingInfo("123", "Lkk priority 1"), new MappingInfo("P:123", "V1 prioprity 1") },
                                 { new MappingInfo("456", "Lkk priority 2"), new MappingInfo("P:456", "V1 prioprity 2") }
                             };
			var validator = new V1PriorityValidator(priorities.Values) { Logger = LoggerMock, V1Processor = V1ProcessorMock };

			var workitemPriorities = new List<ValueId> {
										TestValueId.Create("V1 priority 1", "P", 123),
										TestValueId.Create("V1 priority 2", "P", 456),
                                     };

			Expect.Call(V1ProcessorMock.GetWorkitemPriorities()).Return(workitemPriorities);

			Repository.ReplayAll();
			var result = validator.Validate();
			Repository.VerifyAll();

			Assert.IsTrue(result, "Not all priorities exist");
		}
	}
}