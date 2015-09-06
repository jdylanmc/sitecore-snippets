using System.Linq;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using MySitecoreExtensions;

namespace MyWebsite.SecurityItemProvider
{
    /// <summary>
    /// This item provider assumes that you have created a base security template in sitecore, and have
    /// other sitecore templates (like your base page) inheriting from this template.
    /// 
    /// In my case, my template had 3 logical areas to check:
    ///   1. Span of Control - a custom control which represented a tree style organizational structure.
    ///   2. Department - different departments that my users participated in.
    ///   3. Roles - Ultimately driven out of content as my roles are dynamic based on a 3rd party API.
    /// </summary>
    public class ItemProvider : Sitecore.Data.Managers.ItemProvider
    {
        // templates have this template ID (in 7.2 at least)
        private const string STANDARD_TEMPLATE_ID = "{AB86861A-6030-46C5-B394-E8F99E8B87DB}";

        // this is your base security template that each item you want to secure would inherit from
        private const string SECURITY_TEMPLATE_ID = "guid of your custom security template";

        // the name of your site from the site config
        private const string WEBSITE_NAME = "website_name"; 

        protected override Item ApplySecurity(Item item, SecurityCheck securityCheck)
        {
            // if this item's a template, just return standard security
            if (item.TemplateID == ID.Parse(STANDARD_TEMPLATE_ID))
            {
                return base.ApplySecurity(item, securityCheck);
            }

            // detect if running the CMS or the end site 
            // && make sure we're supposed to do security checks
            // && check if the item is derived from the custom security template
            if (Context.Site != null 
                && Context.Site.Name.ToLower() == WEBSITE_NAME 
                && securityCheck != SecurityCheck.Disable 
                && Context.PageMode.IsNormal
                && item.IsDerivedFrom(ID.Parse(SECURITY_TEMPLATE_ID)))
            {
                // here you can apply custom security rules based on your security template.
                // in this case, I have... 
                //  1. a concept called "Span of Control" and an extension method to check it against the user
                //  2. a set of roles on the security template that the user needs to have.  In this case, I perform the check with an extension method.
                //  3. a concept around "Departments", and an extension method hanging off the user to perform this check.
                // Be extremely mindful of the efficiency of this check.  Sitecore will run through this code each time you touch an item through the sitecore API.
                if (item.HasSpanOfControlOver(Context.User) && Context.User.IsInDepartmentFor(item) && Context.User.HasRolesFor(item))
                {
                    return base.ApplySecurity(item, securityCheck);
                }
                else
                {
                    // trick sitecore into thinking that the item doesn't exist
                    return null;
                }
            }

            return base.ApplySecurity(item, securityCheck);
        }
    }

    public static class ItemExtensions
    {
        public static bool IsDerivedFrom([NotNull] this Item item, [NotNull] ID templateId)
        {
            return TemplateManager.GetTemplate(item).IsDerived(templateId);
        }
    }

    public static class TemplateExtensions
    {
        public static bool IsDerived([NotNull] this Template template, [NotNull] ID templateId)
        {
            return template.ID == templateId || template.GetBaseTemplates().Any(baseTemplate => IsDerived(baseTemplate, templateId));
        }
    }
}
