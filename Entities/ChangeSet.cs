using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    public class ChangeSet : Entity {
        public override string TypeToken {
            get { return VersionOneProcessor.ChangeSetType; }
        }

        internal ChangeSet(Asset asset, IEntityFieldTypeResolver typeResolver) : base(asset, typeResolver) { }

        public string Reference {
            get { return GetProperty<string>(ReferenceProperty); }
            set { SetProperty(ReferenceProperty, value);}
        }

        public string Description {
            get { return GetProperty<string>(DescriptionProperty); }
            set { SetProperty(DescriptionProperty, value);}
        }

        // TODO PrimaryWorkitems
    }
}
