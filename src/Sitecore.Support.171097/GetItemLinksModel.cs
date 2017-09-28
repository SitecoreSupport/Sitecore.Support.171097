using Sitecore.Data;
using Sitecore.Data.Items;
using System.Collections.Generic;

namespace Sitecore.Support
{
  public class GetItemLinksModel
  {
    public Item Argument { get; set; }

    public List<object> List { get; set; }

    public List<ID> List2 { get; set; }

    public Sitecore.Support.ExperienceEditor.Speak.Ribbon.Requests.DatasourceUsages.GetDatasourceUsagesCount Instance { get; set; }
  }
}