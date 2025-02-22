﻿using System;
using System.Threading.Tasks;
using Diagnostics.ModelsAndUtils.Models;
using Diagnostics.ModelsAndUtils.Models.ResponseExtensions;
using Diagnostics.RuntimeHost.Models;
using Diagnostics.RuntimeHost.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Configuration;

namespace Diagnostics.RuntimeHost.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route(UriElements.AppServiceDomainResource)]
    public sealed class AppServiceDomainController : DiagnosticControllerBase<AppServiceDomain>
    {
        public AppServiceDomainController(IServiceProvider services, IRuntimeContext<AppServiceDomain> runtimeContext, IConfiguration config)
            : base(services, runtimeContext, config)
        {
        }

        [HttpPost(UriElements.Query)]
        public async Task<IActionResult> ExecuteQuery(string subscriptionId, string resourceGroupName, string domainName, [FromBody]CompilationPostBody<dynamic> jsonBody, string startTime = null, string endTime = null, string timeGrain = null, [FromQuery][ModelBinder(typeof(FormModelBinder))] Form Form = null)
        {
            return await base.ExecuteQuery(GetResource(subscriptionId, resourceGroupName, domainName), jsonBody, startTime, endTime, timeGrain, Form: Form);
        }

        [HttpPost(UriElements.Detectors)]
        public async Task<IActionResult> ListDetectors(string subscriptionId, string resourceGroupName, string domainName, [FromBody] dynamic postBody, [FromQuery(Name = "text")] string text = null, [FromQuery] string l = "")
        {
            return await base.ListDetectors(GetResource(subscriptionId, resourceGroupName, domainName), text, language: l.ToLower());
        }

        [HttpPost(UriElements.Detectors + UriElements.DetectorResource)]
        public async Task<IActionResult> GetDetector(string subscriptionId, string resourceGroupName, string domainName, string detectorId, [FromBody] dynamic postBody, string startTime = null, string endTime = null, string timeGrain = null, [FromQuery][ModelBinder(typeof(FormModelBinder))] Form form = null, [FromQuery] string l = "")
        {
            return await base.GetDetector(GetResource(subscriptionId, resourceGroupName, domainName), detectorId, startTime, endTime, timeGrain, form: form, language: l.ToLower());
        }

        [HttpPost(UriElements.Detectors + UriElements.DetectorResource + UriElements.StatisticsQuery)]
        public async Task<IActionResult> ExecuteSystemQuery(string subscriptionId, string resourceGroupName, string domainName, [FromBody]CompilationPostBody<dynamic> jsonBody, string detectorId, string dataSource = null, string timeRange = null)
        {
            return await base.ExecuteQuery(GetResource(subscriptionId, resourceGroupName, domainName), jsonBody, null, null, null, detectorId, dataSource, timeRange);
        }

        [HttpPost(UriElements.Detectors + UriElements.DetectorResource + UriElements.Statistics + UriElements.StatisticsResource)]
        public async Task<IActionResult> GetSystemInvoker(string subscriptionId, string resourceGroupName, string domainName, string detectorId, string invokerId, string dataSource = null, string timeRange = null)
        {
            return await base.GetSystemInvoker(GetResource(subscriptionId, resourceGroupName, domainName), detectorId, invokerId, dataSource, timeRange);
        }

        [HttpPost(UriElements.Insights)]
        public async Task<IActionResult> GetInsights(string subscriptionId, string resourceGroupName, string domainName, [FromBody] dynamic postBody, string pesId, string supportTopicId = null, string supportTopic = null, string startTime = null, string endTime = null, string timeGrain = null)
        {
            string postBodyString;
            try
            {
                postBodyString = JsonConvert.SerializeObject(postBody.Parameters);
            }
            catch (RuntimeBinderException)
            {
                postBodyString = "";
            }
            return await base.GetInsights(GetResource(subscriptionId, resourceGroupName, domainName), pesId, supportTopicId, startTime, endTime, timeGrain, supportTopic, postBodyString);
        }

        /// <summary>
        /// Publish package.
        /// </summary>
        /// <param name="pkg">The package.</param>
        /// <returns>Task for publishing package.</returns>
        [HttpPost(UriElements.Publish)]
        public async Task<IActionResult> PublishPackageAsync([FromBody] Package pkg)
        {
            return await PublishPackage(pkg);
        }

        /// <summary>
        /// List all gists.
        /// </summary>
        /// <returns>Task for listing all gists.</returns>
        [HttpPost(UriElements.Gists)]
        public async Task<IActionResult> ListGistsAsync(string subscriptionId, string resourceGroupName, string domainName, [FromBody] dynamic postBody)
        {
            return await base.ListGists(GetResource(subscriptionId, resourceGroupName, domainName));
        }

        /// <summary>
        /// List the gist.
        /// </summary>
        /// <param name="subscriptionId">Subscription id.</param>
        /// <param name="resourceGroupName">Resource group name.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="gistId">Gist id.</param>
        /// <returns>Task for listing the gist.</returns>
        [HttpPost(UriElements.Gists + UriElements.GistResource)]
        public async Task<IActionResult> GetGistAsync(string subscriptionId, string resourceGroupName, string domainName, string gistId, [FromBody] dynamic postBody, string startTime = null, string endTime = null, string timeGrain = null)
        {
            return await base.GetGist(GetResource(subscriptionId, resourceGroupName, domainName), gistId, startTime, endTime, timeGrain);
        }
    }
}
