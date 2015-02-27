using VersionOne.SDK.APIClient;

﻿namespace VersionOne.Integration.Tfs.ServerConnector.Entities {
    public class Scope : Entity {
        internal Scope(Asset asset) : base(asset, null) {}
    }
}