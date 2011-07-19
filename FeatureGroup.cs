using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector {
    public class FeatureGroup : Workitem {
        internal FeatureGroup(Asset asset) : base(asset) { }

        internal FeatureGroup(Asset asset, IDictionary<string, PropertyValues> listValues) : base(asset, listValues) { }
    }
}