using System;
using NUnit.Framework;
using Rhino.Mocks;
using VersionOne.ServiceHost.Core.StartupValidation;

namespace VersionOne.Integration.TFS.ServerConnector.Tests.StartupValidation
{
	[TestFixture]
	public class ValidationStepTester : BaseValidationTester
	{
		private ISimpleValidator _validatorMock;
		private ISimpleResolver _resolverMock;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			_validatorMock = Repository.StrictMock<ISimpleValidator>();
			_resolverMock = Repository.StrictMock<ISimpleResolver>();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void EmptyValidatorFailure()
		{
			Expect.Call(_validatorMock.Validate()).Repeat.Never();
			Expect.Call(_resolverMock.Resolve()).Repeat.Never();

			Repository.ReplayAll();
			var step = new ValidationSimpleStep(null, _resolverMock);
			step.Run();
			Repository.VerifyAll();
		}

		[Test]
		[ExpectedException(typeof(ValidationException))]
		public void ValidationFailureEmptyResolver()
		{
			Expect.Call(_validatorMock.Validate()).Return(false);
			Expect.Call(_resolverMock.Resolve()).Repeat.Never();

			Repository.ReplayAll();
			var step = new ValidationSimpleStep(_validatorMock, null);
			step.Run();
			Repository.VerifyAll();
		}

		[Test]
		[ExpectedException(typeof(ValidationException))]
		public void ResolveFailure()
		{
			Expect.Call(_validatorMock.Validate()).Return(false);
			Expect.Call(_resolverMock.Resolve()).Return(false);

			Repository.ReplayAll();
			var step = new ValidationSimpleStep(_validatorMock, _resolverMock);
			step.Run();
			Repository.VerifyAll();
		}

		[Test]
		public void SuccessfulValidate()
		{
			Expect.Call(_validatorMock.Validate()).Return(true);
			Expect.Call(_resolverMock.Resolve()).Repeat.Never();

			Repository.ReplayAll();
			var step = new ValidationSimpleStep(_validatorMock, _resolverMock);
			step.Run();
			Repository.VerifyAll();
		}

		[Test]
		public void SuccessfulResolve()
		{
			Expect.Call(_validatorMock.Validate()).Return(false);
			Expect.Call(_resolverMock.Resolve()).Return(true);

			Repository.ReplayAll();
			var step = new ValidationSimpleStep(_validatorMock, _resolverMock);
			step.Run();
			Repository.VerifyAll();
		}
	}
}