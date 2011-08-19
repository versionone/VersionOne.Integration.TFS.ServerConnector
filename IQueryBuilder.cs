using System.Collections.Generic;
using VersionOne.SDK.APIClient;
using VersionOne.ServerConnector.Entities;

namespace VersionOne.ServerConnector {
    // TODO refactor APIClient types
    internal interface IQueryBuilder {
        IDictionary<string, PropertyValues> ListPropertyValues { get; }

        void Setup(IServices services, IMetaModel metaModel);
        void AddProperty(string attr, string prefix, bool isList);
        PropertyValues QueryPropertyValues(string propertyName);
        AssetList Query(string typeToken, IFilterTerm filter);
    }
}