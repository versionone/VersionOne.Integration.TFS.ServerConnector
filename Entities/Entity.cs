using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    [DebuggerDisplay("{TypeName} {Name}, Id={Id}")]
    public class Entity {
        internal readonly Asset Asset;

        public string Id { get; protected set; }
        public string TypeName { get; protected set; }

        protected IDictionary<string, PropertyValues> ListValues { get; set; }

        internal Entity(Asset asset) {
            Asset = asset;
            Id = asset.Oid.ToString();
            TypeName = asset.AssetType.Token;
        }

        protected Entity() { }

        public string GetCustomFieldValue(string fieldName) {
            var fullFieldName = fieldName;

            if (!fullFieldName.StartsWith("Custom_")) {
                fullFieldName = "Custom_" + fullFieldName;
            }
            var value = GetProperty<object>(fullFieldName);

            if (value != null && value is Oid && ((Oid)value).IsNull) {
                return null;
            }
            return value != null ? value.ToString() : null;
        }        

        public void SetCustomListValue(string fieldName, string type, string value) {
            var valueData = ListValues[type].Find(value);

            if (valueData != null) {
                SetProperty(fieldName, valueData.Oid);
            }
        }

        protected virtual T GetProperty<T>(string name) {
            var attributeDefinition = Asset.AssetType.GetAttributeDefinition(name);
            return (T)Asset.GetAttribute(attributeDefinition).Value;
        }

        protected virtual IList<T> GetProperties<T>(string name) {
            var attributeDefinition = Asset.AssetType.GetAttributeDefinition(name);
            return Asset.GetAttribute(attributeDefinition).ValuesList.Cast<T>().ToList();
        }

        protected virtual void SetProperty<T>(string name, T value) {
            var attributeDefinition = Asset.AssetType.GetAttributeDefinition(name);
            Asset.SetAttributeValue(attributeDefinition, value);
        }
    }
}