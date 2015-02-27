using System;
using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.Integration.Tfs.ServerConnector.Tests
{
	public class AssetDisposer : IDisposable
	{
		private readonly Stack<Asset> _assets = new Stack<Asset>();
		private readonly IServices _services;

		public delegate Asset CreateAssetOperation();

		public AssetDisposer(IServices services)
		{
			_services = services;
		}

		public Asset CreateAndRegisterForDisposal(CreateAssetOperation createOperation)
		{
			var asset = createOperation.Invoke();
			_assets.Push(asset);
			return asset;
		}

		public void Dispose()
		{
			while (_assets.Count > 0)
			{
				var asset = _assets.Pop();
				DeleteAsset(asset);
			}
		}

		private void DeleteAsset(Asset subject)
		{
			if (subject == null)
			{
				return;
			}

			var operation = subject.AssetType.GetOperation(VersionOneProcessor.DeleteOperation);
			_services.ExecuteOperation(operation, subject.Oid);
		}
	}
}