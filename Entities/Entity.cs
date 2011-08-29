using System.Collections.Generic;
using System.Diagnostics;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    [DebuggerDisplay("{TypeName} {Name}, Id={Id}")]
    public abstract class Entity : BaseEntity {
        public const string NameAttribute = "Name";
        public const string InactiveAttribute = "Inactive";

        public string Id { get; protected set; }
        public string TypeName { get; protected set; }

        protected internal IDictionary<string, PropertyValues> ListValues { get; set; }

        internal Entity(Asset asset) : base(asset) {
            Id = asset.Oid.ToString();
            TypeName = asset.AssetType.Token;
        }

        protected Entity() { }

        public virtual bool HasChanged() {
            return Asset.HasChanged;
        }

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

        public void SetCustomNumericValue(string fieldName, double value) {
            SetProperty(fieldName, value);
        }

        public double? GetCustomNumericValue(string fieldName) {
            return GetProperty<double?>(fieldName);
        }
    }
}