using VersionOne.SDK.APIClient;

﻿namespace VersionOne.Integration.TFS.ServerConnector.Entities {
    public class BuildProject : Entity {
        public override string TypeToken {
            get { return VersionOneProcessor.BuildProjectType; }
        }

        internal BuildProject(Asset asset) : base(asset, null) { }
    }
}