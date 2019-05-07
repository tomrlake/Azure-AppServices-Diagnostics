﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Diagnostics.DataProviders;
using Diagnostics.Logger;
using Diagnostics.RuntimeHost.Services;
using Diagnostics.RuntimeHost.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Diagnostics.RuntimeHost.Middleware
{
    public class DiagnosticsRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public DiagnosticsRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            Exception exception = null;
            int statusCode = 0;
            GenerateMissingRequestHeaders(ref httpContext);
            BeginRequest_Handle(httpContext);

            try
            {
                await _next(httpContext);
            }
            catch (TimeoutException ex)
            {
                exception = ex;
                statusCode = (int)HttpStatusCode.RequestTimeout;
            }
            catch (Exception ex)
            {
                exception = ex;
                statusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (exception != null && !httpContext.Response.HasStarted)
                {
                    httpContext.Response.Clear();
                    httpContext.Response.StatusCode = statusCode;
                    LogException(httpContext, exception);
                }

                EndRequest_Handle(httpContext);
            }

            return;
        }

        private void BeginRequest_Handle(HttpContext httpContext)
        {
            var logger = new ApiMetricsLogger(httpContext);
            var cTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(HostConstants.TimeoutInMilliSeconds));

            httpContext.RequestAborted = cTokenSource.Token;
            httpContext.Request.Headers.TryGetValue(HeaderConstants.RequestIdHeaderName, out StringValues values);
            var queryStringValues = httpContext.Request.Query.ToDictionary(query => query.Key.ToLower(), query => query.Value.FirstOrDefault());

            DateTimeHelper.PrepareStartEndTimeWithTimeGrain(
                queryStringValues.GetValueOrDefault("starttime", null),
                queryStringValues.GetValueOrDefault("endtime", null),
                queryStringValues.GetValueOrDefault("timegrain", null),
                out DateTime startTimeUtc,
                out DateTime endTimeUtc,
                out TimeSpan timeGrainTimeSpan,
                out string errorMessage);

            var dataSourcesConfigurationService = ((ServiceProvider)httpContext.RequestServices).GetService<IDataSourcesConfigurationService>();
            var wawsObserverTokenService = ((ServiceProvider)httpContext.RequestServices).GetService<IWawsObserverTokenService>();
            var supportBayApiObserverTokenService = ((ServiceProvider)httpContext.RequestServices).GetService<ISupportBayApiObserverTokenService>();

            httpContext.Items.Add(HostConstants.ApiLoggerKey, logger);
            var clientObjId = string.Empty;
            var isInternalClient = false;

            // For requests coming Applens, populate client object id with Applens App Id.
            httpContext.Request.Headers.TryGetValue("x-ms-internal-client", out var internalClientHeader);
            if (internalClientHeader.Any())
            {
                bool.TryParse(internalClientHeader.First(), out isInternalClient);
            }

            if (isInternalClient)
            {
                clientObjId = dataSourcesConfigurationService.Config.ChangeAnalysisDataProviderConfiguration.ClientId;
            }
            else if (httpContext.Request.Headers.ContainsKey("x-ms-client-object-id"))
            {
                clientObjId = httpContext.Request.Headers["x-ms-client-object-id"];
            }

            string clientPrincipalName = string.Empty;
            if (httpContext.Request.Headers.ContainsKey("x-ms-client-principal-name"))
            {
                clientPrincipalName = httpContext.Request.Headers["x-ms-client-principal-name"];
            }

            httpContext.Items.Add(HostConstants.DataProviderContextKey, new DataProviderContext(dataSourcesConfigurationService.Config, values.FirstOrDefault() ?? string.Empty, cTokenSource.Token, startTimeUtc, endTimeUtc, wawsObserverTokenService, supportBayApiObserverTokenService, clientObjId, clientPrincipalName));
        }

        private void EndRequest_Handle(HttpContext httpContext)
        {
            var logger = (ApiMetricsLogger)httpContext.Items[HostConstants.ApiLoggerKey] ?? new ApiMetricsLogger(httpContext);

            logger.LogRuntimeHostAPIMetrics(httpContext);
        }

        private void LogException(HttpContext context, Exception ex)
        {
            try
            {
                var logger = (ApiMetricsLogger)context.Items[HostConstants.ApiLoggerKey] ?? new ApiMetricsLogger(context);

                logger.LogRuntimeHostUnhandledException(context, ex);
            }
            catch (Exception logEx)
            {
                string requestId = string.Empty;
                if (context.Request.Headers.TryGetValue(HeaderConstants.RequestIdHeaderName, out StringValues values) && values != default(StringValues) && values.Any())
                {
                    requestId = values.First().ToString();
                }

                DiagnosticsETWProvider.Instance.LogRuntimeHostUnhandledException(
                    requestId,
                    "LogException_DiagnosticsRequestMiddleware",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    logEx.GetType().ToString(),
                    logEx.ToString());
            }
        }

        private void GenerateMissingRequestHeaders(ref HttpContext httpContext)
        {
            if (!httpContext.Request.Headers.TryGetValue(HeaderConstants.RequestIdHeaderName, out StringValues values)
                || values == default(StringValues) || !values.Any() || string.IsNullOrWhiteSpace(values.First()))
            {
                httpContext.Request.Headers[HeaderConstants.RequestIdHeaderName] = Guid.NewGuid().ToString();
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class DiagnosticsRequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseDiagnosticsRequestMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DiagnosticsRequestMiddleware>();
        }
    }
}
