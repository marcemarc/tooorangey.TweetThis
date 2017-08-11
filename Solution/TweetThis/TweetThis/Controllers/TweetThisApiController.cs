using Skybrud.Social.Twitter;
using Skybrud.Social.Twitter.Responses;
using Skybrud.Social.Umbraco.Twitter.PropertyEditors.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core.Logging;
using Umbraco.Web.WebApi;
using Umbraco.Web;
using Umbraco.Core.Models;

namespace TweetThis.Controllers
{
    public class TweetThisApiController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// get the TwitterOAuthData for the authenticated account
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        private TwitterOAuthData getTwitterAccount(int contentId)
        {
            var contentPage = Umbraco.TypedContent(contentId);
            var homePage = contentPage != null ? contentPage.AncestorOrSelf(1) : default(IPublishedContent);
            var twitterAccount = homePage != null && homePage.HasProperty("authenticatedTwitterAccount") && homePage.HasValue("authenticatedTwitterAccount") ? homePage.GetPropertyValue<TwitterOAuthData>("authenticatedTwitterAccount") : default(TwitterOAuthData);
            return twitterAccount;
        }
        /// <summary>
        /// helper to return the twitter account
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TwitterOAuthData GetTwitterAccount(int id)
        {
            return this.getTwitterAccount(id);
        }

        /// <summary>
        /// endpoint that receives the twitter instruction and creates the tweet!
        /// </summary>
        /// <param name="tweetInstruction"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult TweetThis(TweetInstruction tweetInstruction)
        {
            try
            {
                //read authenticated twitter account details
                var twitterAccount = getTwitterAccount(tweetInstruction.ContentId);

                if (twitterAccount != null)
                {
                    //get twitter service using authenticated twitter details
                    TwitterService service = twitterAccount.GetService();
                    TwitterStatusMessageResponse response = service.Statuses.PostStatusMessage(tweetInstruction.Message);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(TweetThisApiController), "Error Tweeting Message", ex);
            }
            return BadRequest("Error");
        }

        /// <summary>
        /// an endpoint to retrieve recent tweets for the authenticated twitter account
        /// </summary>
        /// <param name="blogId"></param>
        /// <returns></returns>
        [HttpGet]
        public TwitterTimelineResponse GetRecentTweets(int blogId)
        {
            TwitterTimelineResponse latestTweets = default(TwitterTimelineResponse);
            try
            {
                var blogHome = Umbraco.TypedContent(blogId);
                if (blogHome != null)
                {
                    var twitterAccount = blogHome.HasProperty("authenticatedTwitterAccount") && blogHome.HasValue("authenticatedTwitterAccount") ? blogHome.GetPropertyValue<TwitterOAuthData>("authenticatedTwitterAccount") : default(TwitterOAuthData);
                    if (twitterAccount != null)
                    {
                        //get twitter service using authenticated twitter details
                        Skybrud.Social.Twitter.TwitterService service = twitterAccount.GetService();
                        //get last 5 tweets
                        latestTweets = service.Statuses.GetUserTimeline(twitterAccount.ScreenName, 5);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(TweetThisApiController), "Error Retrieving Timeline", ex);
            }
            return latestTweets;
        }
        /// <summary>
        /// Only need this for versions of Umbraco less than 7.6 that don't have the new getUrl angularJS helper
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public string GetByUrl(int id)
        {
            var blogUrl = "#";
            var blogPost = Umbraco.TypedContent(id);
            if (blogPost != null)
            {
                blogUrl = blogPost.Url;
            }
            return blogUrl;

        }

        /// <summary>
        /// Storing information about the blog and twitter message template 
        /// on the blog homepage, you might not be using this with multiple blogs
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public BlogInfo GetBlogInfoFromBlogPost(int id)
        {
            var blogInfo = new BlogInfo();
            var blogPost = Umbraco.TypedContent(id);
            var blogPostUrlWithDomain = blogPost.UrlWithDomain();
            if (blogPostUrlWithDomain.EndsWith("/"))
            {
                blogPostUrlWithDomain.Remove(blogPostUrlWithDomain.Length - 1, 1);
            }
            var homePage = blogPost != null ? blogPost.AncestorOrSelf(1) : default(IPublishedContent);
            if (homePage != null)
            {
                blogInfo.Id = homePage.Id;
                blogInfo.Name = homePage.Name;
                blogInfo.BlogTitle = homePage.HasProperty("blogTitle") && homePage.HasValue("blogTitle") ? homePage.GetPropertyValue<string>("blogTitle") : homePage.Name;
                blogInfo.BlogUrl = homePage.UrlWithDomain().TrimEnd('/');
                var tweetMessageTemplate = homePage.HasProperty("tweetMessageTemplate") && homePage.HasValue("tweetMessageTemplate") ? homePage.GetPropertyValue<string>("tweetMessageTemplate") : "{0} Read more: {1}";
                blogInfo.TweetMessageTemplate = String.Format(tweetMessageTemplate, blogPost.Name, blogPostUrlWithDomain, blogInfo.BlogTitle);
            }
            return blogInfo;
        }

        /// <summary>
        /// this may vary depending on how you have your site setup
        /// eg multiple blogs, articulate, or just a content section of the site
        /// I'm storing twitter message template etc on the homepage of each blog
        /// but you might only have one...
        /// </summary>
        public class BlogInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string BlogTitle { get; set; }
            public string BlogUrl { get; set; }

            public string TweetMessageTemplate { get; set; }

        }
        /// <summary>
        /// the instruction that gets sent to the api, to create the tweet
        /// </summary>
        public class TweetInstruction
        {
            public int ContentId { get; set; }
            public string Message { get; set; }
        }


    }
}

