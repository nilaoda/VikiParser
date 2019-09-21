static string _APP_SECRET = "MM_d*yP@`&1@]@!AVrXf_o-HVEnoTnm$O-ti4[G~$JDI/Dc-&piU&z&5.;:}95=Iad";

private string GetM3u8Url(string mpdUrl)
{
    string str = mpdUrl;
    string m3u8 = str.Replace("manifest.mpd", "manifest.m3u8");
    string prefix = "https://";
    string[] arr = m3u8.Replace(prefix, "").Split('/');
    arr[5] = "m3u8";
    arr[9] = "m3u8hd";
    m3u8 = prefix + string.Join("/", arr);
    m3u8 = Regex.Replace(m3u8, @"tag=\w+:\w+:(?<str>\w+):.*cached", "tag=m3u8hd:normal:${str}:sourceVIKI:m3u8:cached");
    return m3u8;
}

private string GetToken(string userName, string password)
{
    string api = $"https://api.viki.io";
    string p1 = "/v4/sessions.json?app=100005a&t=1569053071&site=www.viki.com";
    api = api + p1 + "&sig=" + HmacSha1Sign(p1, _APP_SECRET);
    string data = "{\"password\": \"" + password + "\", \"login_id\": \"" + userName + "\"}";
    string json = Post(api, data);
    string token = JObject.Parse(json)["token"].ToString();
    return token;
}

private Dictionary<string, string> GetSubtitles(string vid, string text)
{
    Dictionary<string, string> dic = new Dictionary<string, string>();
    string api = $"https://www.viki.com/player5_fragment/{text}.{vid}?id={text}&il=zh&autofocus=false";
    string webSource = GetWebSource(api);
    Regex reg = new Regex(@"var parsedSubtitles = ([\s\S]*);\s*var parsedStreamSubtitles");
    JArray subs = JArray.Parse(reg.Match(webSource).Groups[1].Value.Trim());
    foreach (JObject item in subs)
    {
        dic.Add(Regex.Replace(text.Replace(vid + "-", "") + "_" + item["label"].ToString(), @"\W<span.*>(?<str>.*)</span>", "${str}"), item["src"].ToString());
    }
    return dic;
}

private string GetMpdUrl(string vid, string token = "")
{
    string api = $"https://api.viki.io";
    string p1 = $"/v4/videos/{vid}/streams.json?app=100005a&t={GetTimeStamp(true)}&site=www.viki.com" + (token == "" ? "" : $"&token={token}");
    api = api + p1 + "&sig=" + HmacSha1Sign(p1, _APP_SECRET);
    string webSorce = GetWebSource(api);
    //MessageBox.Show(webSorce);

    return JObject.Parse(webSorce)["mpd"]["http"]["url"].ToString();
}
