using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Common.Logging;

using Microsoft.AspNet.SignalR;

namespace ExampleApp.Hubs {
    /// <summary>
    /// SignalR hub that will push new snapshot values to clients when they change.
    /// </summary>
    [Authorize]
    public class DataHub : Hub {

        /// <summary>
        /// Logging.
        /// </summary>
        private static readonly ILog s_log = LogManager.GetLogger<DataHub>();

        /// <summary>
        /// Holds a Data Core streaming client for each registered connection.
        /// </summary>
        private static readonly ConcurrentDictionary<string, DataCore.Client.DataCoreStreamClient> StreamingClients = new ConcurrentDictionary<string, DataCore.Client.DataCoreStreamClient>();

        /// <summary>
        /// Holds the tags that each connection has subscribed to receive real-time updates for.
        /// </summary>
        private static readonly ConcurrentDictionary<string, SubscribedTagsCollection> SubscribedTags = new ConcurrentDictionary<string, SubscribedTagsCollection>();

        /// <summary>
        /// We will ask for new values to be pushed to us every 5 seconds. We will only receive a push if new values 
        /// have occurred.
        /// </summary>
        private static readonly TimeSpan PushFrequency = TimeSpan.FromSeconds(5);

        /// <summary>
        /// App Store requires us to give names to the subscriptions we create.
        /// </summary>
        private const string SubscriptionName = "MySubscription";


        /// <summary>
        /// Sends values to a SignalR connection.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="dataSourceName">The data source name.</param>
        /// <param name="values">The values.</param>
        private static void SendValuesToConnection(string connectionId, string dataSourceName, IEnumerable<DataCore.Client.Model.TagValue> values) {
            GlobalHost.ConnectionManager.GetHubContext<DataHub>().Clients.Client(connectionId).OnValuesReceived(dataSourceName, values);
        }


        /// <summary>
        /// Adds snapshot subscriptions for the specified tags.
        /// </summary>
        /// <param name="dataSourceName">The data source name.</param>
        /// <param name="tags">The tags to subscribe to.</param>
        /// <returns>
        /// A task that will add the subscription.
        /// </returns>
        public async Task AddSnapshotSubscription(string dataSourceName, IEnumerable<string> tags) {
            try {
                DataCore.Client.DataCoreStreamClient client;
                if (!StreamingClients.TryGetValue(Context.ConnectionId, out client)) {
                    throw new InvalidOperationException("No streaming client has been created for the caller.");
                }

                SubscribedTagsCollection subscribedTags;
                if (!SubscribedTags.TryGetValue(Context.ConnectionId, out subscribedTags)) {
                    throw new InvalidOperationException("No tag subscription list has been created for the caller.");
                }

                if (client.ConnectionState == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected) {
                    await client.Start(CancellationToken.None).ConfigureAwait(false);
                }

                // Ensure that a real-time subscription has been created in the App Store.
                await client.RealTimeData.CreateSubscriptionAsync(SubscriptionName, PushFrequency).ConfigureAwait(false);

                // Add the requested tags to the subscription.
                await client.RealTimeData.AddTagsToSubscriptionAsync(
                    SubscriptionName,
                    new Dictionary<string, IEnumerable<string>>() {
                        { dataSourceName, tags }
                    }
                ).ConfigureAwait(false);

                foreach (var item in tags) {
                    subscribedTags.AddTag(dataSourceName, item);
                }
            }
            catch (HubException) {
                throw;
            }
            catch (Exception e) {
                s_log.Error($"An error occurred while adding snapshot subscriptions for connection {Context.ConnectionId}.", e);
                throw new HubException("An error occurred while adding snapshot subscriptions.");
            }
        }


        /// <summary>
        /// Removes snapshot subscriptions for the specified tags.
        /// </summary>
        /// <param name="dataSourceName">The data source name.</param>
        /// <param name="tags">The tags to unsubscribe from.</param>
        /// <returns>
        /// A task that will process the request.
        /// </returns>
        public async Task RemoveSnapshotSubscription(string dataSourceName, IEnumerable<string> tags) {
            try {
                DataCore.Client.DataCoreStreamClient client;
                if (!StreamingClients.TryGetValue(Context.ConnectionId, out client)) {
                    throw new InvalidOperationException("No streaming client has been created for the caller.");
                }

                SubscribedTagsCollection subscribedTags;
                if (!SubscribedTags.TryGetValue(Context.ConnectionId, out subscribedTags)) {
                    throw new InvalidOperationException("No tag subscription list has been created for the caller.");
                }

                if (client.ConnectionState == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected) {
                    return;
                }

                await client.RealTimeData.RemoveTagsFromSubscriptionAsync(
                    SubscriptionName,
                    new Dictionary<string, IEnumerable<string>>() {
                        { dataSourceName, tags }
                    }
                ).ConfigureAwait(false);

                foreach (var item in tags) {
                    subscribedTags.RemoveTag(dataSourceName, item);
                }
            }
            catch (HubException) {
                throw;
            }
            catch (Exception e) {
                s_log.Error($"An error occurred while removing snapshot subscriptions for connection {Context.ConnectionId}.", e);
                throw new HubException("An error occurred while removing snapshot subscriptions.");
            }
        }


