﻿using System;
using Gi7.Client.Model;
using Gi7.Client.Request.Base;

namespace Gi7.Client.Request
{
    public class WatchedRepoRequest : PaginatedRequest<Repository>
    {
        public WatchedRepoRequest(string username)
        {
            Uri = String.Format("/users/{0}/watched", username);
        }
    }
}