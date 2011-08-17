using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    [DebuggerDisplay("{TypeName} {Name}, Id={Id}")]
    public class Entity : BaseEntity {
        public string Id { get; protected set; }
        public string TypeName { get; protected set; }

        protected IDictionary<string, PropertyValues> ListValues { get; set; }

        internal Entity(Asset asset) : base(asset) {
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
    }
}