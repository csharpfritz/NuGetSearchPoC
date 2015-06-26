using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using NuGetSearchPoC.Core;
using Raven.Client;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NuGetSearchPoC.Web.Controllers
{
  public class SearchController : ApiController
  {

    private static string _apiKey;
    private static string _searchServiceName;
    private SearchIndexClient _IndexClient;
    private SearchServiceClient _SearchClient;
    private static string RavenUrl = "http://localhost:8080";

    static SearchController()
    {

      _searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
      _apiKey = ConfigurationManager.AppSettings["SearchServiceApiKey"];

    }

    public SearchController()
    {

      _SearchClient = new SearchServiceClient(_searchServiceName, new SearchCredentials(_apiKey));
      _IndexClient = new SearchIndexClient(_searchServiceName, "packages", new SearchCredentials(_apiKey));

    }

    // GET api/<controller>/5
    public string[] Get(string id)
    {
      //return AzureSearch(id);
      return RavenSearch(id);

    }

    private string[] AzureSearch(string id)
    {
      var sp = new SuggestParameters()
      {
        UseFuzzyMatching = true,
        Top = 10
      };

      ///  COnnect this to type-ahead
      var suggestions = _IndexClient.Documents.Suggest(id, "sg", sp);
      return suggestions.Select(i => i.Document["NuGetIdRaw"].ToString()).ToArray();
    }

    private string[] RavenSearch(string id)
    {

      using (var store = new DocumentStore
      {
        Url = RavenUrl,
        DefaultDatabase = "nuget_test"
      }.Initialize())
      {

        using (var session = store.OpenSession())
        {

          return session.Query<Package>("dynamic/Packages")
            .Where(p => p.Title.StartsWith(id) && p.IsLatestVersion == true)
            .Take(10)
            .Select(p => p.Title)
            .ToArray();


        }

      }

    }

  }

}