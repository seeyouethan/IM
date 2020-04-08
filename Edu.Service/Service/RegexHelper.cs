using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web;

namespace Edu.Service.Service
{
    public static class RegexHelper
    {
        /// <summary>   
        /// 取得HTML中所有图片的 URL。   
        /// </summary>   
        /// <param name="sHtmlText">HTML代码</param>   
        /// <returns>图片的URL列表</returns>   
        public static string[] GetHtmlImageUrlList(string sHtmlText)
        {
            // 定义正则表达式用来匹配 img 标签   
            Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);

            // 搜索匹配的字符串   
            MatchCollection matches = regImg.Matches(sHtmlText);
            int i = 0;
            string[] sUrlList = new string[matches.Count];

            // 取得匹配项列表   
            foreach (Match match in matches)
                sUrlList[i++] = match.Groups["imgUrl"].Value;
            return sUrlList;
        }

        //public static string[]  EmojiStrings = new string[] { "hehe", "haha", "tushe", "a", "ku", "lu", "kaixin", "han", "lei", "heixian", "bishi", "bugaoxing", "zhenbang", "qian", "yiwen", "yinxian", "tu", "yi", "weiqu", "huaxin", "hu", "xiaonian", "neng", "taikaixin", "huaji", "mianqiang", "kuanghan", "guai", "shuijiao", "jinku", "shengqi", "jinya", "pen", "aixin", "xinsui", "meigui", "liwu", "caihong", "xxyl", "taiyang", "qianbi", "dnegpao", "chabei", "dangao", "yinyue", "haha2", "shenli", "damuzhi", "ruo", "OK" };
        public static string[] EmojiStrings = new string[]
        {
            "呵呵","色","失望","惊讶","嘻嘻","亲亲","调皮","皱眉","失望","尴尬","大哭","吐舌","恼火","不开心","轻蔑","哈哈","生病","脸红","流汗","笑哭","微笑","鼻涕","鬼脸","惊恐","抱歉","眩晕","自信笑","惊吓","叹气","委屈","可以","小鬼","妖怪","圣诞","小狗","小猪","小猫","大拇指","大拇指下","拳头","拳头上","耶",
             "肌肉","鼓掌","向左","向上","向右","向下","OK","爱心","心碎","太阳","月亮","醒醒","雷电","乌云","红唇","玫瑰","咖啡","蛋糕","闹钟","啤酒","查询","手机","房屋","汽车","礼物","足球","炸弹","钻石","外星人","满分","现金","游戏","大便","SOS","睡觉","唱歌","雨伞","书本","祈祷","火箭","粥","西瓜"
        };

        /// <summary>
        /// 将Web端发送来是一连串的文本+图片消息转换为Dictionary队列，通过此步操作，就可以从返回的Dictionary中取要发送的消息，如果key
        /// 包含text则表示是文本消息（包括表情转义后的文本，接收方需要转义表情图标）；如果key包含img则是图片消息，value中存放的是图片的地址
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetMessageDictionaryQueue(string str)
        {
            var imglist = GetHtmlImageUrlList(str);
            var tempList = imglist.ToList();
            var dic = new Dictionary<string, string>();
            //第一个foreach将表情筛选出来转换为文本
            foreach (var s in imglist)
            {
                /*http://okcs.dev.cnki.net/im/Tookit/jQuery-emoji/img/emoji/84.png*/
                if (s.Contains("/Tookit/jQuery-emoji/img/emoji/"))
                {
                    tempList.Remove(s);
                    var emoijName = s.Substring(s.LastIndexOf("/") + 1).Replace(".png", "");
                    //表情
                    var emoijIndex = Convert.ToInt32(emoijName);
                    var emoijText = "#" + EmojiStrings[emoijIndex - 1] + "#";//用井号做占位符
                    str = str.Replace("<img class=\"emoji_icon\" src=\"" + s + "\">", emoijText);
                }
            }
            if (tempList.Count == 0)
                dic.Add("text" + dic.Count, str);
            else
            {
                //第二个foreach将表情和图片分离
                foreach (var img in tempList)
                {
                    var text = "";
                    var tempstr = "<img class=\"imgA\" src=\"" + img + "\">";
                    var index = str.IndexOf(tempstr);
                    if (index != -1)
                    {
                        text = str.Substring(0, index);
                        var zindex = text.Length + tempstr.Length;
                        if (!string.IsNullOrEmpty(text))
                        {
                            dic.Add("text" + dic.Count, text);
                        }
                        dic.Add("img" + dic.Count, img);
                        str = str.Substring(zindex);
                    }
                }
                if (!string.IsNullOrEmpty(str))
                    dic.Add("text" + dic.Count, str);
            }
            return dic;
        }

        public static string GetLinkString(string str)
        {
            try
            {
                string reg = @"<a[^>]*href=([""'])?(?<href>[^'""]+)\1[^>]*>";
                var item = Regex.Match(str, reg, RegexOptions.IgnoreCase);
                return item.Groups["href"].Value;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
