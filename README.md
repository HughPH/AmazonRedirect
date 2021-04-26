# AmazonRedirect

You may have been redirected here from az.sub-etha.link.

This AWS Lambda code redirects the browser to the best available Amazon site (or just to .com if no best can be determined) based on the user's location - which is determined by the cloudfront edge location they connect to. This will generally be accurate, obviates the need for IP Geolocation and works with IPv6.

If you don't know how you got here, check out my novel, Rebellion's Martyr: https://az.sub-etha.link/dp/B08ZBT27WR

If you want to use an Amazon Redirector like this, please make your own, with your own ASINs. Lambda functions aren't free, and while this doesn't cost me anything for the clicks I get, I might find myself with a big bill if everyone started using it.

To use this code:
1. Make sure you have the Amazon Lambda Templates: `dotnet new --install Amazon.Lambda.Templates::5.2.0`
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
