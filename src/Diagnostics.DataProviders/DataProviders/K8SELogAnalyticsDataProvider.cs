﻿using Diagnostics.DataProviders.DataProviderConfigurations;
using Diagnostics.DataProviders.Interfaces;
using Diagnostics.ModelsAndUtils.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.DataProviders
{
    public class K8SELogAnalyticsDataProvider : LogAnalyticsDataProvider
    {
        private ILogAnalyticsClient _K8SELogAnalyticsClient;

        public K8SELogAnalyticsDataProvider(OperationDataCache cache, K8SELogAnalyticsDataProviderConfiguration configuration, string RequestId) : base(cache, configuration)
        {
            _K8SELogAnalyticsClient = new K8SELogAnalyticsClient(RequestId);
        }

        public override Task<DataTable> ExecuteQueryAsync(string query)
        {
            return _K8SELogAnalyticsClient.ExecuteQueryAsync(query);
        }
    }
}