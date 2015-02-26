using NUnit.Framework;
using Rhino.Mocks;
using VersionOne.Integration.TFS.ServerConnector.StartupValidation;
using VersionOne.ServiceHost.Core.StartupValidation;

namespace VersionOne.Integration.TFS.ServerConnector.Tests.StartupValidation
{
	[TestFixture]
	public class V1ConnectionValidatorTester : BaseValidationTester
	{
		private ISimpleValidator _validator;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			_validator = new V1ConnectionValidator { Logger = LoggerMock, V1Processor = V1ProcessorMock };
		}

		[Test]
		public void ConnectionIsValid()
		{
			Expect.Call(V1ProcessorMock.LogConnectionConfiguration);
			Expect.Call(V1ProcessorMock.ValidateConnection()).Return(true);
			Expect.Call(V1ProcessorMock.LogConnectionInformation);

			Repository.ReplayAll();
			var result = _validator.Validate();
			Repository.VerifyAll();

			Assert.IsTrue(result);
		}

		[Test]
		public void ConnectionIsNotValid()
		{
			Expect.Call(V1ProcessorMock.LogConnectionConfiguration);
			Expect.Call(V1ProcessorMock.ValidateConnection()).Return(false);

			Repository.ReplayAll();
			var result = _validator.Validate();
			Repository.VerifyAll();

			Assert.IsFalse(result);
		}
	}
}