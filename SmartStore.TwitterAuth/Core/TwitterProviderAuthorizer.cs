using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Services.Authentication.External;

namespace SmartStore.TwitterAuth.Core
{
    public class TwitterProviderAuthorizer : IExternalProviderAuthorizer
    {
        private const string OAUTH_VERSION = "1.0";
        private const string TWITTER_AUTHENTICATE_URL = "https://api.twitter.com/oauth/authenticate";
        private const string TWITTER_REQUESTTOKEN_URL = "https://api.twitter.com/oauth/request_token";
        private const string TWITTER_ACCESSTOKEN_URL = "https://api.twitter.com/oauth/access_token";
        private const string TWITTER_ACCOUNT_URL = "https://api.twitter.com/1.1/account/verify_credentials.json";

        private readonly IExternalAuthorizer _authorizer;
        private readonly HttpContextBase _httpContext;
        private readonly ICommonServices _services;
        private readonly TwitterExternalAuthSettings _twitterSettings;

        public TwitterProviderAuthorizer(
            IExternalAuthorizer authorizer,
            HttpContextBase httpContext,
            ICommonServices services,
            TwitterExternalAuthSettings twitterSettings)
        {
            _authorizer = authorizer;
            _httpContext = httpContext;
            _services = services;
            _twitterSettings = twitterSettings;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        private string CreateNonce()
        {
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)));
        }

        private string CreateTimeStamp()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            return Convert.ToInt64(timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        private string GetErrorMessage(WebException ex)
        {
            string error = null;

            using (var response = ex.Response as HttpWebResponse)
            {
                error = response.StatusDescription;

                var enc = Encoding.GetEncoding(response.CharacterSet);
                using (var reader = new StreamReader(response.GetResponseStream(), enc))
                {
                    error = reader.ReadToEnd();
                }
            }

            return error;
        }

        private NameValueCollection GetRequestToken(string callbackUrl)
        {
            var helper = new TwitterUrlHelper(TWITTER_REQUESTTOKEN_URL);
            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("oauth_callback", callbackUrl));
            parameters.Add(new KeyValuePair<string, string>("oauth_consumer_key", _twitterSettings.ConsumerKey));
            parameters.Add(new KeyValuePair<string, string>("oauth_nonce", CreateNonce()));
            parameters.Add(new KeyValuePair<string, string>("oauth_signature_method", "HMAC-SHA1"));
            parameters.Add(new KeyValuePair<string, string>("oauth_timestamp", CreateTimeStamp()));
            parameters.Add(new KeyValuePair<string, string>("oauth_version", OAUTH_VERSION));

            var signature = helper.CreateSignature(_twitterSettings.ConsumerSecret, parameters);
            parameters.Insert(3, new KeyValuePair<string, string>("oauth_signature", signature));

            using (var client = new WebClient())
            {
                var url = helper.CreateCallingUrls(parameters);
                var response = client.DownloadString(url);
                var dict = HttpUtility.ParseQueryString(response);
                return dict;
            }
        }

        private NameValueCollection GetAccessToken(string oauth_token, string oauth_verifier)
        {
            var helper = new TwitterUrlHelper(TWITTER_ACCESSTOKEN_URL);
            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("oauth_consumer_key", _twitterSettings.ConsumerKey));
            parameters.Add(new KeyValuePair<string, string>("oauth_nonce", CreateNonce()));
            parameters.Add(new KeyValuePair<string, string>("oauth_signature_method", "HMAC-SHA1"));
            parameters.Add(new KeyValuePair<string, string>("oauth_timestamp", CreateTimeStamp()));
            parameters.Add(new KeyValuePair<string, string>("oauth_token", oauth_token));
            parameters.Add(new KeyValuePair<string, string>("oauth_verifier", oauth_verifier));
            parameters.Add(new KeyValuePair<string, string>("oauth_version", OAUTH_VERSION));

            var signature = helper.CreateSignature(_twitterSettings.ConsumerSecret, parameters, null);
            parameters.Insert(2, new KeyValuePair<string, string>("oauth_signature", signature));

            using (var client = new WebClient())
            {
                var url = helper.CreateCallingUrls(parameters);
                var response = client.DownloadString(url);
                var dict = HttpUtility.ParseQueryString(response);
                return dict;
            }
        }

        private string GetAccount(string authToken, string authTokenSecret)
        {
            var helper = new TwitterUrlHelper(TWITTER_ACCOUNT_URL);
            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("include_email", "true"));
            parameters.Add(new KeyValuePair<string, string>("oauth_consumer_key", _twitterSettings.ConsumerKey));
            parameters.Add(new KeyValuePair<string, string>("oauth_nonce", CreateNonce()));
            parameters.Add(new KeyValuePair<string, string>("oauth_signature_method", "HMAC-SHA1"));
            parameters.Add(new KeyValuePair<string, string>("oauth_timestamp", CreateTimeStamp()));
            parameters.Add(new KeyValuePair<string, string>("oauth_token", authToken));
            parameters.Add(new KeyValuePair<string, string>("oauth_version", OAUTH_VERSION));

            var signature = helper.CreateSignature(_twitterSettings.ConsumerSecret, parameters, authTokenSecret);
            parameters.Insert(3, new KeyValuePair<string, string>("oauth_signature", signature));

            using (var client = new WebClient())
            {
                var url = helper.CreateCallingUrls(parameters);
                var response = client.DownloadString(url);
                return response;
            }
        }

        public AuthorizeState Authorize(string returnUrl, bool? verifyResponse)
        {
            AuthorizeState state = null;
            string error = null;

            try
            {
                if (verifyResponse.Value)
                {
                    string email = null;
                    string name = null;
                    var token = _httpContext.Request.QueryString?.GetValues("oauth_token")?.FirstOrDefault();
                    var verifier = _httpContext.Request.QueryString?.GetValues("oauth_verifier")?.FirstOrDefault();

                    var responseData = GetAccessToken(token, verifier);

                    var parameters = new OAuthAuthenticationParameters(TwitterExternalAuthMethod.SystemName)
                    {
                        ExternalIdentifier = responseData["oauth_token"],
                        ExternalDisplayIdentifier = responseData["screen_name"],
                        OAuthToken = responseData["oauth_token"],
                        OAuthAccessToken = responseData["oauth_token_secret"]
                    };

                    // Get email address and full name.
                    try
                    {
                        var str = GetAccount(parameters.OAuthToken, parameters.OAuthAccessToken);
                        if (str.HasValue())
                        {
                            var json = JObject.Parse(str);
                            email = json.GetValue("email").ToString();
                            name = json.GetValue("name").ToString();
                        }
                    }
                    catch (WebException wex)
                    {
                        Logger.Error(GetErrorMessage(wex));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message);
                    }

                    var splittedName = name.SplitSafe(" ");
                    var claims = new UserClaims();
                    claims.Name = new NameClaims();
                    claims.Contact = new ContactClaims();
                    claims.Contact.Email = email;
                    claims.Name.FullName = name;

                    if (splittedName.Length >= 2)
                    {
                        claims.Name.First = splittedName[0];
                        claims.Name.Last = splittedName[1];
                    }
                    else if (splittedName.Length >= 1)
                    {
                        claims.Name.Last = splittedName[0];
                    }

                    //$"{claims.Contact.Email.NaIfEmpty()} {claims.Name.FullName.NaIfEmpty()}: {claims.Name.First.NaIfEmpty()} {claims.Name.Last.NaIfEmpty()}".Dump();

                    parameters.AddClaim(claims);

                    var result = _authorizer.Authorize(parameters);

                    state = new AuthorizeState(returnUrl, result);
                }
                else
                {
                    var callbackUrl = _services.WebHelper.GetStoreLocation() + "Plugins/SmartStore.TwitterAuth/LoginCallback/";
                    var responseData = GetRequestToken(callbackUrl);
                    var authenticateUrl = string.Concat(TWITTER_AUTHENTICATE_URL, "?oauth_token=", HttpUtility.UrlEncode(responseData["oauth_token"]));

                    state = new AuthorizeState(string.Empty, OpenAuthenticationStatus.RequiresRedirect)
                    {
                        Result = new RedirectResult(authenticateUrl)
                    };
                }
            }
            catch (WebException wex)
            {
                error = GetErrorMessage(wex);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (state == null)
            {
                error = error.NullEmpty() ?? _services.Localization.GetResource("Admin.Common.UnknownError");
                state = new AuthorizeState(string.Empty, OpenAuthenticationStatus.Error);
                state.AddError(error);
                Logger.Error(error);
            }

            return state;
        }
    }
}