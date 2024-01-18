> This repository is deprecated. Please using the updated client tools and templates found [here](https://github.com/intelligentplant/IndustrialAppStore.ClientTools.DotNet).
 
# Getting Started

## Registering as an App Store Developer

Before you can create App Store applications, you must be a registered App Store developer.  To register as a developer, visit https://appstore.intelligentplant.com, sign in, and click on the "Get Developing" link under the My Account menu.

Once you have accepted the terms and conditions, you will be able to register your applications with the App Store.

## Add the App Store NuGet Feed to Visual Studio

The repository contains a `NuGet.config` file that enables this project to use the App Store's NuGet package source automatically.

To enable the package source in all Visual Studio projects, follow these steps:

1. Open the `Tools > NuGet Package Manager > Package Manager Settings` item in the menu.
2. Add a package source that points to `https://appstore.intelligentplant.com/NuGet/nuget`. 

## Register Your Application with the App Store

Visit the [App Store developer home page](https://appstore.intelligentplant.com/Developer/Home) to register your application.

Register the `/signin-ip` route as an authorized redirect URL for the application.  This will allow the App Store to authenticate sign in requests made by your application.  For example, if the base URL for your application is `http://localhost:12345`, you should add `http://localhost:12345/signin-ip` as an authorized redirect URL.
If you would like to test debiting api (more info here https://appstore.intelligentplant.com/wiki/doku.php?id=dev:app_store_developers) make sure to set you application status to "Pending".

## Update Web.config

Open `Web.config` and update the `appStore:clientId` and `appStore:clientSecret` settings in the `appSettings` section to use the application ID and application secret generated when you registered the application with the App Store.

## Run Your Application

Press F5 to compile and run the application.  

On the home page for your application, you will be prompted to sign in.

When you click the login button, you will be redirected to the App Store and presented with the App Store consent screen for the application.  Select the data sources that the application will be authorized to access.

If you are not currently logged into the App Store, you will be prompted to sign in before the consent screen is displayed.

Once you have granted consent, you will be redirected back to your application.  Your application contains a basic data viewer, that can be used to browse the App Store data sources that you have granted consent to, and to view the current values of tags that you select.

## Logging

The project template is automatically configured to use [Common.Logging](http://net-commons.github.io/common-logging/), using the [log4net](https://logging.apache.org/log4net/) adapter by default (i.e. all calls to `Common.Logging` will be redirected to `log4net` under the hood).  

By default, log files will be written to the application's `App_Data\Logs` folder.

If you prefer to use a different logging library (e.g. [NLog](http://nlog-project.org/)), you can install the appropriate NuGet package(s) and configure the `Common.Logging` adapter in `Web.config`.

## Notes

* To persist OAuth access tokens, create your own implementation of the `IOAuthTokenStore` interface and replace the `IOAuthTokenStore` registration at the start of `ConfigureAppStoreAuthentication` in `Startup.Auth.cs`.
* The Intelligent Plant login provider middleware is used to automatically handle sign-in and sign-out requests. You can remove this middleware registration in `Startup.Auth.cs` if you intend to use ASP.NET Identity to manage user accounts for your application.
* By default, the Intelligent Plant login provider middleware configures session cookies to expire at the same time as the OAuth access tokens you receive from App Store. If you request offline access for your app (i.e. you receive refresh tokens in addition to access tokens), you should modify how the session cookie expiry is set.
