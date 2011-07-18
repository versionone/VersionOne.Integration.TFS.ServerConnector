﻿using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector {
    public class ValueId {
        internal readonly Oid Oid;
        private readonly string name;

        public ValueId() : this(Oid.Null, string.Empty) { }

        public ValueId(Oid oid, string name) {
            Oid = oid.Momentless;
            this.name = name;
        }

        public override string ToString() {
            return name;
        }

        public bool Equals(ValueId obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            return Equals(obj.Oid, Oid);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != typeof(ValueId)) {
                return false;
            }

            return Equals((ValueId)obj);
        }

        public override int GetHashCode() {
            return Oid.GetHashCode();
        }
    }
}
