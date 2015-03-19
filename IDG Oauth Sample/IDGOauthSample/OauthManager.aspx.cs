/*
 * Copyright (c) 2015 Intuit, Inc.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.opensource.org/licenses/eclipse-1.0.php
 * Contributors:
 *
 *    Intuit Developer Group - initial contribution 
 *
 */

using System;
using System.Configuration;
using System.Web;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Provider;
using DevDefined.OAuth.Storage.Basic;


namespace IDGOauthSample
{
    public partial class OauthManager : System.Web.UI.Page
    {
        public static String REQUEST_TOKEN_URL = "https://oauth.intuit.com/oauth/v1/get_request_token";
        public static String ACCESS_TOKEN_URL = "https://oauth.intuit.com/oauth/v1/get_access_token";
        public static String AUTHORIZE_URL = "https://appcenter.intuit.com/connect/begin";
        public static String OAUTH_URL = "https://oauth.intuit.com/oauth/v1";
        public String consumerKey = "qyprd1DKC9998700XCyUsXtgErgCgC";
        public String consumerSecret = "MLUPfmvY7sTcjZs66mMEu4vvMmoZQQAG52VgzYPF";
        public string strrequestToken = "";
        public string tokenSecret = "";
        public string oauth_callback_url = "http://localhost:65281/OauthManager.aspx?";
        public string GrantUrl = "http://localhost:65281/OauthManager.aspx";


        protected void Page_Load(object sender, EventArgs e)
        {


            if (Request.QueryString.HasKeys())
            {
                //Get Access Tokens
                
                HttpContext.Current.Session["oauthToken"] = Request.QueryString["oauth_token"].ToString(); ;
                HttpContext.Current.Session["oauthVerifyer"] = Request.QueryString["oauth_verifier"].ToString(); 
                HttpContext.Current.Session["realm"] = Request.QueryString["realmId"].ToString(); 
                HttpContext.Current.Session["dataSource"] = Request.QueryString["dataSource"].ToString();
                //Stored in a session for demo purposes.
                //Production applications should securely store the Access Token
                getAccessToken();
                

            }
            else if ((HttpContext.Current.Session["oauthLink"] != null || Request.UrlReferrer != null) && HttpContext.Current.Session["accessToken"] == null && HttpContext.Current.Session["accessTokenSecret"]  == null)
            {
                HttpContext.Current.Session["consumerKey"] = consumerKey;
                HttpContext.Current.Session["consumerSecret"] = consumerSecret;
                CreateAuthorization();
                IToken token = (IToken)HttpContext.Current.Session["requestToken"];
                tokenSecret = token.TokenSecret;
                strrequestToken = token.Token; 
               
            }
            else
            {
                //Show Connect to QB Button
                if (HttpContext.Current.Session["accessToken"] == null && HttpContext.Current.Session["accessTokenSecret"] == null)
                {
                    c2qb.Visible = true;
                    disconnect.Visible = false;
                    lblDisconnect.Visible = false;
                }
                else
                {
                    c2qb.Visible = false;
                    disconnect.Visible = true;

                }

            }










        }


        public void GetRequestToken()
        {
            //Remember these for later.
            HttpContext.Current.Session["consumerKey"] = consumerKey;
            HttpContext.Current.Session["consumerSecret"] = consumerSecret;
            //HttpContext.Current.Session["oauthLink"] 

            IOAuthSession session = CreateSession();
            IToken requestToken = session.GetRequestToken();
            HttpContext.Current.Session["requestToken"] = strrequestToken;
            strrequestToken = requestToken.Token;
            tokenSecret = requestToken.TokenSecret;

            //string authURL = GetAuthorizeURL(RequestToken);
            string authUrl = AUTHORIZE_URL + "?oauth_token=" + strrequestToken + "&oauth_callback=" +
                             UriUtility.UrlEncode(oauth_callback_url);
            //hypAuthToken.NavigateUrl = authURL;
            //hypAuthToken.Text = authURL;
            HttpContext.Current.Session["oauthLink"] = authUrl;
            Response.Redirect(authUrl);
        }


        protected IOAuthSession CreateSession()
        {
            var consumerContext = new OAuthConsumerContext
                {
                    ConsumerKey = HttpContext.Current.Session["consumerKey"].ToString(),
                    ConsumerSecret = HttpContext.Current.Session["consumerSecret"].ToString(),
                    SignatureMethod = SignatureMethod.HmacSha1
                };

            return new OAuthSession(consumerContext,
                                    REQUEST_TOKEN_URL,
                                    HttpContext.Current.Session["oauthLink"].ToString(),
                                    ACCESS_TOKEN_URL);
        }
        private void getAccessToken()
        {
            IOAuthSession clientSession = CreateSession();
            IToken accessToken = clientSession.ExchangeRequestTokenForAccessToken((IToken)HttpContext.Current.Session["requestToken"], HttpContext.Current.Session["oauthVerifyer"].ToString());
            HttpContext.Current.Session["accessToken"] = accessToken.Token;
            HttpContext.Current.Session["accessTokenSecret"] = accessToken.TokenSecret;

        }

        protected void CreateAuthorization()
        {
            //Remember these for later.
            HttpContext.Current.Session["consumerKey"] = consumerKey;
            HttpContext.Current.Session["consumerSecret"] = consumerSecret;
            HttpContext.Current.Session["oauthLink"] = OAUTH_URL;

            IOAuthSession session = CreateSession();
            IToken requestToken = session.GetRequestToken();
            HttpContext.Current.Session["requestToken"] = requestToken;
            tokenSecret = requestToken.TokenSecret;

            //string authURL = GetAuthorizeURL(RequestToken);
            string authUrl = AUTHORIZE_URL + "?oauth_token=" + requestToken.Token + "&oauth_callback=" +
                             UriUtility.UrlEncode(oauth_callback_url);
            //hypAuthToken.NavigateUrl = authURL;
            //hypAuthToken.Text = authURL;
            HttpContext.Current.Session["oauthLink"] = authUrl;
            Response.Redirect(authUrl);

        }

        protected void btnDisconnect_Click(object sender, EventArgs e)
        {
            //Clearing all session data
            Session.Clear();
            Session.Abandon();
            HttpContext.Current.Session["accessToken"] = null;
            HttpContext.Current.Session["accessTokenSecret"] = null;
            HttpContext.Current.Session["realm"] = null;
            HttpContext.Current.Session["dataSource"] = null;
            
            disconnect.Visible = false;
            lblDisconnect.Visible = true;

        }
    }
}