        /// <summary>
        /// Removes snapshot subscriptions for all specified tags.
        /// </summary>
        /// <returns>
        /// A task that will process the request.
        /// </returns>
        public async Task RemoveAllSnapshotSubscriptions() {
            try {
                DataCore.Client.DataCoreStreamClient client;
                if (!StreamingClients.TryGetValue(Context.ConnectionId, out client)) {
                    throw new InvalidOperationException("No streaming client has been created for the caller.");
                }

                SubscribedTagsCollection subscribedTags;
                if (!SubscribedTags.TryGetValue(Context.ConnectionId, out subscribedTags)) {
                    throw new InvalidOperationException("No tag subscription list has been created for the caller.");
                }

                if (client.ConnectionState == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected) {
                    return;
                }

                // We can easily remove all tag subscriptions by deleting the subscription at the App 
                // Store end. It will be automagically recreated the next time that AddSnapshotSubscription 
                // is called.
                await client.RealTimeData.DeleteSubscriptionAsync(SubscriptionName).ConfigureAwait(false);

                subscribedTags.Clear();
            }
            catch (HubException) {
                throw;
            }
            catch (Exception e) {
                s_log.Error($"An error occurred while removing snapshot subscriptions for connection {Context.ConnectionId}.", e);
                throw new HubException("An error occurred while removing snapshot subscriptions.");
            }
        }


        /// <summary>
        /// Called when a client connects to the hub.
        /// </summary>
        /// <returns>
        /// A task that will process the connection.
        /// </returns>
        public override Task OnConnected() {
            // Get the connection settings from the OWIN environment.
            var connectionSettings = ExampleApp.OwinExtensions.GetDataCoreConnectionSettings(Context.Request.Environment);

            // Create a new streaming client for the SignalR connection.  Note that we don;t start the connection 
            // yet.  We'll do this when the caller adds a subscription.
            var client = new DataCore.Client.DataCoreStreamClient(connectionSettings, new LogManager());
            var connectionId = Context.ConnectionId;

            // Configure the callback on the client so that it will send new values to the caller when they arrive.
            client.RealTimeData.ValuesReceived += (dsn, values) => {
                SendValuesToConnection(connectionId, dsn, values);
            };

            StreamingClients[connectionId] = client;
            SubscribedTags[connectionId] = new SubscribedTagsCollection();

            return base.OnConnected();
        }


        /// <summary>
        /// Called when a client disconnects from the hub.
        /// </summary>
        /// <param name="stopCalled">A flag specifying if the connection was explicitly stopped, or if it timed out.</param>
        /// <returns>
        /// A task that will process the disconnection.
        /// </returns>
        public override Task OnDisconnected(bool stopCalled) {
            SubscribedTagsCollection _;
            SubscribedTags.TryRemove(Context.ConnectionId, out _);

            DataCore.Client.DataCoreStreamClient client;
            if (StreamingClients.TryRemove(Context.ConnectionId, out client)) {
                try {
                    client.Stop();
                }
                catch (Exception e) {
                    s_log.Error($"An error occurred while stopping the Data Core streaming client for connection {Context.ConnectionId}.", e);
                }
            }

            return base.OnDisconnected(stopCalled);
        }

        #region [ Inner Types ]

        /// <summary>
        /// Describes tags that a user has subscribed to receive real-time updates for.
        /// </summary>
        private class SubscribedTagsCollection {

            /// <summary>
            /// Holds tag subscriptions, indexed by data source.
            /// </summary>
            private readonly IDictionary<string, ICollection<string>> _subscribedTags = new Dictionary<string, ICollection<string>>(StringComparer.OrdinalIgnoreCase);


            /// <summary>
            /// Gets all subscriptions, indexed by data source name.
            /// </summary>
            /// <returns>
            /// All tag subscriptions currently defined in the collection, indexed by data source name.
            /// </returns>
            public IDictionary<string, IEnumerable<string>> GetAllSubscriptions() {
                lock (_subscribedTags) {
                    return _subscribedTags.Where(x => x.Value.Any())
                                          .ToDictionary(x => x.Key, x => x.Value.ToArray().AsEnumerable(), StringComparer.OrdinalIgnoreCase);
                }
            }


            /// <summary>
            /// Adds a tag subscription if it does not already exist.
            /// </summary>
            /// <param name="dsn">The data source name.</param>
            /// <param name="tag">The tag name.</param>
            public void AddTag(string dsn, string tag) {
                if (dsn == null || tag == null) {
                    return;
                }

                lock (_subscribedTags) {
                    ICollection<string> tags;
                    if (!_subscribedTags.TryGetValue(dsn, out tags)) {
                        tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        _subscribedTags[dsn] = tags;
                    }

                    tags.Add(tag);
                }
            }


            /// <summary>
            /// Removes an existing subscription.
            /// </summary>
            /// <param name="dsn">The data source name.</param>
            /// <param name="tag">The tag name.</param>
            public void RemoveTag(string dsn, string tag) {
                if (dsn == null || tag == null) {
                    return;
                }

                lock (_subscribedTags) {
                    ICollection<string> tags;
                    if (!_subscribedTags.TryGetValue(dsn, out tags)) {
                        return;
                    }

                    tags.Remove(tag);
                }
            }


            /// <summary>
            /// Removes all subscriptions.
            /// </summary>
            public void Clear() {
                lock (_subscribedTags) {
                    _subscribedTags.Clear();
                }
            }

        }

        #endregion

    }
}
