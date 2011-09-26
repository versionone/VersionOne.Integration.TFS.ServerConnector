using System;
using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    public abstract class Workitem : Entity {
        public const string AssetTypeProperty = "AssetType";
        public const string NumberProperty = "Number";
        public const string EstimateProperty = "Estimate";
        public const string PriorityProperty = "Priority";
        public const string ParentNameProperty = "Parent.Name";
        public const string TeamNameProperty = "Team.Name";
        public const string SprintNameProperty = "Timebox.Name";
        public const string DescriptionProperty = "Description";
        public const string OrderProperty = "Order";
        public const string ReferenceProperty = "Reference";
        public const string OwnersProperty = "Owners";

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

        public DateTime ChangeDateUtc {
            get { return GetProperty<DateTime>(ChangeDateUtcProperty); }
            set { SetProperty(ChangeDateUtcProperty, value); }
        }
        
        public string PriorityToken {
            get {
                var oid = GetProperty<Oid>(PriorityProperty);
                return oid.IsNull ? null : oid.Momentless.Token;
            }
            set {
                var priority = ListValues[VersionOneProcessor.WorkitemPriorityType].Find(value);
                if (priority != null) {
                    SetProperty(PriorityProperty, priority.Oid);
                }
            }
        }

        public IList<Member> Owners { get; protected set; }

        internal Workitem(Asset asset, IDictionary<string, PropertyValues> listValues, IList<Member> owners, IEntityFieldTypeResolver typeResolver) 
                : this(asset, listValues, typeResolver) {
            Owners = owners;
        }

        internal Workitem(Asset asset, IDictionary<string, PropertyValues> listValues, IEntityFieldTypeResolver typeResolver) : this(asset, typeResolver) {
            ListValues = listValues;
        }

        private Workitem(Asset asset, IEntityFieldTypeResolver typeResolver) : base(asset, typeResolver) {}

        protected Workitem() { }

        internal static Workitem Create(Asset asset, IDictionary<string, PropertyValues> listPropertyValues, IEntityFieldTypeResolver typeResolver) {
            switch(asset.AssetType.Token) {
                case VersionOneProcessor.StoryType:
                    return new Story(asset, listPropertyValues, typeResolver);
                case VersionOneProcessor.DefectType:
                    return new Defect(asset, listPropertyValues, typeResolver);
                default:
                    throw new NotSupportedException("Type " + asset.AssetType.Token + " is not supported in factory method");
            }
        }
    }
}