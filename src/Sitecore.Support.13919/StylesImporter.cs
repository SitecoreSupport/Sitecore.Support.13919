using Sitecore.Data.Items;
using Sitecore.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sitecore.XA.Feature.CreativeExchange.Extensions;
using Sitecore.XA.Feature.CreativeExchange.Pipelines.Import.RenderingProcessing;
using Sitecore.Data.Fields;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
using Sitecore.XA.Foundation.Presentation.Extensions;
using Sitecore.Diagnostics;

namespace Sitecore.Support.XA.Feature.CreativeExchange.Pipelines.Import.RenderingProcessing
{
  public class StylesImporter : Sitecore.XA.Feature.CreativeExchange.Pipelines.Import.RenderingProcessing.StylesImporter
  {
    protected override void CreateMissingComponentClass(ImportRenderingProcessingArgs args, string parameterName, IEnumerable<string> parameterValues)
    {
      LayoutField layoutField = args.RenderingSourceItem.IsPartialDesign() ? new LayoutField(args.RenderingSourceItem.Fields[FieldIDs.FinalLayoutField]) : new LayoutField(args.RenderingSourceItem.Fields[args.LayoutFieldID]);
      string layoutXml = layoutField.Value;
      string currentDeviceLayoutXml = this.GetCurrentDeviceLayoutXml(args, layoutXml, args.Page.DisplayName, args.ImportContext.DeviceId);
      string renderingXmlNode = this.GetRenderingXmlNode(args, args.Page, currentDeviceLayoutXml, args.RenderingUniqueId);
      if (renderingXmlNode == null)
      {
        args.Messages.AddWarning(Translate.Text("Unable to find the rendering with {0} id on the {1} page. Possibly, the rendering is loaded from the fallback device."), (object)args.Rendering.ID, (object)args.Page.DisplayName);
      }
      else
      {
        Item pageDesignsItem = this.PresentationContext.GetPageDesignsItem(args.Page);
        if (pageDesignsItem == null)
          return;
        Item stylesItem = pageDesignsItem.Parent.FirstChildInheritingFrom(Sitecore.XA.Foundation.Presentation.Templates.Styles.ID);
        IList<string> list = (IList<string>)this.GetUsedClasses(args, stylesItem, args.Rendering, parameterValues).ToList<string>();
        Match match = Regex.Match(renderingXmlNode, this.GetParametersString("([^\"]+)"));
        string renderingParametersString = match.Groups[1].Value;
        string newParamsString = this.CreateNewParamsString(args, renderingParametersString, parameterName, (IEnumerable<string>)list);
        string newValue1 = match.Success ? renderingXmlNode.Replace(match.Value, newParamsString) : renderingXmlNode.Insert(renderingXmlNode.LastIndexOf('"') + 1, " " + newParamsString);
        string newValue2 = currentDeviceLayoutXml.Replace(renderingXmlNode, newValue1);
        string str = layoutXml.Replace(currentDeviceLayoutXml, newValue2);
        if (!(layoutField.Value != str))
          return;
        args.RenderingSourceItem.Editing.BeginEdit();
        layoutField.Value = str.Replace(this.GetParametersString(string.Empty), string.Empty);
        args.RenderingSourceItem.Editing.EndEdit();
      }
    }
  }
}
