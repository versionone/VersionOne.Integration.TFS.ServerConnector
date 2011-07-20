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

        public const string PriorityList = "WorkitemPriority";
        
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
            get {
                var oid = GetProperty<Oid>(PriorityProperty);
                return oid.IsNull ? null : oid.Momentless.Token;
            }
            set {
                var priority = ListValues[PriorityList].Find(value);
                if (priority != null) {
                    SetProperty(PriorityProperty, priority.Oid);
                }
            }
        }

        //TODO just some thoughts maybe we can avoid using it
        public string GetCustomFieldValue(string fieldName) {
            var fullFieldName = fieldName;

            if (!fullFieldName.StartsWith("Custom_")) {
                fullFieldName = "Custom_" + fullFieldName;
            }
            
            var value = GetProperty<object>(fullFieldName);
            
            if (value != null && value is Oid && ((Oid)value).IsNull) {
                return null;
            }
            
            return value != null ? value.ToString() : null;
        }

        //TODO just some thoughts maybe we can avoid using it
        public void SetCustomListValue(string fieldName, string type, string value) {
            var valueData = ListValues[type].Find(value);
            
            if (valueData != null) {
                SetProperty(fieldName, valueData.Oid);
            }
        }

        protected IDictionary<string, PropertyValues> ListValues { get; set; }

        internal Workitem(Asset asset, IDictionary<string, PropertyValues> listValues) : this(asset) {
            ListValues = listValues;
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