using System.Collections.Generic;
using System.Linq;
using VersionOne.ServerConnector.Entities;

namespace VersionOne.ServerConnector.Tests.TestEntity
{
	internal class TestFeatureGroup : FeatureGroup
	{
		private const string FeatureGroupTypeName = "Theme";
		private bool _changed;

		private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();

		public TestFeatureGroup(string id, IDictionary<string, PropertyValues> listValues, IList<Workitem> children, IEntityFieldTypeResolver typeResolver)
			: this(id, listValues, typeResolver)
		{
			Children = children;
		}

		private TestFeatureGroup(string id, IDictionary<string, PropertyValues> listValues, IEntityFieldTypeResolver typeResolver)
		{
			Id = id;
			TypeName = FeatureGroupTypeName;
			ListValues = listValues;
			TypeResolver = typeResolver;
		}

		public TestFeatureGroup Set(string propertyName, object value)
		{
			SetProperty(propertyName, value);
			_changed = false;
			return this;
		}

		public new IList<Member> Owners
		{
			set { base.Owners = value; }
		}

		public override T GetProperty<T>(string name)
		{
			return _properties.ContainsKey(name) ? (T)_properties[name] : default(T);
		}

		protected override void SetProperty<T>(string name, T value)
		{
			if (!_properties.ContainsKey(name))
			{
				_properties.Add(name, value);
			}
			else
			{
				_properties[name] = value;
			}
			_changed = true;
		}

		public override bool HasChanged()
		{
			return _changed || Children.Any(x => x.HasChanged());
		}
	}
}