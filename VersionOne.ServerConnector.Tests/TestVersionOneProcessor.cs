using System.Xml;
using VersionOne.SDK.APIClient;
using VersionOne.ServiceHost.Core.Logging;

namespace VersionOne.ServerConnector.Tests
{
	public class TestVersionOneProcessor : VersionOneProcessor
	{
		public TestVersionOneProcessor(XmlElement config, ILogger logger) : base(config, logger) { }

		internal void ConnectTest(IServices testServices, IMetaModel testMetaData, IQueryBuilder testQueryBuilder)
		{
			Connect(testServices, testMetaData, testQueryBuilder);
		}
	}
}