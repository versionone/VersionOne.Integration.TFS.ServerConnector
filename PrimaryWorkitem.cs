using System.Collections.Generic;
using System.Diagnostics;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector {
    [DebuggerDisplay("{TypeName} {Name}, Id={Id}, Number={Number}")]
    public class PrimaryWorkitem {
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

        public IDictionary<string, Oid> Priorities { private get; set; }

        // TODO resolve
        //[LkkTag(Prefix = "VersionOne:Feature:")]
        public string FeatureGroupName { get { return GetProperty<string>(ParentNameProperty); } }

        // TODO resolve
        //[LkkTag(Prefix = "VersionOne:Team:")]
        public string Team { get { return GetProperty<string>(TeamNameProperty); } }

        // TODO resolve
        //[LkkTag(Prefix = "VersionOne:Sprint:")]
        public string SprintName { get { return GetProperty<string>(SprintNameProperty); } }

        public string Name {
            get { return GetProperty<string>(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }

        public string Description {
            get { return GetProperty<string>(DescriptionProperty); }
            set { SetProperty(DescriptionProperty, value); }
        }

        public int Order {
            get {
                int order;
                int.TryParse(GetProperty<Rank>(OrderProperty).ToString(), out order);
                return order;
            }
        }

        protected PrimaryWorkitem() { }

        internal PrimaryWorkitem(Asset asset, IDictionary<string, Oid> dictionary) {
            Asset = asset;
            Id = asset.Oid.ToString();
            TypeName = asset.AssetType.Token;
            Priorities = dictionary;
        }

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