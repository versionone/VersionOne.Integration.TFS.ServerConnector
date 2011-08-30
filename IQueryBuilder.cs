using System.Collections.Generic;
using VersionOne.SDK.APIClient;
using VersionOne.ServerConnector.Entities;

namespace VersionOne.ServerConnector {
    // TODO refactor APIClient types
    internal interface IQueryBuilder {
        IDictionary<string, PropertyValues> ListPropertyValues { get; }
        IEntityFieldTypeResolver TypeResolver { get; }

        void Setup(IServices services, IMetaModel metaModel);
        void AddProperty(string attr, string prefix, bool isList);
        void AddListProperty(string fieldName, string typeToken);
        void AddOptionalProperty(string attr, string prefix);

        PropertyValues QueryPropertyValues(string propertyName);
        AssetList Query(string typeToken, IFilterTerm filter);
    }
}