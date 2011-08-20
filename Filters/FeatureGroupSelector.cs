using System;
using System.Collections.Generic;
using System.Linq;
using VersionOne.ServerConnector.Entities;

namespace VersionOne.ServerConnector.Filters {
    /// <summary>
    /// This is only idea that is not currently working as intended because Workitem.GetProperty() can easily return non-primitive objects like Assets.
    /// TODO review and either upgrade to handle at least Status cases or remove this class.
    /// </summary>
    public class FeatureGroupSelector {
        private readonly IList<FeatureGroup> featureGroups;

        public FeatureGroupSelector(IList<FeatureGroup> featureGroups) {
            this.featureGroups = featureGroups;
        }

        public IList<FeatureGroup> Select(Filter filter, Filter childFilter) {
            return featureGroups
                .Where(featureGroup => Matches(featureGroup, filter))
                .Select(item => new FeatureGroup(
                    item.Asset, 
                    item.ListValues, 
                    item.Children.Where(child => Matches(child, childFilter)).ToList(), 
                    item.Owners))
                .ToList();
        }

        private static bool Matches(Workitem item, Filter filter) {
            foreach(var filterValue in filter.Values) {
                switch(filterValue.Action) {
                    case FilterValuesActions.Equal:
                        if(!Equals(filterValue.Value, item.GetProperty<object>(filter.Name))) {
                            return false;
                        }

                        break;
                    case FilterValuesActions.NotEqual:
                        if(Equals(filterValue.Value, item.GetProperty<object>(filter.Name))) {
                            return false;
                        }

                        break;
                    default:
                        throw new NotSupportedException("This action is not currently supported");
                }
            }
            
            return true;
        }
    }
}