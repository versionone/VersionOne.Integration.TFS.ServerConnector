using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector {
    public class ValueId {
        internal readonly Oid Oid;
        private readonly string name;

        public ValueId() : this(Oid.Null, string.Empty) { }

        internal ValueId(Oid oid, string name) {
            Oid = oid.Momentless;
            this.name = name;
        }

        public override string ToString() {
            return name;
        }

        public override bool Equals(object obj) {
            if(obj == null || obj.GetType() != typeof(ValueId)) {
                return false;
            }

            if(ReferenceEquals(this, obj)) {
                return true;
            }

            var other = (ValueId) obj;
            return Equals(Oid, other.Oid);
        }

        public override int GetHashCode() {
            return Oid.GetHashCode();
        }
    }
}