using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;


namespace Web.MvcHtmlHelpers
{
    /// <summary>
    /// A Razer extension for finding "Mime Icons" for linked documents in sitecore.  This is useful if your content
    /// editors have lots of lists of files that they want to share within their website.  You can use this to include
    /// an icon for pdf/doc/xlsx/etc
    /// 
    /// To get this to work, you need to create a folder in the media library at: /sitecore/media library/mime icons
    /// Within this mime icons folder, you need the following images:
    ///     Default
    ///     Internal  (set mime type to application/internal-link)
    ///     External  (set mime type to application/external-link)
    ///     pdf       (set mime type to application/pdf)
    ///     etc
    /// 
    /// Then, in Razer you can do the following:
    ///     @Html.MimeIcon(Model.Item.Fields["Link"])
    ///     @Html.MimeIcon(someItem) // in this case, someItem would be obtained from a multilist
    /// </summary>
    public static class MimeIconExtensions
    {
        private const string mimeIconLocation = "/sitecore/media library/mime icons";

        public static MvcHtmlString MimeIcon(this HtmlHelper helper, MediaItem item)
        {
            return new MvcHtmlString(BuildImageString(GetIconUriFor(item)));
        }

        public static MvcHtmlString MimeIcon(this HtmlHelper helper, LinkField field)
        {
            string retVal = string.Empty;

            try
            {
                // media link
                if (field.IsMediaLink)
                {
                    retVal = BuildImageString(GetIconUriFor(field.TargetItem));
                }
                // internal non media link
                else if (field.IsInternal)
                {
                    retVal = BuildImageString(GetIconUriFor("application/internal-link"));
                }
                // external link
                else
                {
                    retVal = BuildImageString(GetIconUriFor("application/external-link"));
                }
            }
            catch (Exception e)
            {
                Log.Error("Cannot find mime type for field " + field.Value, e);
            }

            return new MvcHtmlString(retVal);
        }

        private static string BuildImageString(string urlToIcon)
        {
            return String.Format("<img src='{0}' />", urlToIcon);
        }

        private static string GetIconUriFor(MediaItem mediaItem)
        {
            return GetIconUriFor(mediaItem.MimeType);
        }

        private static string GetIconUriFor(string mimeType)
        {
            string retVal = string.Empty;

            try
            {
                var mimeItems = Sitecore.Context.Database.GetItem(mimeIconLocation);

                // one of the mime icons is named "Default"
                retVal = MediaManager.GetMediaUrl(mimeItems.Children.FirstOrDefault(p => p.Name == "Default"));

                // try and find a mime icon based on the MimeType from the item's template
                // you could improve this by not abusing the default Mime field, and instead using a custom template.
                var mime = mimeItems.Children.FirstOrDefault(x => x.Fields["Mime"].Value == mimeType);

                if (mime != null)
                {
                    retVal = MediaManager.GetMediaUrl(mime);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Can't find mime type for document.  " + ex.Message, ex);
            }

            return retVal;
        }
    }
}