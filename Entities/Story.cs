using System.Collections.Generic;
using System.Diagnostics;
using VersionOne.SDK.APIClient;


namespace VersionOne.ServerConnector.Entities {
    [DebuggerDisplay("{TypeName} {Name}, Id={Id}, Number={Number}")]
    public class Story : PrimaryWorkitem {
        public const string BenefitsProperty = "Benefits";

        internal Story(Asset asset, IDictionary<string, PropertyValues> listValues)
            : base(asset, listValues) {
        }

        protected Story() { }

        public string Benefits {
            get { return GetProperty<string>(BenefitsProperty); }
            set { SetProperty(BenefitsProperty, value); }
        }
    }
}