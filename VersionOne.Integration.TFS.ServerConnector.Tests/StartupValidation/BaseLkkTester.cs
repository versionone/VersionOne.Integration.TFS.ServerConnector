using NUnit.Framework;
using Rhino.Mocks;
using VersionOne.ServiceHost.Core.Logging;

namespace VersionOne.Integration.TFS.ServerConnector.Tests.StartupValidation
{
	[TestFixture]
	public class BaseLkkTester
	{
		protected readonly MockRepository Repository = new MockRepository();
		protected IVersionOneProcessor V1ProcessorMock;
		protected ILogger LoggerMock;

		[SetUp]
		public virtual void SetUp()
		{
			V1ProcessorMock = Repository.StrictMock<IVersionOneProcessor>();
			LoggerMock = Repository.Stub<ILogger>();
		}
	}
}