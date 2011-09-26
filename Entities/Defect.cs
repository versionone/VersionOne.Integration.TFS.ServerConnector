using System.Collections.Generic;
using System.Diagnostics;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    [DebuggerDisplay("{TypeName} {Name}, Id={Id}, Number={Number}")]
    public class Defect : Workitem {
        public override string TypeToken {
            get { return VersionOneProcessor.DefectType; }
        }

        protected internal Defect(Asset asset, IDictionary<string, PropertyValues> listValues, IEntityFieldTypeResolver typeResolver) : base(asset, listValues, typeResolver) { }
    }
}