﻿using System;
using Gi7.Client.Model.Event;
using Gi7.Client.Request.Base;
using Gi7.Client.Utils;
using RestSharp;

namespace Gi7.Client.Request.Event
{
    public class ListForUser : PaginatedRequest<Model.Event.Event>
    {
        public ListForUser(String username)
        {
            Uri = String.Format("/users/{0}/events", username);
        }

        protected override void preRequest(RestClient client, RestRequest request)
        {
            client.AddHandler("application/json", new EventDeserializer());
        }
    }
}
