using System;
using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    public class PrimaryWorkitem : Workitem {        
        public const string TeamNameProperty = "Team.Name";
        public const string ParentNameProperty = "Parent.Name";
        public const string SprintNameProperty = "Timebox.Name";
        public const string OrderProperty = "Order";
        public const string CompletedInBuildRunsProperty = "CompletedInBuildRuns";

        public string FeatureGroupName {
            get { return GetProperty<string>(ParentNameProperty); }
        }

        public string Team {
            get { return GetProperty<string>(TeamNameProperty); }
        }

        public string SprintName {
            get { return GetProperty<string>(SprintNameProperty); }
        }

        public int Order {
            get {
                int order;
                Int32.TryParse(GetProperty<Rank>(OrderProperty).ToString(), out order);
                return order;
            }
        }

        public ValueId Status {
            get { return GetListValue(StatusProperty); }
            set { SetCustomListValue(StatusProperty, value.Token); }
        }

        public override string TypeToken {
            get { return VersionOneProcessor.PrimaryWorkitemType; }
        }

        internal protected PrimaryWorkitem(Asset asset, IDictionary<string, PropertyValues> listValues, IEntityFieldTypeResolver typeResolver) : base(asset, listValues, typeResolver) { }

        internal protected PrimaryWorkitem() { }
    }
}