using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetSearchPoC.Core
{


  public class Package
  {
    public string Id { get; set; }

    public string NuGetId
    {
      get { return this.ToString(); }
    }

    public string Version { get; set; }
    public string NormalizedVersion { get; set; }
    public string Authors { get; set; }
    public string Copyright { get; set; }
    public DateTime Created { get; set; }
    public object[] Dependencies { get; set; }
    public string Description { get; set; }
    public int DownloadCount { get; set; }
    public string GalleryDetailsUrl { get; set; }
    public object IconUrl { get; set; }
    public bool IsLatestVersion { get; set; }
    public bool IsAbsoluteLatestVersion { get; set; }
    public bool IsPrerelease { get; set; }
    public string Language { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime Published { get; set; }
    public string PackageHash { get; set; }
    public string PackageHashAlgorithm { get; set; }
    public string PackageSize { get; set; }
    public string ProjectUrl { get; set; }
    public string ReportAbuseUrl { get; set; }
    public object ReleaseNotes { get; set; }
    public bool RequireLicenseAcceptance { get; set; }
    public string Summary { get; set; }
    public string[] Tags { get; set; }
    public string Title { get; set; }
    public int VersionDownloadCount { get; set; }
    public object MinClientVersion { get; set; }
    public object LastEdited { get; set; }
    public string LicenseUrl { get; set; }
    public string LicenseNames { get; set; }
    public string LicenseReportUrl { get; set; }

    public override string ToString()
    {

      return Id.Split('/')[1];

    }

  }

}
