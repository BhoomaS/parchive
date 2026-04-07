using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CH.Business.Services
{
  public interface ITableauService
  {
    Task<byte[]> GetSheetPdfBytes(string workbookUri, string sheetUri, bool forceRefresh,
      IEnumerable<KeyValuePair<string, object>> parameters);
  }

  public class TableauService : ITableauService
  {
    private readonly IConfiguration _config;

    public TableauService(IConfiguration config)
    {
      _config = config;
    }

    public async Task<byte[]> GetSheetPdfBytes(string workbookUri,
      string sheetUri, bool forceRefresh,
      IEnumerable<KeyValuePair<string, object>> parameters)
    {
      const string userAgentHeader =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36";

      string tableauTicket = "-1"; // Base case of failed request for ticket

      for (int i = 0; i < 3; i++)
      {
        try
        {
          // Try this 3 times with a slight delay between tries
          tableauTicket = GetAuthenticationTicket();
          if (!string.IsNullOrWhiteSpace(tableauTicket) && tableauTicket != "-1")
            break;
          // Sleep for just a second in case that helps...
          System.Threading.Thread.Sleep(1000);
        }
        catch (Exception e)
        { /* Do nothing */ }
      }

      if (tableauTicket == "-1")
        //throw new Exception($"Could not get ticket to {workbookUri}");
        throw new Exception($"Could not get ticket");

      string relativeUri = ConcatenateUri(workbookUri, sheetUri);
      string viewUrl = GetTableauTrustedViewUrl(tableauTicket, relativeUri);

      if (!viewUrl.EndsWith(".pdf"))
        viewUrl += ".pdf";

      if (forceRefresh)
      {
        if (parameters == null)
          parameters = new List<KeyValuePair<string, object>>();
        string refreshKey = ":refresh";
        if (!parameters.Any(o => o.Key == refreshKey))
          parameters = parameters.Append(new KeyValuePair<string, object>(refreshKey, "yes"));
      }

      if (parameters != null && parameters.Any())
      {
        var paramStrings = parameters.Select(o => $"{o.Key}={o.Value}");
        viewUrl += $"?{string.Join('&', paramStrings)}";
      }

      try
      {
        Console.WriteLine(viewUrl);

        var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(viewUrl);

        // Ignore SSL certificate errors
        request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

        request.Method = "GET";
        // This next line is required so that the automatic redirect contains the Tableau session cookie.
        //request.CookieContainer = request1.CookieContainer;
        request.CookieContainer = new System.Net.CookieContainer(10);

        request.Headers = new WebHeaderCollection();
        request.Headers.Add(HttpRequestHeader.UserAgent, userAgentHeader);

        var response = (System.Net.HttpWebResponse)request.GetResponse();
        if (response.StatusCode != System.Net.HttpStatusCode.OK ||
          !response.ContentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
          throw new Exception(string.Format("Invalid response from Tableau (status = {0}, contentType = {1})",
            response.StatusCode, response.ContentType));
        }

        // Loop through the responseStream until it's empty.
        byte[] rgb = new byte[5000];
        using (var responseStream = response.GetResponseStream())
        using (var ms = new MemoryStream(rgb.Length))
        {
          int bytesRead = responseStream.Read(rgb, 0, rgb.Length);
          while (bytesRead > 0)
          {
            ms.Write(rgb, 0, bytesRead);
            bytesRead = responseStream.Read(rgb, 0, rgb.Length);
          }
          if (ms.Length == 0)
            throw new Exception("Empty response from Tableau");

          return ms.ToArray();
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw;
      }
    }

    private string GetAuthenticationTicket()
    {
      string ticketUrl = string.Format($"{_config.GetTableauServerUrl()}/trusted");

      var postData = string.Format("username={0}", _config.GetTableauUsername());
      if (!string.IsNullOrWhiteSpace(_config.GetTableauSiteId()))
        postData = string.Format("{0}&target_site={1}", postData, _config.GetTableauSiteId());

      byte[] data = Encoding.UTF8.GetBytes(postData);

      var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(ticketUrl);

      // Ignore SSL certificate errors
      request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

      request.Method = "POST";
      request.ContentType = "application/x-www-form-urlencoded";
      request.ContentLength = data.Length;

      using (var requestStream = request.GetRequestStream())
        requestStream.Write(data, 0, data.Length);

      var response = (System.Net.HttpWebResponse)request.GetResponse();
      using (var responseStream = new StreamReader(response.GetResponseStream()))
        return responseStream.ReadToEnd();
    }

    private string GetTableauTrustedViewUrl(string tableauTicket, string viewUri)
    {
      string uri = ConcatenateUri(_config.GetTableauServerUrl(), $"trusted/{tableauTicket}");

      if (!string.IsNullOrWhiteSpace(_config.GetTableauSiteId()))
        uri = ConcatenateUri(uri, $"/t/{_config.GetTableauSiteId()}");

      return ConcatenateUri(uri, viewUri);
    }

    private string ConcatenateUri(string baseUri, string relativeUri)
    {
      return $"{(baseUri ?? string.Empty).Trim('/')}/{(relativeUri ?? string.Empty).Trim('/')}";
    }


  }
}
