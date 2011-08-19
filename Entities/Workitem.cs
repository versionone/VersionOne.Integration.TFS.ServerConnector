using System.Collections.Generic;
using System.Diagnostics;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    [DebuggerDisplay("{TypeName} {Name}, Id={Id}, Number={Number}")]
    public class Workitem : Entity {
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
        public const string ReferenceProperty = "Reference";
        public const string OwnersProperty = "Owners";

        public const string PriorityList = "WorkitemPriority";

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

        public string Reference {
            get { return GetProperty<string>(ReferenceProperty); }
            set { SetProperty(ReferenceProperty, value); }
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

        public IList<Member> Owners { get; protected set; }

        internal Workitem(Asset asset, IDictionary<string, PropertyValues> listValues, IList<Member> owners) 
            : this(asset, listValues) {
            Owners = owners;
        }

        internal Workitem(Asset asset, IDictionary<string, PropertyValues> listValues) : this(asset) {
            ListValues = listValues;
        }

        private Workitem(Asset asset) : base(asset) {}

        protected Workitem() { }
    }
}