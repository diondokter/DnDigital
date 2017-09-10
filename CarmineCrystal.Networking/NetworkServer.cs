using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarmineCrystal.Networking
{
	public static class NetworkServer
	{
		public static bool Started { get; private set; }

		private static List<NetworkClient> _Clients = new List<NetworkClient>();
		public static ReadOnlyCollection<NetworkClient> Clients => _Clients.AsReadOnly();
		private static TcpListener Listener;
		private static CancellationTokenSource ListenerCancelToken;
		private static MessageProcessingModule[] ProcessingModules;

		public static void Start(int Port, params MessageProcessingModule[] ProcessingModules)
		{
			NetworkServer.ProcessingModules = ProcessingModules;

			Listener = new TcpListener(IPAddress.Any, Port);
			Listener.Start();

			ListenerCancelToken = new CancellationTokenSource();
			new Task(Listen, ListenerCancelToken.Token).Start();
			Started = true;
		}

		public static void Stop()
		{
			ListenerCancelToken.Cancel();
			Listener?.Stop();
			Listener = null;

			_Clients.ForEach(x => x.Dispose());
			Started = false;
		}

		private static async void Listen()
		{
			while (Listener != null)
			{
				TcpClient NewClient = await Listener.AcceptTcpClientAsync();
				NetworkClient NewNetworkClient = new NetworkClient(NewClient, ProcessingModules);
				_Clients.Add(NewNetworkClient);
				ClientAdded?.Invoke(NewNetworkClient);
			}
		}

		public static void BroadCast(Message message)
		{
			if (message is Request)
			{
				throw new ArgumentException("A request message can't be broadcast because there is no way to listen for the responses properly.", nameof(message));
			}

			// Reverse order so that if a client gets removed, it will not skip an other client
			for (int i = Clients.Count - 1; i >= 0; i--)
			{
				Clients[i].Send(message);
			}
		}

		public static void RemoveClient(NetworkClient target, ClientRemoveReason reason)
		{
			target.Dispose();
			_Clients.Remove(target);

			ClientRemoved?.Invoke(target, reason);
		}

		public static event ClientAddedHandler ClientAdded;
		public delegate void ClientAddedHandler(NetworkClient target);

		public static event ClientRemovedHandler ClientRemoved;
		public delegate void ClientRemovedHandler(NetworkClient target, ClientRemoveReason reason);

		public enum ClientRemoveReason
		{
			Forced, ClientRequest, Disconnect
		}
	}
}
