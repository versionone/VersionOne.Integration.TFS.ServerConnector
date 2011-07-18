using System.Collections.Generic;

namespace VersionOne.ServerConnector {
    public interface IVersionOneProcessor {
        bool ValidateConnection();
        IList<PrimaryWorkitem> GetWorkitemsByProjectId(string projectId);
        IList<PrimaryWorkitem> GetClosedWorkitemsByProjectId(string projectId);
        IList<FeatureGroup> GetFeatureGroupsByProjectId(string projectId);
        void UpdateWorkitemLinkAndReference(PrimaryWorkitem workitem, string cardId, string cardLink);
        void SaveWorkitems(IEnumerable<PrimaryWorkitem> workitems);
        void CloseWorkitem(PrimaryWorkitem workitem);
        void UpdateProject(string projectId, string boardLink);
        string GetWorkitemLink(PrimaryWorkitem workitem);
        IList<string> GetAssetTypes();
        void SetWorkitemStatus(PrimaryWorkitem workitem, string statusId);
        KeyValuePair<string, string> CreateWorkitemStatus(string statusName);
        IList<KeyValuePair<string, string>> GetWorkitemStatuses();
        IList<KeyValuePair<string, string>> GetWorkitemPriorities();
        bool IsProjectExist(string projectId);
    }
}