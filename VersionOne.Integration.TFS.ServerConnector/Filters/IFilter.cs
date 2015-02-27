using VersionOne.SDK.APIClient;

namespace VersionOne.Integration.Tfs.ServerConnector.Filters {
    public interface IFilter {
        GroupFilterTerm GetFilter(IAssetType type);
    }
}