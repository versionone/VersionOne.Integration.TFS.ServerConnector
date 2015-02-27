using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.Integration.Tfs.ServerConnector.Entities
{
	public class Theme : Workitem
	{
		public override string TypeToken
		{
			get { return VersionOneProcessor.ThemeType; }
		}

		internal Theme(Asset asset, IDictionary<string, PropertyValues> listValues, IList<Member> owners, IEntityFieldTypeResolver typeResolver)
			: base(asset, listValues, owners, typeResolver) { }
	}
}