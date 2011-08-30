using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    public class WorkitemFromExternalSystem : Workitem {
        private readonly string externalIdFieldName;

        internal WorkitemFromExternalSystem(Asset asset, IDictionary<string, PropertyValues> listValues, string externalIdFieldName, IEntityFieldTypeResolver typeResolver)
            : base(asset, listValues, typeResolver) {
            this.externalIdFieldName = externalIdFieldName;
        }

        public string ExternalId {
            get { return GetProperty<string>(externalIdFieldName); }

        }
    }
}