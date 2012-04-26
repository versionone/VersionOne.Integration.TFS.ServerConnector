using Ninject;
using VersionOne.ServiceHost.Core.Logging;
using VersionOne.ServiceHost.Core.StartupValidation;

namespace VersionOne.ServerConnector.StartupValidation {
    public abstract class BaseValidator : ISimpleValidator {
        [Inject]
        public ILogger Logger { get; set; }

        [Inject]
        public IVersionOneProcessor V1Processor { get; set; }

        public abstract bool Validate();
    }
}