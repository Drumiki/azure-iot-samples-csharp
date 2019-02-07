// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Samples.Common;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client.Samples
{
    public class DeviceStreamSample
    {
        private DeviceClient _deviceClient;
        private ModuleClient _moduleClient;

        public DeviceStreamSample(DeviceClient deviceClient, ModuleClient moduleClient)
        {
            if(deviceClient == null && moduleClient == null)
            {
                throw new Exception("No device client or module client..");
            }
            _deviceClient = deviceClient;
            _moduleClient = moduleClient;
        }

        public async Task RunSampleAsync()
        {
            await RunSampleAsync(true).ConfigureAwait(false);
        }

        public async Task RunSampleAsync(bool acceptDeviceStreamingRequest)
        {
            byte[] buffer = new byte[1024];

            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
            {
                DeviceStreamRequest streamRequest = null;
                if (this._deviceClient != null)
                {
                    streamRequest = await _deviceClient.WaitForDeviceStreamRequestAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                }
                else if (this._moduleClient != null)
                {
                    streamRequest = await _moduleClient.WaitForDeviceStreamRequestAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                }
                
                if (streamRequest != null)
                {
                    if (acceptDeviceStreamingRequest)
                    {
                        if (this._deviceClient != null)
                        {
                            await _deviceClient.AcceptDeviceStreamRequestAsync(streamRequest, cancellationTokenSource.Token).ConfigureAwait(false);
                        }
                        else if (this._moduleClient != null)
                        {
                            await _moduleClient.AcceptDeviceStreamRequestAsync(streamRequest, cancellationTokenSource.Token).ConfigureAwait(false);
                        }

                        using (ClientWebSocket webSocket = await DeviceStreamingCommon.GetStreamingClientAsync(streamRequest.Url, streamRequest.AuthorizationToken, cancellationTokenSource.Token).ConfigureAwait(false))
                        {
                            WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), cancellationTokenSource.Token).ConfigureAwait(false);
                            Console.WriteLine("Received stream data: {0}", Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));

                            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, receiveResult.Count), WebSocketMessageType.Binary, true, cancellationTokenSource.Token).ConfigureAwait(false);
                            Console.WriteLine("Sent stream data: {0}", Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));

                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, cancellationTokenSource.Token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        if (this._deviceClient != null)
                        {
                            await _deviceClient.RejectDeviceStreamRequestAsync(streamRequest, cancellationTokenSource.Token).ConfigureAwait(false);
                        }
                        else if (this._moduleClient != null)
                        {
                            await _moduleClient.RejectDeviceStreamRequestAsync(streamRequest, cancellationTokenSource.Token).ConfigureAwait(false);
                        }
                    }
                }

                if (this._deviceClient != null)
                {
                    await _deviceClient.CloseAsync().ConfigureAwait(false);
                }
                else if (this._moduleClient != null)
                {
                    await _moduleClient.CloseAsync().ConfigureAwait(false);
                }
                
            }
        }
    }
}
