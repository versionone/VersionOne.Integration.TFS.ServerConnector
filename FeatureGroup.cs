using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector {
    public class FeatureGroup : Workitem {

        public IList<Workitem> Children {
            get; protected set;
        }

        protected FeatureGroup() { }
        public FeatureGroup(Asset asset) : base(asset) { }

        internal FeatureGroup(Asset asset, IDictionary<string, PropertyValues> listValues) : base(asset, listValues) { }

        internal FeatureGroup(Asset asset, IDictionary<string, PropertyValues> listValues, IList<Workitem> children) : base(asset, listValues) {
            Children = children;
        }
    }
}