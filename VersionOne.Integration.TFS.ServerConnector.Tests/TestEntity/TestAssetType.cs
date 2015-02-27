using System;
using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.Integration.Tfs.ServerConnector.Tests.TestEntity
{
	public class TestAssetType : IAssetType
	{
		private readonly IDictionary<string, IAttributeDefinition> _attributeDifinitions;

		public TestAssetType(string typeName)
			: this(typeName, new Dictionary<string, IAttributeDefinition>())
		{
		}

		public TestAssetType(string typeName, IDictionary<string, IAttributeDefinition> attributeDifinitions)
		{
			Token = typeName;
			_attributeDifinitions = attributeDifinitions;
		}

		public bool Is(IAssetType targettype)
		{
			return Token.Equals(targettype.Token);
		}

		public IAttributeDefinition GetAttributeDefinition(string name)
		{
			return _attributeDifinitions.ContainsKey(name) ? _attributeDifinitions[name] : new TestAttributeDefinition(this);
		}

		public bool TryGetAttributeDefinition(string name, out IAttributeDefinition def)
		{
			throw new NotImplementedException();
		}

		public IOperation GetOperation(string name)
		{
			throw new NotImplementedException();
		}

		public bool TryGetOperation(string name, out IOperation op)
		{
			throw new NotImplementedException();
		}

		public string Token { get; set; }

		public IAssetType Base
		{
			get { throw new NotImplementedException(); }
		}

		public string DisplayName
		{
			get { throw new NotImplementedException(); }
		}

		public IAttributeDefinition DefaultOrderBy
		{
			get { throw new NotImplementedException(); }
		}

		public IAttributeDefinition ShortNameAttribute
		{
			get { throw new NotImplementedException(); }
		}

		public IAttributeDefinition NameAttribute
		{
			get { throw new NotImplementedException(); }
		}

		public IAttributeDefinition DescriptionAttribute
		{
			get { throw new NotImplementedException(); }
		}
	}
}