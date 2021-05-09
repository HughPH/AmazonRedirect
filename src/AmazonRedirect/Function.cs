using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AmazonRedirect
{
	public class Functions
	{
		private static readonly string[] ToLowerCountries = new[]
		                                                    {
			                                                    "DE", "FR", "ES", "IT", "JP", "CA", "CN", "IN", "SG", "AE",
			                                                    "SA", "NL", "PL", "SE", "AU", "BR", "MX", "TR"
		                                                    };

		private static readonly string[] ComCountries = new[] {"AU", "BR", "MX", "TR", "US"};

		private static readonly string[] SupportedASINs = new[] {"dp/B08ZBT27WR", "dp/B09483MB61", "dp/B08ZDFPG1B", "Hugh-Phoenix-Hulme/e/B091M7JTYG"};

		private static readonly Dictionary<string, string> Tags = new Dictionary<string, string>
		                                                          {
			                                                          {"GB", "hpph-21"},
			                                                          {"US", "hpph-20"}
		                                                          };
		
		private static readonly Dictionary<(string, int), int> Redirections = new Dictionary<(string, int), int>
		                                                                      {
			                                                                      {("AE", 0), 1},
			                                                                      {("SG", 0), 1},
			                                                                      {("TR", 0), 1},
			                                                                      {("PL", 0), 1},
			                                                                      {("SE", 0), 1},
			                                                                      {("**", 2), 1},
		                                                                      };

		public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
		{
			string path = request.PathParameters.ContainsKey("path") ? request.PathParameters["path"] : "";
			string asin = string.IsNullOrWhiteSpace(path) ? null : SupportedASINs.FirstOrDefault(path.Contains);

			string location;

			if (asin == null)
			{
				location = $"https://github.com/HughPH/AmazonRedirect";
			}
			else
			{
				string tld = "";
				string tag = null;
				if (request.Headers.ContainsKey("cloudfront-viewer-country"))
				{
					string countryCode = request.Headers["cloudfront-viewer-country"];
					if (ComCountries.Contains(countryCode)) tld = ".com";
					if (ToLowerCountries.Contains(countryCode)) tld += $".{countryCode.ToLower()}";
					if (countryCode == "GB") tld = ".co.uk"; // always got to be the special case

					var key = (countryCode, Array.IndexOf(SupportedASINs, asin));

					if (Redirections.ContainsKey(key))
						path = SupportedASINs[Redirections[key]];

					var key2 = ("**", key.Item2);
					if (Redirections.ContainsKey(key2))
						path = SupportedASINs[Redirections[key2]];

					if (Tags.ContainsKey(countryCode)) tag = Tags[countryCode];
				}

				if (tld == "") tld = ".com";
				
				location = $"https://amazon{tld}/{path}";

				if (!string.IsNullOrWhiteSpace(tag)) location += $"?tag={tag}";

			}

			return new APIGatewayProxyResponse
			       {
				       StatusCode = (int) HttpStatusCode.Found,
				       Headers = new Dictionary<string, string> {{"Location", location}}
			       };
		}
	}
}