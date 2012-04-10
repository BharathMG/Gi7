﻿using System;
using Gi7.Client.Model;
using Gi7.Client.Request.Base;

namespace Gi7.Client.Request
{
    public class IssuesRequest : PaginatedRequest<Issue>
    {
        public IssuesRequest(string username, string repo)
        {
            Uri = String.Format("/repos/{0}/{1}/issues", username, repo);
        }
    }
}