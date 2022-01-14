// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.EventLog;

    public class ServiceBuilder : HostBuilder
    {
        internal void ConfigureDefaults()
        {
            this.ConfigureDefaults(null);
        }

        internal void ConfigureDefaults(string[] args)
        {
#if NET6_0
            this.ConfigureDefaults(args);
#else

            this.UseContentRoot(Directory.GetCurrentDirectory());
            this.ConfigureHostConfiguration(config =>
            {
                config.AddEnvironmentVariables(prefix: "DOTNET_");
                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            });

            this.ConfigureAppConfiguration((hostingContext, config) =>
            {
                IHostEnvironment env = hostingContext.HostingEnvironment;

                bool reloadOnChange = hostingContext.Configuration.GetValue("hostBuilder:reloadConfigOnChange", defaultValue: true);

                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: reloadOnChange)
                      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: reloadOnChange);

                if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                {
                    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                    if (appAssembly != null)
                    {
                        config.AddUserSecrets(appAssembly, optional: true);
                    }
                }

                config.AddEnvironmentVariables();

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                // IMPORTANT: This needs to be added *before* configuration is loaded, this lets
                // the defaults be overridden by the configuration.
                if (isWindows)
                {
                    // Default the EventLogLoggerProvider to warning or above
                    logging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning);
                }

                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
                logging.AddEventSourceLogger();

                if (isWindows)
                {
                    // Add the EventLogLoggerProvider on windows machines
                    logging.AddEventLog();
                }
#if NET5_0
                logging.Configure(options =>
                {
                    options.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
                                                        | ActivityTrackingOptions.TraceId
                                                        | ActivityTrackingOptions.ParentId;
                });
#endif
            })
            .UseDefaultServiceProvider((context, options) =>
            {
                bool isDevelopment = context.HostingEnvironment.IsDevelopment();
                options.ValidateScopes = isDevelopment;
                options.ValidateOnBuild = isDevelopment;
            });
#endif
            }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
