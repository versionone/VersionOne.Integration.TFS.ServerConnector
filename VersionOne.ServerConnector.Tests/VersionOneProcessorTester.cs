using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VersionOne.ServerConnector.Entities;
using VersionOne.ServerConnector.Filters;
using VersionOne.ServiceHost.Core.Configuration;

namespace VersionOne.ServerConnector.Tests
{
	[Ignore]
	[TestClass]
	public class VersionOneProcessorTester
	{
		private VersionOneProcessor _versionOneProcessor;

		[TestInitialize]
		public void Init()
		{
			var settings = new VersionOneSettings()
			{
				Username = "admin",
				Password = "admin",
				Url = "https://www14.v1host.com/v1sdktesting/",
				IntegratedAuth = false,
				ProxySettings = null
			};

			_versionOneProcessor = new VersionOneProcessor(settings);
			_versionOneProcessor.ValidateConnection();
		}

		[TestMethod]
		public void get_logged_in_member()
		{
			Member member = _versionOneProcessor.GetLoggedInMember();
		}

		[TestMethod]
		public void get_work_items_with_filter_id()
		{
			IList<Workitem> workitems = _versionOneProcessor
				.GetWorkitems("Story", Filter.Equal("ID", "Story:1071"));
		}
	}
}
