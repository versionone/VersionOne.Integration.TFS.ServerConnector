using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Tests.TestEntity
{
	public class TestAttributeDefinition : IAttributeDefinition
	{
		private readonly IAssetType _assetType;
		private readonly bool _isMultiValue;
		private readonly bool _isReadOnly;
		private readonly bool _isRequired;

		public TestAttributeDefinition(IAssetType assetType)
			: this(assetType, false, false, false)
		{
		}

		public TestAttributeDefinition(IAssetType assetType, bool isMultiValue, bool isReadOnly, bool isRequired)
		{
			_assetType = assetType;
			_isMultiValue = isMultiValue;
			_isReadOnly = isReadOnly;
			_isRequired = isRequired;
		}

		public IAttributeDefinition Aggregate(Aggregate aggregate)
		{
			throw new System.NotImplementedException();
		}

		public IAssetType AssetType
		{
			get { return _assetType; }
		}

		public AttributeType AttributeType
		{
			get { return AttributeType.Text; }
		}

		public IAttributeDefinition Base
		{
			get { throw new System.NotImplementedException(); }
		}

		public object Coerce(object value)
		{
			return value;
		}

		public string DisplayName
		{
			get { throw new System.NotImplementedException(); }
		}

		public IAttributeDefinition Downcast(IAssetType assetType)
		{
			throw new System.NotImplementedException();
		}

		public IAttributeDefinition Filter(IFilterTerm filter)
		{
			throw new System.NotImplementedException();
		}

		public bool IsMultiValue
		{
			get { return _isMultiValue; }
		}

		public bool IsReadOnly
		{
			get { return _isReadOnly; }
		}

		public bool IsRequired
		{
			get { return _isRequired; }
		}

		public IAttributeDefinition Join(IAttributeDefinition joined)
		{
			throw new System.NotImplementedException();
		}

		public string Name
		{
			get { return _assetType.Token; }
		}

		public IAssetType RelatedAsset
		{
			get { throw new System.NotImplementedException(); }
		}

		public string Token
		{
			get { return _assetType.Token; }
		}
	}
}