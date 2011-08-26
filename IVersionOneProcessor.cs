using System.Collections.Generic;
using VersionOne.ServerConnector.Entities;
using VersionOne.ServerConnector.Filters;

namespace VersionOne.ServerConnector {
    public interface IVersionOneProcessor {
        bool ValidateConnection();
        
        IList<PrimaryWorkitem> GetWorkitemsByProjectId(string projectId);
        IList<PrimaryWorkitem> GetClosedWorkitemsByProjectId(string projectId);
        IList<FeatureGroup> GetFeatureGroupsByProjectId(string projectId, Filter filters, Filter childrenFilters);
        
        void SaveWorkitems(IEnumerable<Workitem> workitems);
        void CloseWorkitem(PrimaryWorkitem workitem);
        void UpdateProject(string projectId, string link, string linkTitle);
        string GetWorkitemLink(PrimaryWorkitem workitem);
        void SetWorkitemStatus(PrimaryWorkitem workitem, string statusId);
        KeyValuePair<string, string> CreateWorkitemStatus(string statusName);
        IList<KeyValuePair<string, string>> GetWorkitemStatuses();
        IList<KeyValuePair<string, string>> GetWorkitemPriorities();
        
        bool ProjectExists(string projectId);
        bool TypeExists(string typeName);
        bool AttributeExists(string typeName, string attributeName);
        
        void AddProperty(string attr, string prefix, bool isList);
        void AddOptionalProperty(string attr, string prefix);
        void AddLinkToWorkitem(Workitem workitem, string link, string title, bool onMenu);

        IDictionary<string, PropertyValues> ListPropertyValues { get; }
    }
}