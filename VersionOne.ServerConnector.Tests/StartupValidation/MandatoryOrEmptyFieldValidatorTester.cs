using NUnit.Framework;
using Rhino.Mocks;
using VersionOne.ServerConnector.StartupValidation;

namespace VersionOne.ServerConnector.Tests.StartupValidation
{
	[TestFixture]
	public class MandatoryOrEmptyFieldValidatorTester : BaseValidationTester
	{
		private const string CustomFieldName = "Custom_WorkflowStatus";

		[Test]
		public void FieldValueIsOmitted()
		{
			var validator = new MandatoryOrEmptyFieldValidator(null, "LeanKit Kanban Workflow Status", VersionOneProcessor.PrimaryWorkitemType)
			{
				Logger = LoggerMock,
				V1Processor = V1ProcessorMock
			};
			Assert.IsTrue(validator.Validate());
		}

		[Test]
		public void ValidFieldName()
		{
			Expect.Call(V1ProcessorMock.AttributeExists(VersionOneProcessor.PrimaryWorkitemType, CustomFieldName)).Return(true);

			Repository.ReplayAll();
			var validator = new MandatoryOrEmptyFieldValidator(CustomFieldName, "LeanKit Kanban Workflow Status", VersionOneProcessor.PrimaryWorkitemType)
			{
				Logger = LoggerMock,
				V1Processor = V1ProcessorMock
			};
			Assert.IsTrue(validator.Validate());
			Repository.VerifyAll();
		}

		[Test]
		public void InvalidFieldName()
		{
			Expect.Call(V1ProcessorMock.AttributeExists(VersionOneProcessor.PrimaryWorkitemType, CustomFieldName)).Return(false);

			Repository.ReplayAll();
			var validator = new MandatoryOrEmptyFieldValidator(CustomFieldName, "LeanKit Kanban Workflow Status", VersionOneProcessor.PrimaryWorkitemType)
			{
				Logger = LoggerMock,
				V1Processor = V1ProcessorMock
			};
			Assert.IsFalse(validator.Validate());
			Repository.VerifyAll();
		}
	}
}