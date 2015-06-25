using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuGetSearchPoC.Web.Controllers
{
  public class HomeController : Controller
  {
    private string _apiKey;
    private SearchIndexClient _IndexClient;
    private SearchServiceClient _SearchClient;
    private string _searchServiceName;

    public ActionResult Index()
    {
      return View();
    }

    public ActionResult Search()
    {

      _searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
      _apiKey = ConfigurationManager.AppSettings["SearchServiceApiKey"];

      _SearchClient = new SearchServiceClient(_searchServiceName, new SearchCredentials(_apiKey));
      _IndexClient = new SearchIndexClient(_searchServiceName, "packages", new SearchCredentials(_apiKey));

      SuggestParameters sp = new SuggestParameters()
      {
        UseFuzzyMatching=true,
        Top=10
      };

      ///  COnnect this to type-ahead
      var suggestions = _IndexClient.Documents.Suggest("Cors", "sg", sp);

      return View(suggestions.Results);
      

    }

  }
}