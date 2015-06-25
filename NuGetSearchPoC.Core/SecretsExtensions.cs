using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NuGetSearchPoC.Core
{
  public static class SecretsExtensions
  {

    public static void AddSearchSecrets()
    {

      var configMap = new ExeConfigurationFileMap();

      if (HttpContext.Current != null)
      {
        configMap.ExeConfigFilename = HttpContext.Current.Server.MapPath("~/azuresearch.config");
      }
      else
      {
        configMap.ExeConfigFilename = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "azuresearch.config");
      }

      // Exit now if the config file is not present
      if (!File.Exists(configMap.ExeConfigFilename)) return;


      var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);


      foreach (var key in config.AppSettings.Settings.AllKeys)
      {
        ConfigurationManager.AppSettings.Set(key, config.AppSettings.Settings[key].Value);
      }

    }

  }
}
