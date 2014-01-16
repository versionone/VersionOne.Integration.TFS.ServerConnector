using System.Collections.Generic;
using VersionOne.ServerConnector.Entities;

namespace VersionOne.ServerConnector.Tests.TestEntity
{
	public class TestStory : Story
	{
		private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
		private bool _changed;

		public TestStory(string id, IDictionary<string, PropertyValues> listValues, IEntityFieldTypeResolver typeResolver)
		{
			Id = id;
			TypeName = VersionOneProcessor.StoryType;
			ListValues = listValues;
			TypeResolver = typeResolver;
		}

		public TestStory(string id, IEntityFieldTypeResolver typeResolver) : this(id, null, typeResolver) { }

		public TestStory Set(string propertyName, object value)
		{
			SetProperty(propertyName, value);
			_changed = false;
			return this;
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
			return _changed;
		}
	}
}