using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using VersionOne.ServerConnector.StartupValidation;
using VersionOne.ServiceHost.Core.Configuration;

namespace VersionOne.ServerConnector.Tests.StartupValidation
{
	[TestFixture]
	public class VersionOneProjectValidatorTester : BaseLkkTester
	{
		private readonly IDictionary<MappingInfo, MappingInfo> _mappings = new Dictionary<MappingInfo, MappingInfo> {
                                                                              {new MappingInfo("123", "Board 1"), new MappingInfo("S:123", "Project 1")},
                                                                              {new MappingInfo("456", "Board 2"), new MappingInfo("S:456", "Project 2")}
                                                                          };

		[Test]
		public void AllProjectsExist()
		{
			var validator = new V1ProjectsValidator(_mappings.Values) { Logger = LoggerMock, V1Processor = V1ProcessorMock };

			Expect.Call(V1ProcessorMock.ProjectExists("S:123")).Return(true);
			Expect.Call(V1ProcessorMock.ProjectExists("S:456")).Return(true);

			Repository.ReplayAll();
			var result = validator.Validate();
			Repository.VerifyAll();

			Assert.IsTrue(result, "Not all board exist.");
		}

		[Test]
		public void ProjectsDoNotExist()
		{
			var validator = new V1ProjectsValidator(_mappings.Values) { Logger = LoggerMock, V1Processor = V1ProcessorMock };

			Expect.Call(V1ProcessorMock.ProjectExists("S:123")).Return(false);
			Expect.Call(V1ProcessorMock.ProjectExists("S:456")).Return(false);

			Repository.ReplayAll();
			var result = validator.Validate();
			Repository.VerifyAll();

			Assert.IsFalse(result, "All project exist.");
		}

		[Test]
		public void OneProjectDoesNotExist()
		{
			var validator = new V1ProjectsValidator(_mappings.Values) { Logger = LoggerMock, V1Processor = V1ProcessorMock };

			Expect.Call(V1ProcessorMock.ProjectExists("S:123")).Return(true);
			Expect.Call(V1ProcessorMock.ProjectExists("S:456")).Return(false);

			Repository.ReplayAll();
			var result = validator.Validate();
			Repository.VerifyAll();

			Assert.IsFalse(result, "Incorrect projects status.");
		}
	}
}