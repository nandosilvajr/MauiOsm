using Itinero.Profiles;
using MauiMaps.Models;
using MauiMapsui.Shared.Models;
using Refit;

namespace MauiMaps.Services
{
    [Headers("Content-Type: application/json", "Accept: application/json")]
    public interface IItineroServiceAPI
    {
        //http://192.168.1.199:5000/portugal/routing?profile=car&loc=38.8329161,-9.1545638&loc=38.7769792,-9.1858319
        [Get("/portugal/routing")]
        [QueryUriFormat(UriFormat.Unescaped)]
        [Headers("Content-Type: application/json")]
        Task<ItineroRoute> GetRoute(ItineroParams @params);
    }
}   