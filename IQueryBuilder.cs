using System.Collections.Generic;
using VersionOne.SDK.APIClient;
using VersionOne.ServerConnector.Entities;

namespace VersionOne.ServerConnector {
    // TODO refactor APIClient types
    internal interface IQueryBuilder {
        IDictionary<string, PropertyValues> ListPropertyValues { get; }

        void Setup(IServices services, IMetaModel metaModel);
        void AddProperty(string attr, string prefix, bool isList);
        /// <summary>
        /// Add not list property which can be doesn't exist at start.
        /// </summary>
        /// <param name="attr">Attribute name</param>
        /// <param name="prefix">attribute type</param>
        void AddOptionalProperty(string attr, string prefix);

        PropertyValues QueryPropertyValues(string propertyName);
        AssetList Query(string typeToken, IFilterTerm filter);
    }
}