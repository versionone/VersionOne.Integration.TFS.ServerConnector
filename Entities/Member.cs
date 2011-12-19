using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    public class Member : Entity {
        public const string EmailProperty = "Email";
        public const string DefaultRoleNameProperty = "DefaultRole.Name";

        public Member() { }

        public Member(Asset asset) : base(asset, null) {}

        public string Name {
            get { return GetProperty<string>(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }

        public string Email {
            get { return GetProperty<string>(EmailProperty); }
            set { SetProperty(EmailProperty, value); }
        }

        public string DefaultRole {
            get { return GetProperty<string>(DefaultRoleNameProperty); }
        }
    }
}