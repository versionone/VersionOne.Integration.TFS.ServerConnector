using System;

namespace VersionOne.Integration.TFS.ServerConnector {
    public class VersionOneException : Exception {
        public VersionOneException(string message) : base(message) { }
    }
}