using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ExperienceEditor.Speak.Ribbon.Requests.DatasourceUsages;
using Sitecore.ExperienceEditor.Speak.Server.Responses;
using Sitecore.Links;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Sitecore.Support.ExperienceEditor.Speak.Ribbon.Requests.DatasourceUsages
{
  public class GetDatasourceUsagesCount : GetDatasourceUsagesBase
  {
    private const int fallbackLimitItemsCount = 2;
    protected int _maxProcessTime;

    public GetDatasourceUsagesCount()
    {
      try
      {
        _maxProcessTime = Sitecore.Configuration.Settings.GetIntSetting("ExperienceEditor.GetDatasourceUsagesCount.MaxProcessTime", fallbackLimitItemsCount);
      }
      catch (Exception)
      {
        _maxProcessTime = fallbackLimitItemsCount;
      }
      if (_maxProcessTime < 0)
      {
        _maxProcessTime = fallbackLimitItemsCount;
      }
    }

    public override object GenerateItemUsagesData(Item item, int count)
    {
      return new { ItemId = item.ID };
    }

    public override PipelineProcessorResponseValue ProcessRequest()
    {
      List<object> list = new List<object>();
      Item argument = base.RequestContext.Item;
      Assert.ArgumentNotNull(argument, "item");
      List<ID> list2 = new List<ID>
      {
        argument.ID
      };

      list.Add(this.GenerateItemUsagesData(argument, 1));

      var model = new GetItemLinksModel
      {
        Argument = argument,
        List = list,
        List2 = list2,
        Instance = this
      };

      var result = new PipelineProcessorResponseValue { Value = list };

      if (_maxProcessTime == 0)
      {
        GetItemLinksLimited(model);
      }
      else
      {
        var thread = new Thread(GetItemLinksLimited);
        thread.Priority = ThreadPriority.BelowNormal;
        thread.Start(model);
        if (!thread.Join(_maxProcessTime * 1000))
        {
          result.Value = new { length = list.Count + "+" };
        }
      }

      return result;
    }

    protected static void GetItemLinksLimited(object obj)
    {
      var model = (GetItemLinksModel)obj;
      int count = 1;
      foreach (ItemLink link in model.Instance.GetItemLinks(model.Argument))
      {
        if (model.Instance.HasLayoutFieldLink(link))
        {
          using (new SecurityDisabler())
          {
            Item item = model.Argument.Database.GetItem(model.Instance.ItemLinkId(link));
            if (((item != null) && !model.List2.Contains(item.ID)) && model.Instance.IsValidItem(item))
            {
              count++;
              model.List2.Add(item.ID);
              model.List.Add(model.Instance.GenerateItemUsagesData(item, count));
            }
          }
        }
      }
    }

  }
}