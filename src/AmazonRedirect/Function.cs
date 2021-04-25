using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AmazonRedirect
{
	public class Functions
	{
		private static readonly string[] toLowerCountries = new[] {"DE", "FR", "ES", "IT", "JP", "CA", "CN", "IN", "SG", "AE",
			                                                          "SA", "NL", "PL", "SE", "AU", "BR", "MX", "TR"};
		private static readonly string[] comCountries = new[] {"AU", "BR", "MX", "TR", "US"};

		private static readonly string[] supportedASINs = new[] {"dp/B08ZBT27WR", "dp/B08ZDFPG1B", "Hugh-Phoenix-Hulme/e/B091M7JTYG"};

		private static readonly Dictionary<string, Tuple<string, string>> redirections = new Dictionary<string, Tuple<string, string>>
		                                                                 {
			                                                                 {"AE", new Tuple<string, string>(supportedASINs[0], supportedASINs[1])},
			                                                                 {"SG", new Tuple<string, string>(supportedASINs[0], supportedASINs[1])},
			                                                                 {"TR", new Tuple<string, string>(supportedASINs[0], supportedASINs[1])},
			                                                                 {"PL", new Tuple<string, string>(supportedASINs[0], supportedASINs[1])},
			                                                                 {"SE", new Tuple<string, string>(supportedASINs[0], supportedASINs[1])},
		                                                                 };
		
		public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
		{
			string path = request.PathParameters.ContainsKey("path") ? request.PathParameters["path"] : "";

			string location;
			
			if (string.IsNullOrWhiteSpace(path) || !supportedASINs.Any(path.Contains))
			{
				location = $"https://github.com/HughPH/AmazonRedirect";
			}
			else
			{
				string tld = ".com";

				if (request.Headers.ContainsKey("cloudfront-viewer-country"))
				{
					string countryCode = request.Headers["cloudfront-viewer-country"];
					if (Array.IndexOf(comCountries, countryCode) == -1) tld = "";
					if (Array.IndexOf(toLowerCountries, countryCode) >= 0) tld += $".{countryCode.ToLower()}";
					if (countryCode == "GB") tld = ".co.uk"; // always got to be the special case

					if (redirections.ContainsKey(countryCode) && path == redirections[countryCode].Item1) path = redirections[countryCode].Item2;
				}

				if (tld == "") tld = ".com";
				location = $"https://amazon{tld}/{path}";
			}
			
			return new APIGatewayProxyResponse
			       {
				       StatusCode = (int) HttpStatusCode.Found,
				       Headers = new Dictionary<string, string> {{"Location", location}}
			       };
		}
	}
}