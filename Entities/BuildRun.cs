using System;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    public class BuildRun : Entity {
        public const string ElapsedProperty = "Elapsed";
        public const string DateProperty = "Date";
        public const string BuildProjectProperty = "BuildProject";

        public override string TypeToken {
            get { return VersionOneProcessor.BuildRunType; }
        }

        internal BuildRun(Asset asset) : base(asset, null) { }

        public ValueId Status {
            get { return GetListValue(StatusProperty); }
            set { SetCustomListValue(StatusProperty, value.Token); }
        }

        public double? Elapsed {
            get { return GetProperty<double?>(ElapsedProperty); }
            set { SetProperty(ElapsedProperty, value); }
        }

        public DateTime Date {
            get { return GetProperty<DateTime>(DateProperty); }
            set { SetProperty(DateProperty, value); }
        }

        // TODO BuildProject
        // TODO ChangeSets
    }
}