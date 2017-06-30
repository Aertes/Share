using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Mvc;
using NetDimension.OpenAuth.Sina;
using ITNode.YXK.Components;
using ITNode.YXK.Models;
namespace widget2.Controllers
{
    public class ShareController : Controller
    {
        // GET: Share
        public ActionResult Index()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["siteid"]))
                TempData["siteid"] = Request.QueryString["siteid"];
            if (!string.IsNullOrEmpty(Request.QueryString["snsid"]))
                TempData["snsid"] = Request.QueryString["snsid"];
            if (!string.IsNullOrEmpty(Request.QueryString["snsname"]))
                TempData["snsname"] = Request.QueryString["snsname"];
            if (!string.IsNullOrEmpty(Request.QueryString["url"]))
                TempData["url"] = Request.QueryString["url"];
            if (!string.IsNullOrEmpty(Request.QueryString["rurl"]))
                TempData["rurl"] = Request.QueryString["rurl"];
            if (!string.IsNullOrEmpty(Request.QueryString["shareimg"]))
                TempData["shareimg"] = Request.QueryString["shareimg"];
            if (!string.IsNullOrEmpty(Request.QueryString["pagedesc"]))
                TempData["pagedesc"] = Request.QueryString["pagedesc"];
            if (TempData["ptype"]==null)
                TempData["ptype"] = 0;
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
		/// 获取用户信息
		/// </summary>
		/// <returns></returns>
		public ActionResult GetUserState()
        {
            var client = GetOpenAuthClient();

            if (!client.IsAuthorized)
            {
                return Json(new
                {
                    authorized = false,
                    url = client.GetAuthorizationUrl()
                });
            }

            // 调用获取获取用户信息api
            // 参考：http://open.weibo.com/wiki/2/users/show
            var response = client.HttpGet("users/show.json", new
            {
                uid = client.UID
            });

            if (response.IsSuccessStatusCode)
            {
                var json = new JObject();
                json["authorized"] = true;
                json["data"] = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                return Content(json.ToString(Formatting.None), "application/json");
            }
            else
            {
                var json = new JObject();
                json["authorized"] = false;
                json["data"] = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                json["authorized"] = true;
                return Content(json.ToString(Formatting.None), "application/json");
            }
        }

        /// <summary>
        /// 用户提交事件
        /// </summary>
        /// <returns></returns>
        public ActionResult OnSubmit()
        {
            string txbShare = Request["txbShare"];
            var client = GetOpenAuthClient();
            if (!client.IsAuthorized)
            {
                string state = "";
                state += TempData["siteid"];
                state += "|" + TempData["snsid"];
                state += "|" + TempData["url"];
                state += "|" +  TempData["shareimg"];
                state += "|" + HttpUtility.UrlEncode(Request["txbShare"]);
                state += "|" + TempData["rurl"];
                state += "|" + TempData["pagedesc"];
                state = "&state="+ HttpUtility.UrlEncode(state);
                return Redirect(client.GetAuthorizationUrl() + state);
            }
            return View();
        }

        /// <summary>
        /// 授权认证
        /// </summary>
        /// <param name="code">新浪返回的code</param>
        /// <returns></returns>
        public ActionResult Authorized(string code, string state)
        {
            TempData["ptype"] = 2;
            if (string.IsNullOrEmpty(code)||string.IsNullOrEmpty(state))
            {
                return RedirectToAction("Index");
            }

            state = HttpUtility.UrlDecode(state);
   
            int siteid = 0;
            int snsid = 0;
            string url = "";
            string sharepic = "";
            string sharecontent = "";
            string shareurl = "";
            string sharepagetitle = "";

            siteid = Convert.ToInt32(state.Split('|')[0]);
            snsid = Convert.ToInt32(state.Split('|')[1]);
            url = state.Split('|')[2];
            sharepic = state.Split('|')[3];
            sharecontent = state.Split('|')[4];
            shareurl = state.Split('|')[5];
            sharepagetitle = state.Split('|')[6];
            
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
                    var userjson = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    Site site = SiteService.GetSite(siteid);
                    //根据ID获得SNS名称
                    SNS sns = SNSService.GetSNS(snsid);
                    SNSUser user = CreateSNSUserBySina(userjson, site.ID, site.SiteName, sns.ID, sns.Name, client.UID);

                    //同步到微博
                    if (url.IndexOf("?") > -1)
                    {
                        url = url + "&yxkfrom=" + user.ID + "&yxksnsid=" + sns.ID;
                    }
                    else
                    {
                        url = url + "?yxkfrom=" + user.ID + "&yxksnsid=" + sns.ID;
                    }
                    long weiboid = PostWeibo(sharecontent + " " + url, client);
                    CreateShare(sharecontent, site, sns, user, weiboid, shareurl, sharepagetitle, sharepic);
                    TempData["ptype"] = 1;
                    return RedirectToAction("Index");


                    //    ////如果关注的话，执行微博官微API并将关注数据入库
                    //    //if (s.IsFollow)
                    //    //{
                    //    //    var rst = Sina.API.Friendships.Show("", user.NickName, "", Convert.ToString(Session["shareguanweiid"]));
                    //    //    if (!rst.target.followed_by)
                    //    //    {
                    //    //        Sina.API.Friendships.Create("", Convert.ToString(Session["shareguanweiid"]));
                    //    //        Follow fo = new Follow();
                    //    //        fo.CreateTime = DateTime.Now;
                    //    //        fo.PageTitle = s.PageTitle;
                    //    //        fo.SiteCategoryID = s.SiteCategoryID;
                    //    //        fo.SiteCategoryName = s.SiteCategoryName;
                    //    //        fo.SiteID = s.SiteID;
                    //    //        fo.SiteName = s.SiteName;
                    //    //        fo.SNSContentID = s.SNSContentID;
                    //    //        fo.SNSID = s.SNSID;
                    //    //        fo.SNSName = s.SNSName;
                    //    //        fo.SNSUserID = s.SNSUserID;
                    //    //        fo.SNSUserNickName = s.SNSUserNickName;
                    //    //        fo.URL = s.URL;

                    //    //        FollowService.CreateFollow(fo);
                    //    //    }
                    //    //}
                    //    //ConfigURL cfgurl = new ConfigURL();
                    //    //cfgurl = ConfigURLService.CreateAndUpdateConfigURL(s.SiteID, s.URL);



                    //    return RedirectToAction("Index");
                    //}


                }
                else
                    return RedirectToAction("Index");
             
            }

            return RedirectToAction("Index");
        }

        private int CreateShare(string sharecontent,Site site,SNS sns,SNSUser user, long weiboid,string shareurl,string sharepagetitle,string sharepic)
        {
            //根据URL来获得所属类别
            SiteCategory sc = SiteCategoryService.GetSiteCategory(site.ID, shareurl);
            //分享内容入库
            Share s = new Share();
            s.Content = sharecontent;
            s.CommentCount = 0;
            s.ForwardCount = 0;
            s.IsFollow = Convert.ToBoolean(Session["isfollow"]);
            s.SiteID = site.ID;
            s.SiteName = site.SiteName;
            s.SNSID = sns.ID;
            s.SNSName = sns.Name;
            s.SNSUserID = user.ID;
            s.SNSContentID = weiboid.ToString();
            s.SNSUserNickName = user.NickName;
            s.URL = shareurl;
            s.Vote = 0;
            s.SiteCategoryID = -1;
            s.SiteCategoryName = "";
            if (sc != null)
            {
                s.SiteCategoryID = sc.ID;
                s.SiteCategoryName = sc.CategoryName;
            }
            s.AgreeBeMember = true;
            s.PageTitle = sharepagetitle;
            s.Picture = sharepic;
            s.CreateTime = DateTime.Now;

            int sid = ShareService.CreateShare(s);
            if (sid > 0)
            {
                //分享内容入更新表
                ShareUpdate su = new ShareUpdate();
                su.ShareID = sid;
                su.SNSID = s.SNSID;
                su.SNSName = s.SNSName;
                su.SiteCategoryID = s.SiteCategoryID;
                su.SiteCategoryName = s.SiteCategoryName;
                su.SiteID = s.SiteID;
                su.SiteName = s.SiteName;
                su.SNSContentID = s.SNSContentID;
                su.CommentCount = 0;
                su.ForwardCount = 0;
                su.CreateTime = DateTime.Now;
                return ShareUpdateService.CreateShareUpdate(su);
            }
            return 0;
        }

        private long PostWeibo(string status,SinaWeiboClient client)
        {
            if (!client.IsAuthorized)
            {
                return 0;
            }
            // 参考：http://open.weibo.com/wiki/2/statuses/update
            var response = client.HttpPost("statuses/update.json", new
            {
                status = status
            });
            if (response.IsSuccessStatusCode)
            {
                var json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                return Convert.ToInt64(json["id"]);
            }
            else
            {
                return 0;
            }
        }
        private SNSUser CreateSNSUserBySina(JObject json,int siteid,string sitename,int snsid,string snsname,string snsuid)
        {
            //用户信息入库
            SNSUser user = new SNSUser();
            user.NickName = json["screen_name"].ToString();
            user.HeadImg = json["profile_image_url"].ToString();
            user.Email = "";
            user.Mobile = "";
            user.Birthday = Convert.ToDateTime("1900-01-01");
            user.SiteID = siteid;
            user.SiteName = sitename;
            user.SNSUID = snsuid;
            user.SNSName = snsname;
            user.SNSID = snsid;
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
             SNSManager.CreateSNSUser(user);
            return user;
        }

        /// <summary>
        /// 获取当前URL分享数量
        /// </summary>
        /// <returns></returns>
        public ActionResult GetURLCount(string url, string siteid)
        {
         
            string smallpic = "";
            string smallpicalt = "";
            string bigpic = "";
            string bigpicalt = "";
            string bigpiclink = "";
            string opentype = "click";
            string imagedomain = ConfigurationManager.AppSettings["ImageDomain"];
            if (url.IndexOf("yxkfrom") > -1)
            {
                url = url.Remove(url.IndexOf("yxkfrom") - 1);
            }

            ConfigURL cfgurl = new ConfigURL();
            cfgurl = ConfigURLService.GetConfigURL(int.Parse(siteid), url);
            if (cfgurl == null)
            {
                cfgurl = new ConfigURL();

                cfgurl = ConfigURLService.CreateAndUpdateConfigURL(int.Parse(siteid), url);
            }

            string snum = "0";
            if (cfgurl != null)
            {
                snum = Convert.ToString(cfgurl.ShareCount);
                if (cfgurl.ShareCount > 999)
                {
                    snum = Convert.ToString(cfgurl.ShareCount / 1000) + "K+";
                }
                if (cfgurl.SmallPic != "")
                {
                    smallpic = imagedomain + cfgurl.SmallPic;
                }
                smallpicalt = cfgurl.SmallPicAlt;
                if (cfgurl.BigPic != "")
                {
                    bigpic = imagedomain + cfgurl.BigPic;
                }
                bigpicalt = cfgurl.BigPicAlt;
                bigpiclink = cfgurl.BigPicLink;
                opentype = cfgurl.OpenType;
            }
            return Json(new { sharecount = snum });
        }

        /// <summary>
        /// 微信分享
        /// </summary>
        /// <returns></returns>
        public ActionResult ShareWeixin(string url, string siteid)
        {
            Site site = SiteService.GetSite(Convert.ToInt32(siteid));
            //根据ID获得SNS名称
            SNS sns = SNSService.GetSNS(5);
            SNSUser user = new SNSUser()
            {
                Birthday = Convert.ToDateTime("1900-01-01"),
                City = "未知",
                Email = "",
                FansCount=0,
                FollowCount=0,
                HeadImg="",
                Mobile="",
                NickName= "未知",
                Province= "未知",
                Sex=0,
                SiteID= site.ID,
                SiteName=site.SiteName,
                SNSID=5,
                SNSName="微信分享",
                SNSUID="",
                verified=true,
                CreateTime=DateTime.Now
            };
            SNSManager.CreateSNSUser(user);
            CreateShare("微信分享", site, sns, user, 0, url, "微信分享", "");
            return Json(true);
        }
    }
}