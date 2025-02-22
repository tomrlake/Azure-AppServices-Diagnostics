﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Diagnostics.ModelsAndUtils.Models.Storage;

namespace Diagnostics.RuntimeHost.Services.DevOpsClient
{
    public interface IRepoClient
    {
        /// <summary>
        /// make a pull request
        /// </summary>
        /// <param name="sourceBranch"></param>
        /// <param name="targetBranch"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        Task<object> MakePullRequestAsync(string sourceBranch, string targetBranch, string title, string resourceUri, string requestId);

        /// <summary>
        /// makes a commit with your changes
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="file"></param>
        /// <param name="repoPath"></param>
        /// <param name="comment"></param>
        /// <param name="changeType"></param>
        /// <returns></returns>
        Task<object> PushChangesAsync(string branch, string file, string repoPath, string comment, string changeType, string resourceUri, string requestId);

        /// <summary>
        /// will retrieve and display the given file from the repository
        /// </summary>
        /// <param name="filePathInRepo"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        Task<object> GetFileContentAsync(string filePathInRepo, string resourceUri, string requestId, string branch = null);

        /// <summary>
        /// gets all of the branches in the repo
        /// </summary>
        /// 
        /// <returns></returns>
        Task<object> GetBranchesAsync(string resourceUri, string requestId);

        /// <summary>
        /// Gets file changed in a given commit id
        /// </summary>
        /// <param name="commitId">Commit id</param>
        Task<List<DevopsFileChange>> GetFilesInCommit(string commitId);

        /// <summary>
        /// Gets file changed between two commits or date range
        /// </summary>
        /// <param name="parameters">Deployment parameters provided by caller</param>
        Task<List<DevopsFileChange>> GetFilesBetweenCommits(DeploymentParameters parameters);
    }
}
