using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    public class Member : Entity {
        public const string NameProperty = "Name";
        public const string EmailProperty = "Email";

        public Member(){}
        public Member(Asset asset) : base(asset) {}

        public string Name {
            get { return GetProperty<string>(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }

        public string Email {
            get { return GetProperty<string>(EmailProperty); }
            set { SetProperty(EmailProperty, value); }
        }
    }
}