using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    public class Scope : Entity {
        internal Scope(Asset asset) : base(asset, null) {}

        public string Name {
            get { return GetProperty<string>(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }
    }
}