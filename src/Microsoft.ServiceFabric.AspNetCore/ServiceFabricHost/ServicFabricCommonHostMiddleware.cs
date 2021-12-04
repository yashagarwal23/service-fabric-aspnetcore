// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public class ServicFabricCommonHostMiddleware
    {
        private readonly RequestDelegate next;
        private IList<string> urlSuffixes;

        public ServicFabricCommonHostMiddleware(RequestDelegate next)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }

            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.urlSuffixes = ServiceRegistrant.GetAll().ToList();

            foreach (var urlSuffix in this.urlSuffixes)
            {
                if (context.Request.Path.StartsWithSegments(urlSuffix, out var matchedPath, out var remainingPath))
                {
                    // All good, change Path, PathBase and call next middleware in the pipeline
                    var originalPath = context.Request.Path;
                    var originalPathBase = context.Request.PathBase;
                    context.Request.Path = remainingPath;
                    context.Request.PathBase = originalPathBase.Add(matchedPath);

                    var previousServiceProvider = context.RequestServices;

                    try
                    {
                        // TODO find the right place
                        context.RequestServices = ServiceRegistrant.Get(urlSuffix).CreateScope().ServiceProvider;
                        await this.next(context);
                    }
                    finally
                    {
                        context.Request.Path = originalPath;
                        context.Request.PathBase = originalPathBase;
                        context.RequestServices = previousServiceProvider;
                    }

                    return;
                }
            }

            context.Response.StatusCode = StatusCodes.Status410Gone;
            return;
        }
    }
}
