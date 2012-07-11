using System;
using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector {
    internal class EntityFactory {
        private readonly IMetaModel metaModel;
        private readonly IServices services;

        internal EntityFactory(IMetaModel metaModel, IServices services) {
            this.metaModel = metaModel;
            this.services = services;
        }

        internal Asset Create(string assetTypeName, IEnumerable<AttributeValue> attributeValues) {
            var assetType = metaModel.GetAssetType(assetTypeName);
            var asset = services.New(assetType, Oid.Null);

            foreach (var attributeValue in attributeValues) {
                if(attributeValue is SingleAttributeValue) {
                    asset.SetAttributeValue(assetType.GetAttributeDefinition(attributeValue.Name), ((SingleAttributeValue)attributeValue).Value);
                } else if(attributeValue is MultipleAttributeValue) {
                    var values = ((MultipleAttributeValue) attributeValue).Values;
                    var attributeDefinition = assetType.GetAttributeDefinition(attributeValue.Name);

                    foreach (var value in values) {
                        asset.AddAttributeValue(attributeDefinition, value);
                    }

                } else {
                    throw new NotSupportedException("Unknown Attribute Value type.");
                }
            }

            services.Save(asset);
            return asset;
        }
    }
}