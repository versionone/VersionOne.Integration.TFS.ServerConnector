using System.Collections.Generic;
using System.Diagnostics;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Entities {
    [DebuggerDisplay("{TypeName} {Name}, Id={Id}, Number={Number}")]
    public class TestSet : PrimaryWorkitem {
        public override string TypeToken {
            get { return VersionOneProcessor.TestSetType; }
        }

        protected internal TestSet(Asset asset, IDictionary<string, PropertyValues> listValues, IEntityFieldTypeResolver typeResolver, IList<Member> owners = null) 
            : base(asset, listValues, typeResolver, owners) { }
    }
}