using NUnit.Framework;
using Rhino.Mocks;
using VersionOne.ServerConnector.Entities;
using VersionOne.ServerConnector.StartupValidation;
using VersionOne.ServiceHost.Core.StartupValidation;

namespace VersionOne.ServerConnector.Tests.StartupValidation
{
	[TestFixture]
	public class V1CustomListFieldValidatorTester : BaseValidationTester
	{
		private ISimpleValidator _validator;

		private const string CustomFieldName = "Custom_BaFStatus1";

		private const string ReadyStatusToken = "Custom_BaF_status:1242";
		private const string PortedStatusToken = "Custom_BaF_status:1243";

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			_validator = new V1CustomListFieldValidator(CustomFieldName, VersionOneProcessor.FeatureGroupType) { Logger = LoggerMock, V1Processor = V1ProcessorMock };
		}

		[Test]
		public void AttributeDoesNotExist()
		{
			Expect.Call(V1ProcessorMock.AttributeExists(VersionOneProcessor.FeatureGroupType, CustomFieldName)).Return(false);

			Repository.ReplayAll();
			var result = _validator.Validate();
			Repository.VerifyAll();

			Assert.IsFalse(result);
		}

		[Test]
		public void FailureToEnlistAvailableValues()
		{
			_validator = new V1CustomListFieldValidator(CustomFieldName, VersionOneProcessor.FeatureGroupType, ReadyStatusToken, PortedStatusToken) { Logger = LoggerMock, V1Processor = V1ProcessorMock };

			Expect.Call(V1ProcessorMock.AttributeExists(VersionOneProcessor.FeatureGroupType, CustomFieldName)).Return(true);
			Expect.Call(V1ProcessorMock.GetAvailableListValues(VersionOneProcessor.FeatureGroupType, CustomFieldName)).Throw(new VersionOneException(null));

			Repository.ReplayAll();
			var result = _validator.Validate();
			Repository.VerifyAll();

			Assert.IsFalse(result);
		}

		[Test]
		public void CustomListValuesMissing()
		{
			_validator = new V1CustomListFieldValidator(CustomFieldName, VersionOneProcessor.FeatureGroupType, ReadyStatusToken, PortedStatusToken) { Logger = LoggerMock, V1Processor = V1ProcessorMock };
			var propertyValues = Repository.PartialMock<PropertyValues>();

			Expect.Call(V1ProcessorMock.AttributeExists(VersionOneProcessor.FeatureGroupType, CustomFieldName)).Return(true);
			Expect.Call(V1ProcessorMock.GetAvailableListValues(VersionOneProcessor.FeatureGroupType, CustomFieldName)).Return(propertyValues);
			Expect.Call(propertyValues.Find(ReadyStatusToken)).Return(new ValueId());
			Expect.Call(propertyValues.Find(PortedStatusToken)).Return(null);

			Repository.ReplayAll();
			var result = _validator.Validate();
			Repository.VerifyAll();

			Assert.IsFalse(result);
		}

		[Test]
		public void ValidationSuccessfulWithoutListValues()
		{
			Expect.Call(V1ProcessorMock.AttributeExists(VersionOneProcessor.FeatureGroupType, CustomFieldName)).Return(true);
			Expect.Call(V1ProcessorMock.GetAvailableListValues(VersionOneProcessor.FeatureGroupType, CustomFieldName)).Return(new PropertyValues());

			Repository.ReplayAll();
			var result = _validator.Validate();
			Repository.VerifyAll();

			Assert.IsTrue(result);
		}

		[Test]
		public void ValidationSuccessfulWithListValues()
		{
			_validator = new V1CustomListFieldValidator(CustomFieldName, VersionOneProcessor.FeatureGroupType, ReadyStatusToken, PortedStatusToken) { Logger = LoggerMock, V1Processor = V1ProcessorMock };
			var propertyValues = Repository.PartialMock<PropertyValues>();

			Expect.Call(V1ProcessorMock.AttributeExists(VersionOneProcessor.FeatureGroupType, CustomFieldName)).Return(true);
			Expect.Call(V1ProcessorMock.GetAvailableListValues(VersionOneProcessor.FeatureGroupType, CustomFieldName)).Return(propertyValues);
			Expect.Call(propertyValues.Find(ReadyStatusToken)).Return(new ValueId());
			Expect.Call(propertyValues.Find(PortedStatusToken)).Return(new ValueId());

			Repository.ReplayAll();
			var result = _validator.Validate();
			Repository.VerifyAll();

			Assert.IsTrue(result);
		}
	}
}