using System.Linq;
using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    public class FeatureGroup : Workitem {
        public IList<Workitem> Children {get; protected set;}

        protected FeatureGroup() { }

        internal FeatureGroup(Asset asset, IDictionary<string, PropertyValues> listValues, IList<Workitem> children, IList<Member> owners)
                : base(asset, listValues, owners) {
            Children = children;
        }

        public override bool HasChanged() {
            return Asset.HasChanged || Children.Any(x => x.HasChanged());
        }
    }
}