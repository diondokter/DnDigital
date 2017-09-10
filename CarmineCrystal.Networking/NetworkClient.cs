using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CarmineCrystal.Networking
{
	public class NetworkClient : IDisposable
	{
		private List<Response> ResponseBuffer = new List<Response>();
		private TcpClient Client;
		private NetworkStream _Connection;
		private NetworkStream Connection
		{
			get
			{
				if (Disposed)
				{
					return null;
				}

				if (_Connection == null)
				{
					try
					{
						_Connection = Client?.GetStream();
					}
					catch (InvalidOperationException)
					{
						Dispose();
						return null;
					}
				}

				return _Connection;
			}
		}

		private static DelegateMessageProcessingModule<PingRequest> PingProcessingModule = new DelegateMessageProcessingModule<PingRequest>((RunTarget, Sender) => Sender.Send(new PingResponse() { Time = RunTarget.Time }));

		public IPAddress ConnectedIP => ((IPEndPoint)Client?.Client.RemoteEndPoint).Address;
		public bool IsConnected => Client.Connected;

		private MessageProcessingModule[] ProcessingModules;

		public NetworkClient(TcpClient Client, params MessageProcessingModule[] ProcessingModules)
		{
			this.Client = Client;
			this.ProcessingModules = ProcessingModules;

			new Task(LoopReceive).Start();
		}

		public NetworkClient(string Host, int Port, params MessageProcessingModule[] ProcessingModules)
		{
			Client = new TcpClient(AddressFamily.InterNetwork);
			this.ProcessingModules = ProcessingModules;

			Connect(Host, Port);
		}

		private async void Connect(string Host, int Port)
		{
			await Client.ConnectAsync(Host, Port);
			new Task(LoopReceive).Start();
		}

		public void Send(Message Value)
		{
			lock (Client)
			{
				if (!Client.Connected || !Client.Client.Connected || Connection == null)
				{
					throw new SocketException((int)SocketError.NotConnected);
				}

				try
				{
					Value.SerializeInto(Connection);
				}
				catch (IOException)
				{
					if (NetworkServer.Started)
					{
						NetworkServer.RemoveClient(this, NetworkServer.ClientRemoveReason.Disconnect);
					}
				}
			}
		}

		public async Task<T> Send<T>(Request Value, long WaitTime = 2000) where T:Response
		{
			Send(Value);

			T Response = null;
			Stopwatch Watch = Stopwatch.StartNew();

			while (Response == null && Watch.ElapsedMilliseconds < WaitTime && !Disposed)
			{
				lock (ResponseBuffer)
				{
					Response = ResponseBuffer.OfType<T>().FirstOrDefault(x => x.ID == Value.ID);
					if (Response != null)
					{
						ResponseBuffer.Remove(Response);
						return Response;
					}
				}

				await Task.Delay(5);
			}

			return Response;
		}

		private async void LoopReceive()
		{
			while (!Disposed)
			{
				Message ReceivedMessage = Receive();

				if (ReceivedMessage == null)
				{
					await Task.Delay(5);
					continue;
				}

				if (ReceivedMessage is Response)
				{
					lock (ResponseBuffer)
					{
						ResponseBuffer.Add((Response)ReceivedMessage);
					}
				}
				else
				{
					new Task(() =>
					{
						PingProcessingModule.Process(ReceivedMessage, this);
						for (int i = 0; i < ProcessingModules.Length; i++)
						{
							ProcessingModules[i].Process(ReceivedMessage, this);
						}
					}).Start();
				}
			}
		}

		private Message Receive()
		{
			lock (Client)
			{
				if (Disposed)
				{
					return null;
				}

				if (Connection == null || !Client.Connected)
				{
					throw new SocketException((int)SocketError.NotConnected);
				}

				while (!Connection.DataAvailable)
				{
					return null;
				}

				return Message.DeserializeFrom(Connection);
			}
		}

		private bool Disposed = false;
		public void Dispose()
		{
			lock (Client)
			{
				Client.Dispose();
				Disposed = true;
			}
		}
	}
}
