using System.Web;

namespace Nefarious.Common.Extensions;

public static class UriExtensions
{
    public static Uri AddParameters(this Uri url, IDictionary<string, string> parameters)
    {
        var uriBuilder = new UriBuilder(url);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        
        foreach (var key in parameters.Keys)
            query[key] = parameters[key];
        
        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }
}