using System.Collections.Generic;
using System.Diagnostics;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    [DebuggerDisplay("{TypeName} {Name}, Id={Id}")]
    // TODO resolve TypeName and TypeToken clashes, if any
    public abstract class Entity : BaseEntity {
        public const string NameAttribute = "Name";
        public const string InactiveAttribute = "Inactive";

        public const string CustomPrefix = "Custom_";

        public string Id { get; protected set; }
        public string TypeName { get; protected set; }

        protected IDictionary<string, PropertyValues> ListValues { get; set; }
        protected IEntityFieldTypeResolver TypeResolver;

        internal Entity(Asset asset, IEntityFieldTypeResolver typeResolver) : base(asset) {
            Id = asset.Oid.ToString();
            TypeName = asset.AssetType.Token;
            TypeResolver = typeResolver;
        }

        protected Entity() { }

        public virtual bool HasChanged() {
            return Asset.HasChanged;
        }

        public string GetCustomFieldValue(string fieldName) {
            fieldName = NormalizeCustomFieldName(fieldName);
            var value = GetProperty<object>(fieldName);

            if (value != null && value is Oid && ((Oid)value).IsNull) {
                return null;
            }

            return value != null ? value.ToString() : null;
        }

        public ValueId GetCustomListValue(string fieldName) {
            fieldName = NormalizeCustomFieldName(fieldName);
            var value = GetProperty<Oid>(fieldName);
            var type = TypeResolver.Resolve(TypeToken, fieldName);
            return ListValues[type].Find(value.Token);
        }

        public void SetCustomListValue(string fieldName, string value) {
            var type = TypeResolver.Resolve(TypeToken, fieldName);
            var valueData = ListValues[type].Find(value);

            if (valueData != null) {
                SetProperty(fieldName, valueData.Oid);
            }
        }

        private static string NormalizeCustomFieldName(string fieldName) {
            return fieldName.StartsWith(CustomPrefix) ? fieldName : CustomPrefix + fieldName;
        }

        public void SetCustomNumericValue(string fieldName, double value) {
            SetProperty(fieldName, value);
        }

        public double? GetCustomNumericValue(string fieldName) {
            return GetProperty<double?>(fieldName);
        }
    }
}