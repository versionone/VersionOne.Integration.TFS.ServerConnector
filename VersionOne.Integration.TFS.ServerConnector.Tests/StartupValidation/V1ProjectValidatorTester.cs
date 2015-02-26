using NUnit.Framework;
using Rhino.Mocks;
using VersionOne.Integration.TFS.ServerConnector.StartupValidation;
using VersionOne.ServiceHost.Core.StartupValidation;

namespace VersionOne.Integration.TFS.ServerConnector.Tests.StartupValidation
{
	[TestFixture]
	public class V1ProjectValidatorTester : BaseValidationTester
	{
		private ISimpleValidator _validator;
		private const string ProjectToken = "Scope:-1";

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			_validator = new V1ProjectValidator(ProjectToken) { Logger = LoggerMock, V1Processor = V1ProcessorMock };
		}

		[Test]
		public void ProjectExists()
		{


			Expect.Call(V1ProcessorMock.ProjectExists(string.Empty)).IgnoreArguments().Constraints(Rhino.Mocks.Constraints.Is.Equal(ProjectToken)).Return(true);

			Repository.ReplayAll();
			var result = _validator.Validate();
			Repository.VerifyAll();

			Assert.IsTrue(result);
		}

		[Test]
		public void ProjectDoesNotExist()
		{
			Expect.Call(V1ProcessorMock.ProjectExists(string.Empty)).IgnoreArguments().Constraints(Rhino.Mocks.Constraints.Is.Equal(ProjectToken)).Return(false);

			Repository.ReplayAll();
			var result = _validator.Validate();
			Repository.VerifyAll();

			Assert.IsFalse(result);
		}
	}
}