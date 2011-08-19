using System.Collections.Generic;
using System.Linq;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    public class BaseEntity {
        internal readonly Asset Asset;

        internal BaseEntity(Asset asset) {
            Asset = asset;
        }

        protected BaseEntity() { }

        protected virtual T GetProperty<T>(string name) {
            var attributeDefinition = Asset.AssetType.GetAttributeDefinition(name);
            return (T)Asset.GetAttribute(attributeDefinition).Value;
        }

        protected virtual void SetProperty<T>(string name, T value) {
            var attributeDefinition = Asset.AssetType.GetAttributeDefinition(name);
            Asset.SetAttributeValue(attributeDefinition, value);
        }
    }
}