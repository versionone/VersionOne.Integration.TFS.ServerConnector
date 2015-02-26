using NUnit.Framework;
using Rhino.Mocks;
using VersionOne.ServiceHost.Core;
using VersionOne.ServiceHost.Core.Logging;

namespace VersionOne.Integration.TFS.ServerConnector.Tests.StartupValidation
{
	[TestFixture]
	public class BaseValidationTester
	{
		protected readonly MockRepository Repository = new MockRepository();
		protected ILogger LoggerMock;
		protected IVersionOneProcessor V1ProcessorMock;
		protected IEntityFieldTypeResolver TypeResolverMock;

		[SetUp]
		public virtual void SetUp()
		{
			LoggerMock = Repository.Stub<ILogger>();
			V1ProcessorMock = Repository.StrictMock<IVersionOneProcessor>();
			TypeResolverMock = Repository.StrictMock<IEntityFieldTypeResolver>();

			ComponentRepository.Instance.Register(LoggerMock);
			ComponentRepository.Instance.Register(V1ProcessorMock);
		}

		[TearDown]
		public void TearDown()
		{
			Repository.BackToRecordAll();
		}
	}
}