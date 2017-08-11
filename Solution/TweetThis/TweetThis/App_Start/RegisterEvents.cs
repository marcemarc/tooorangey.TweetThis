using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Trees;

namespace TweetThis.App_Start
{
    public class RegisterEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //register custom menu item in the media tree
            TreeControllerBase.MenuRendering += TreeControllerBase_MenuRendering;
        }

        private void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            //check if tree is content
            if (sender.TreeAlias == "content")
            { 
                //get content item as IPublishedContent to read document type
                var contentItem = sender.Umbraco.TypedContent(e.NodeId);
                //if it's a blog post let's add the menu item
                if (contentItem != null && contentItem.DocumentTypeAlias == "blogpost")
                {
                    // create a new menu Item
                    var tweetThisMenuItem = new Umbraco.Web.Models.Trees.MenuItem("tweetThis", "Tweet This");
                    //give it an appropriate icon
                    tweetThisMenuItem.Icon = "bird";
                    //whether to add a seperator line before the item in the menu
                    tweetThisMenuItem.SeperatorBefore = true;
                    //wiring up what to do when the menu item is selected
                    //here we are loading our tweetthis view containing our twitter message box onto as an actionView (slidey out thing)
                    tweetThisMenuItem.AdditionalData.Add("actionView", "/app_plugins/tooorangey.TweetThis/tweetthis.html");
                    //here we add to the menu
                    e.Menu.Items.Insert(5, tweetThisMenuItem);
                }
            }
        }
    }
}
