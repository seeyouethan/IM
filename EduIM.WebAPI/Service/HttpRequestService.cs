using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Edu.Service.Service;
using Edu.Tools;

namespace EduIM.WebAPI.Service
{
    public class HttpRequestService
    {
        public static List<string> GetWorkGroupMembers(string groupid)
        {
            var url = ConfigHelper.GetConfigString("GetWorkGroupMembers") + groupid;
            var resp = HttpWebHelper.HttpGet(url);
            var list = new List<string>();
            resp = resp.Replace("[", "").Replace("]", "");
            var arry = resp.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in arry)
            {
                list.Add(s.Replace("\"", ""));
            }
            return list;
        }

        public static List<string> GetSelfGroupMembers(string groupid)
        {
            var url = ConfigHelper.GetConfigString("GetSelfGroupMembers") + groupid;
            var resp = HttpWebHelper.HttpGet(url);
            var list = new List<string>();
            resp = resp.Replace("[", "").Replace("]", "");
            var arry = resp.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in arry)
            {
                list.Add(s.Replace("\"", ""));
            }
            return list;
        }
    }
}