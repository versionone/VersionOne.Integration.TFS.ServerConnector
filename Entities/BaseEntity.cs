using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    public abstract class BaseEntity {
        internal readonly Asset Asset;

        public virtual string TypeToken { get { return Asset.AssetType.Token; } }

        internal BaseEntity(Asset asset) {
            Asset = asset;
        }

        protected BaseEntity() { }

        protected internal virtual T GetProperty<T>(string name) {
            var attributeDefinition = Asset.AssetType.GetAttributeDefinition(name);
            return (T) (Asset.GetAttribute(attributeDefinition) != null ? Asset.GetAttribute(attributeDefinition).Value : null);
        }

        protected virtual void SetProperty<T>(string name, T value) {
            var attributeDefinition = Asset.AssetType.GetAttributeDefinition(name);
            Asset.SetAttributeValue(attributeDefinition, value);
        }
    }
}