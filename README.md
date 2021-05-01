# AmazonRedirect

You may have been redirected here from az.sub-etha.link. If you don't know how you got here, check out my novel, Rebellion's Martyr: https://az.sub-etha.link/dp/B08ZBT27WR

This project is written in C# and targets dotnet 3.1.

Amazon do not have a means of redirecting customers to their local Amazon site automatically. That's good for the very niche use case of buying something in another country to send to someone in that country. And it's good for sellers to be able to see their products appearing as desired in other countries. But if I want people to be able to buy without having to take extra steps to get to the right Amazon site, I either need to run separate ads for each country, or find a way to redirect customers based on where they are.

This AWS Lambda code redirects the browser to the best available Amazon site (or just to .com if no best can be determined) based on the user's location. Often this is done with an IP Geolocation Database, but that means you need to have a file, which could contain errors, and you need to keep it up-to-date. The further issue is that since IPv6 is a much larger address space, such a file could grow significantly and have significantly more errors.

By using AWS CloudFront, we can instead determine the user's (most likely) local Amazon site by the CloudFront edge location they connect to. This will generally be accurate, obviates the need for IP Geolocation and works with IPv6. Whether it works properly with StarLink remains to be seen. In this circumstance we don't need to analyse ANY information about the user or their device or environment, not even their IP address. We only use information gleaned from best path routing.

If you want to use an Amazon Redirector like this, please make your own, with your own ASINs. Lambda functions aren't free, and while this doesn't cost me anything for the clicks I get, I might find myself with a big bill if everyone started using it.

To use this code:
1. (Probably not necessary) Make sure you have the Amazon Lambda Templates: `dotnet new --install Amazon.Lambda.Templates`
2. Install Lambda global tools: `dotnet tool install -g Amazon.Lambda.Tools` or upgrade an existing installation: `dotnet tool update -g Amazon.Lambda.Tools`
3. Make sure you have an S3 bucket for the binaries
4. `cd src/AmazonRedirect`
5. `dotnet lambda deploy-serverless`
	1. Fill in a name for your Stack
	2. Provide the name of the S3 bucket
6. Create a CloudFront distribution for the URL returned by the deployment process
7. Create a new Cache Policy here: https://console.aws.amazon.com/cloudfront/v2/home#/create/policy/cache_policy
	1. Give it a name like "CacheWithGeolocation"
	2. In the "Headers" dropdown, select "Whitelist"
	3. In the next dropdown, select "CloudFront-Viewer-Country"
	4. Click the "Add header" button
	5. Click "Create cache policy" at the bottom of the page.
8. In your new CloudFront Distribution, add a new Behaviour
	1. Click the "Behaviors" tab
	2. Click the "Create Behaviour" button
	3. In the "Cache Policy" dropdown, select your new "CacheWithGeolocation" policy
	4. Click the "Create" button at the bottom of the page.
9. Now you can use the domain name given to your cloudfront distribution (mine is `d2dp4n505tea78.cloudfront.net` for example) or get an SSL certificate and set up your own CNAME. I won't go into the fine detail of all that here, if you know how to do it, go do it, otherwise there's plenty of documentation online and every registrar is different, so I don't want a thousand bug reports because I don't know how your registrar works.

Of course you can add anything you like, such as logging requests to an S3 bucket, and adding parameters to help you see which of your campaigns are performing better.
