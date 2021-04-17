using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
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

		private static readonly string[] supportedASINs = new[] {"B08ZBT27WR", "B08ZDFPG1B", "B091M7JTYG"};
		
		public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
		{
			string path = request.PathParameters.ContainsKey("path") ? request.PathParameters["path"] : "";

			string location;
			
			if (string.IsNullOrWhiteSpace(path) || !supportedASINs.Any(path.Contains))
			{

				return new APIGatewayProxyResponse {StatusCode = 403, Body = "PC LOAD LETTER"};
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