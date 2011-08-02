using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector {
    public class PrimaryWorkitem : Workitem {        
        public string FeatureGroupName { get { return GetProperty<string>(ParentNameProperty); } }
        public string Team { get { return GetProperty<string>(TeamNameProperty); } }
        public string SprintName { get { return GetProperty<string>(SprintNameProperty); } }

        public int Order {
            get {
                int order;
                int.TryParse(GetProperty<Rank>(OrderProperty).ToString(), out order);
                return order;
            }
        }

        internal PrimaryWorkitem(Asset asset, IDictionary<string, PropertyValues> listValues) : base(asset, listValues) { }

        protected PrimaryWorkitem() { }
    }
}