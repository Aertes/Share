using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Mvc;
using NetDimension.OpenAuth.Sina;
using ITNode.YXK.Components;
using ITNode.YXK.Models;

namespace widget2.Controllers
{
    public class SinaCallBackController : Controller
    {
        // GET: Authorized
        public ActionResult Index()
        {
            string code= Request.QueryString["code"];
            string state = Request.QueryString["state"];
            string id =(string)TempData["siteid"];
            //return Redirect("/Share");
            //return Content((string)System.Web.HttpContext.Current.Session["siteid"],"text");
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(state))
            {
                Authorized(code, state);
            }
            else
                Redirect("/Share");

            return View();
        }
      

        private SinaWeiboClient GetOpenAuthClient()
        {
            var accessToken = Session["access_token"] == null ? string.Empty : (string)Session["access_token"];
            var uid = Request.Cookies["uid"] == null ? string.Empty : Request.Cookies["uid"].Value;
            var settings = ConfigurationManager.AppSettings;
            var client = new SinaWeiboClient(settings["appKey"], settings["appSecret"], settings["callbackUrl"], accessToken, uid);

            return client;
        }
        /// <summary>
		/// 授权认证
		/// </summary>
		/// <param name="code">新浪返回的code</param>
		/// <returns></returns>
		private void Authorized(string code, string state)
        {
            //if (string.IsNullOrEmpty(code))
            //{
            //    return RedirectToAction("Index");
            //}

            var client = GetOpenAuthClient();

            client.GetAccessTokenByCode(code);

            if (client.IsAuthorized)
            {
                var response = client.HttpGet("users/show.json", new
                {
                    uid = client.UID
                });

                if (response.IsSuccessStatusCode)
                {

                    var json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    int siteid = 0;
                    int snsid = 0;
                    string url = "";
                    string sharepic = "";
                    string sharecontent = "";
                    string shareurl = "";
                    if (!string.IsNullOrEmpty(state))
                    {
                        siteid = Convert.ToInt32(state.Split('|')[0]);
                        snsid = Convert.ToInt32(state.Split('|')[1]);
                        url  = state.Split('|')[2];
                        sharepic= state.Split('|')[3];
                    }
                    siteid = Convert.ToInt32(Session["siteid"]);
                    snsid = Convert.ToInt32(Session["snsid"]);
                    Site site = new Site();

                    site = SiteService.GetSite(Convert.ToInt32(siteid));
                    //根据ID获得SNS名称
                    SNS sns = new SNS();
                    sns = SNSService.GetSNS(Convert.ToInt32(snsid));

                    //用户信息入库
                    SNSUser user = new SNSUser();
                    user.NickName = json["screen_name"].ToString();
                    user.HeadImg = json["profile_image_url"].ToString();
                    user.Email = "";
                    user.Mobile = "";
                    user.Birthday = Convert.ToDateTime("1900-01-01");
                    user.SiteID = site.ID;
                    user.SiteName = site.SiteName;
                    user.SNSUID = client.UID;
                    user.SNSName = sns.Name;
                    user.SNSID = sns.ID;
                    String location = json["location"].ToString();
                    String[] aa = location.Split(new char[] { ' ' });
                    if (aa.Count() >= 1)
                    {
                        user.Province = aa[0];
                    }
                    if (aa.Count() >= 2)
                    {
                        user.City = aa[1];
                    }
                    else
                    {
                        user.City = "";
                    }
                    //1为男，2为女,0未填写
                    user.Sex = 0;
                    if (json["gender"].ToString() == "m")
                    {
                        user.Sex = 1;
                    }
                    if (json["gender"].ToString() == "f")
                    {
                        user.Sex = 2;
                    }
                    user.FansCount = Convert.ToInt32(json["friends_count"].ToString());
                    user.FollowCount = Convert.ToInt32(json["followers_count"].ToString());
                    user.verified = Convert.ToBoolean(json["verified"].ToString());
                    user.CreateTime = DateTime.Now;

                    int userid = SNSManager.CreateSNSUser(user);

                    ////同步到微博
                    //if (url.IndexOf("?") > -1)
                    //{
                    //    url = url + "&yxkfrom=" + userid + "&yxksnsid=" + user.SNSID;
                    //}
                    //else
                    //{
                    //    url = url + "?yxkfrom=" + userid + "&yxksnsid=" + user.SNSID;
                    //}
                    //string weiboid;
                    //if (sharepic != "")
                    //{
                    //    //图片微博
                    //    util ut = new util();
                    //    var rsp = Sina.API.Statuses.Upload(sharecontent + " " + url, ut.BitmapToBytes(Convert.ToString(Session["sharepic"])));
                    //    weiboid = rsp.id;
                    //}
                    //else
                    //{
                    //    //文字微博
                    //    var rsp = Sina.API.Statuses.Update(sharecontent + " " + url);
                    //    weiboid = rsp.id;
                    //}

                    ////根据URL来获得所属类别
                    //SiteCategory sc = new SiteCategory();
                    //sc = SiteCategoryService.GetSiteCategory(site.ID, shareurl);


                    ////分享内容入库
                    //Share s = new Share();
                    //s.Content = sharecontent;
                    //s.CommentCount = 0;
                    //s.ForwardCount = 0;
                    //s.IsFollow = Convert.ToBoolean(Session["isfollow"]);
                    //s.SiteID = site.ID;
                    //s.SiteName = site.SiteName;
                    //s.SNSID = sns.ID;
                    //s.SNSName = sns.Name;
                    //s.SNSUserID = userid;
                    //s.SNSContentID = weiboid;
                    //s.SNSUserNickName = user.NickName;
                    //s.URL = shareurl;
                    //s.Vote = 0;
                    //s.SiteCategoryID = -1;
                    //s.SiteCategoryName = "";
                    //if (sc != null)
                    //{
                    //    s.SiteCategoryID = sc.ID;
                    //    s.SiteCategoryName = sc.CategoryName;
                    //}
                    //s.AgreeBeMember = Convert.ToBoolean(Session["bemember"]);
                    //s.PageTitle = Convert.ToString(Session["sharepagetitle"]);
                    //s.Picture = Convert.ToString(Session["sharepic"]);
                    //s.CreateTime = DateTime.Now;

                    //int sid = ShareService.CreateShare(s);
                    ////分享内容入更新表
                    //ShareUpdate su = new ShareUpdate();
                    //su.ShareID = sid;
                    //su.SNSID = s.SNSID;
                    //su.SNSName = s.SNSName;
                    //su.SiteCategoryID = s.SiteCategoryID;
                    //su.SiteCategoryName = s.SiteCategoryName;
                    //su.SiteID = s.SiteID;
                    //su.SiteName = s.SiteName;
                    //su.SNSContentID = s.SNSContentID;
                    //su.CommentCount = 0;
                    //su.ForwardCount = 0;
                    //su.CreateTime = DateTime.Now;
                    //ShareUpdateService.CreateShareUpdate(su);


                    ////如果关注的话，执行微博官微API并将关注数据入库
                    //if (s.IsFollow)
                    //{
                    //    var rst = Sina.API.Friendships.Show("", user.NickName, "", Convert.ToString(Session["shareguanweiid"]));
                    //    if (!rst.target.followed_by)
                    //    {
                    //        Sina.API.Friendships.Create("", Convert.ToString(Session["shareguanweiid"]));
                    //        Follow fo = new Follow();
                    //        fo.CreateTime = DateTime.Now;
                    //        fo.PageTitle = s.PageTitle;
                    //        fo.SiteCategoryID = s.SiteCategoryID;
                    //        fo.SiteCategoryName = s.SiteCategoryName;
                    //        fo.SiteID = s.SiteID;
                    //        fo.SiteName = s.SiteName;
                    //        fo.SNSContentID = s.SNSContentID;
                    //        fo.SNSID = s.SNSID;
                    //        fo.SNSName = s.SNSName;
                    //        fo.SNSUserID = s.SNSUserID;
                    //        fo.SNSUserNickName = s.SNSUserNickName;
                    //        fo.URL = s.URL;

                    //        FollowService.CreateFollow(fo);
                    //    }
                    //}
                    //ConfigURL cfgurl = new ConfigURL();
                    //cfgurl = ConfigURLService.CreateAndUpdateConfigURL(s.SiteID, s.URL);

                    //if (cfgurl.CampaignID == 2)
                    //{
                    //    Session["campaignshareid"] = sid.ToString();
                    //    Session["campaigncfgid"] = cfgurl.ConfigID.ToString();
                    //    // Response.Redirect("campaign/LuckyDrawPlay.aspx");
                    //}

                    //Response.Redirect("info.aspx?s=分享成功&type=s");

                    //return RedirectToAction("Index");

                    //return Content(json.ToString(Formatting.None), "application/json");
                }
                else
                {
                    var json = new JObject();
                    json["authorized"] = false;
                    json["data"] = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    json["authorized"] = true;
                   // return Content(json.ToString(Formatting.None), "application/json");
                }
                ////用session记录access token
                //Session["access_token"] = client.AccessToken;
                ////用cookie记录uidvfbge2ew
                //Response.AppendCookie(new HttpCookie("uid", client.UID) { Expires = DateTime.Now.AddDays(7) });
               // return RedirectToAction("Index");
            }
            //else
            //{
            //    return RedirectToAction("Index");
            //}


            // client = new NetDimension.Weibo.OAuth(weiboappkey, weiboappsecret);


            //try
            //{
            //    /
            //    //查询站点名称

            //}
            //catch (WeiboException wex)
            //{
            //    Response.Redirect("info.aspx?s=" + wex.Message + "&type=f");
            //    //throw wex;
            //}


        }
    }
}