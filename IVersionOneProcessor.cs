using System.Collections.Generic;

namespace VersionOne.ServerConnector {
    public interface IVersionOneProcessor {
        bool ValidateConnection();
        IList<Workitem> GetWorkitemsByProjectId(string projectId);
        IList<Workitem> GetClosedWorkitemsByProjectId(string projectId);
        void UpdateWorkitemLinkAndReference(Workitem workitem, string cardId, string cardLink);
        void SaveWorkitems(IEnumerable<Workitem> workitems);
        void CloseWorkitem(Workitem workitem);
        void UpdateProject(string projectId, string boardLink);
        string GetWorkitemLink(Workitem workitem);
        IList<string> GetAssetTypes();
        void SetWorkitemStatus(Workitem workitem, string statusId);
        KeyValuePair<string, string> CreateWorkitemStatus(string statusName);
        IList<KeyValuePair<string, string>> GetWorkitemStatuses();
        IList<KeyValuePair<string, string>> GetWorkitemPriorities();
        bool IsProjectExist(string projectId);
    }
}