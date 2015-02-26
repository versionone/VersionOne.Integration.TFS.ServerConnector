using VersionOne.SDK.APIClient;

namespace VersionOne.Integration.TFS.ServerConnector.Filters {
    public interface IFilter {
        GroupFilterTerm GetFilter(IAssetType type);
    }
}