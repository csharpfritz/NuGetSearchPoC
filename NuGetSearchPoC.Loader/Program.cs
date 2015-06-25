using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using NuGetSearchPoC.Core;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetSearchPoC.Loader
{
  class Program
  {

    private static string RavenUrl = "http://localhost:8080";
    private static SearchServiceClient _SearchClient;
    private static SearchIndexClient _IndexClient;
    private static string _searchServiceName;
    private static string _apiKey;

    static void Main(string[] args)
    {

      SecretsExtensions.AddSearchSecrets();

      var packagesToLoad = GetPackagesFromRavenDb();

      LoadPackagesIntoAzureSearch(packagesToLoad);

      Console.ReadLine();

    }

    private static void LoadPackagesIntoAzureSearch(IEnumerable<Package> packagesToLoad)
    {

      _searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
      _apiKey = ConfigurationManager.AppSettings["SearchServiceApiKey"];

      _SearchClient = new SearchServiceClient(_searchServiceName, new SearchCredentials(_apiKey));
      _IndexClient = new SearchIndexClient(_searchServiceName, "packages", new SearchCredentials(_apiKey));

      Suggester sg = new Suggester();
      sg.Name = "sg";
      sg.SearchMode = SuggesterSearchMode.AnalyzingInfixMatching;
      sg.SourceFields = new List<string> { "NuGetIdRaw", "NuGetIdCollection" };

      var indexDefinition = new Index()
      {
        Name = "packages",
        Suggesters = new List<Suggester> { sg },
        Fields = new[]
        {
          new Field("NuGetIdRaw", DataType.String) { IsKey=false, IsSearchable=true, IsSortable=true, IsRetrievable=true },
          new Field("NuGetIdCollection", DataType.Collection(DataType.String)) { IsKey=false, IsSearchable=true, IsSortable=false, IsRetrievable=true, IsFacetable=true, IsFilterable=true },
          new Field("NuGetId", DataType.String) { IsKey=true, IsSearchable=false, IsSortable=false, IsRetrievable=true }
        }
      };

      // NOTE: ALready exists
      _SearchClient.Indexes.Delete("packages");
      _SearchClient.Indexes.Create(indexDefinition);


      // Populate
      try
      {

        for (var i = 0; i < packagesToLoad.Count(); i += 1000)
        {

          _IndexClient.Documents.Index(
            IndexBatch.Create(
              packagesToLoad.Skip(i).Take(1000).Select(
                doc => IndexAction.Create(new
                {
                  NuGetId = Guid.NewGuid(),
                  NuGetIdRaw = doc.NuGetId,
                  NuGetIdCollection = doc.NuGetId.Split('.')
                })
              )
            )
          );

        }
      }
      catch (IndexBatchException e)
      {
        // Sometimes when your Search service is under load, indexing will fail for some of the documents in
        // the batch. Depending on your application, you can take compensating actions like delaying and
        // retrying. For this simple demo, we just log the failed document keys and continue.
        Console.WriteLine(
            "Failed to index some of the documents: {0}",
            String.Join(", ", e.IndexResponse.Results.Where(r => !r.Succeeded).Select(r => r.Key)));
      }

      Console.Out.WriteLine("Completed");


    }

    private static IEnumerable<Package> GetPackagesFromRavenDb()
    {

      using (var store = new DocumentStore
      {
        Url = RavenUrl,
        DefaultDatabase = "nuget_test"
      }.Initialize())
      {

        List<Package> packages = new List<Package>();
        var lastCount = -1;
        //packages = store.DatabaseCommands.StartsWith("Packages/Microsoft.AspNet.", "*", 0, 100);

        while (true)
        {

          using (var session = store.OpenSession())
          {

            packages.AddRange(session.Query<Package>("Auto/Packages/ByAuthorsAndDownloadCountAndIdAndIsLatestVersionAndIsPrerelease")
              .Where(p => p.IsLatestVersion == true)
              .OrderByDescending(p => p.DownloadCount)
              .Skip(packages.Count())
              .Take(1000)
              .ToList());

            if (lastCount == packages.Count) break;

            Console.Out.WriteLine("Collected {0} packages", packages.Count());

            lastCount = packages.Count();

          }
        }

        packages.ForEach(p => Console.Out.WriteLine("Sending package '{0}'", p.ToString()));
        Console.WriteLine("Writing {0} packages to Azure Search", packages.Count());

        return packages;


      }

    }

    public static string Base64Encode(string plainText)
    {
      var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
      return System.Convert.ToBase64String(plainTextBytes);
    }
  }


}
