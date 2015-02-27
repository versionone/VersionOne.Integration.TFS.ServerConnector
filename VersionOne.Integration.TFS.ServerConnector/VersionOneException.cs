using System;

namespace VersionOne.Integration.Tfs.ServerConnector {
    public class VersionOneException : Exception {
        public VersionOneException(string message) : base(message) { }
    }
}