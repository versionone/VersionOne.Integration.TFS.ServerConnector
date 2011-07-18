using System.Collections.Generic;
using System.Diagnostics;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector {
    [DebuggerDisplay("{TypeName} {Name}, Id={Id}, Number={Number}")]
    // TODO decide on hierarchy
    public class Workitem {
        public const string AssetTypeProperty = "AssetType";
        public const string NumberProperty = "Number";
        public const string StatusProperty = "Status.Name";
        public const string EstimateProperty = "Estimate";
        public const string PriorityProperty = "Priority";
        public const string ParentNameProperty = "Parent.Name";
        public const string TeamNameProperty = "Team.Name";
        public const string SprintNameProperty = "Timebox.Name";
        public const string NameProperty = "Name";
        public const string DescriptionProperty = "Description";
        public const string OrderProperty = "Order";
        
        internal readonly Asset Asset;

        public string Id { get; protected set; }
        public string TypeName { get; protected set; }

        public string Number { get { return GetProperty<string>(NumberProperty); } }
        public string Status { get { return GetProperty<string>(StatusProperty); } }
        
        public string Name {
            get { return GetProperty<string>(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }

        public string Description {
            get { return GetProperty<string>(DescriptionProperty); }
            set { SetProperty(DescriptionProperty, value); }
        }

        public double? Estimate {
            get { return GetProperty<double?>(EstimateProperty); }
            set { SetProperty(EstimateProperty, value);}
        }

        public string PriorityToken {
            get { return GetProperty<Oid>(PriorityProperty).Momentless.Token; }
            set {
                SetProperty(PriorityProperty, Priorities[value]);
            }
        }

        // TODO get rid of this
        protected IDictionary<string, Oid> Priorities { get; set; }

        internal Workitem(Asset asset, IDictionary<string, Oid> priorities) : this(asset) {
            Priorities = priorities;
        }

        internal Workitem(Asset asset) {
            Asset = asset;
            Id = asset.Oid.ToString();
            TypeName = asset.AssetType.Token;
        }

        protected Workitem() { }

        protected virtual T GetProperty<T>(string name) {
            var attributeDefinition = Asset.AssetType.GetAttributeDefinition(name);
            return (T) Asset.GetAttribute(attributeDefinition).Value;
        }

        protected virtual void SetProperty<T>(string name, T value) {
            var attributeDefinition = Asset.AssetType.GetAttributeDefinition(name);
            Asset.SetAttributeValue(attributeDefinition, value);
        }
    }
}