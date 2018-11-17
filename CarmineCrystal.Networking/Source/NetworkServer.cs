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
		private static TcpListener ListenerV6;
		private static MessageProcessingModule[] ProcessingModules;
		private static NetworkClient.AuthenticationHandler AuthenticationCallback;

		public static void Start(int Port, NetworkClient.AuthenticationHandler AuthenticationCallback = null, params MessageProcessingModule[] ProcessingModules)
		{
			if (Started)
			{
				return;
			}

			NetworkServer.ProcessingModules = ProcessingModules;
			NetworkServer.AuthenticationCallback = AuthenticationCallback;

			Listener = new TcpListener(IPAddress.Any, Port);
			Listener.Start();

			ListenerV6 = new TcpListener(IPAddress.IPv6Any, Port);
			ListenerV6.Start();

			new Task(() => Listen(() => Listener)).Start();
			new Task(() => Listen(() => ListenerV6)).Start();
			Started = true;
		}

		public static void Stop()
		{
			if (!Started)
			{
				return;
			}

			Listener?.Stop();
			Listener = null;
			ListenerV6?.Stop();
			ListenerV6 = null;

			_Clients.ForEach(x => x.Dispose());
			_Clients.Clear();
			Started = false;
		}

		private static async void Listen(Func<TcpListener> TargetListenerGetter)
		{
			while (TargetListenerGetter() != null)
			{
				try
				{
					TcpClient NewClient = await TargetListenerGetter().AcceptTcpClientAsync();
					NetworkClient NewNetworkClient = new NetworkClient(NewClient, AuthenticationCallback, ProcessingModules);
					_Clients.Add(NewNetworkClient);
					ClientAdded?.Invoke(NewNetworkClient);
				}
				catch (ObjectDisposedException) { }
			}
		}

		public static void Broadcast(Message message)
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

		public static void BroadcastEncrypted(Message message)
		{
			if (message is Request)
			{
				throw new ArgumentException("A request message can't be broadcast because there is no way to listen for the responses properly.", nameof(message));
			}

			// Reverse order so that if a client gets removed, it will not skip an other client
			for (int i = Clients.Count - 1; i >= 0; i--)
			{
				if (Clients[i].HasEncryptedConnection)
				{
					Clients[i].SendEncrypted(message);
				}
			}
		}

		public static void BroadcastAuthenticated(Message message)
		{
			if (message is Request)
			{
				throw new ArgumentException("A request message can't be broadcast because there is no way to listen for the responses properly.", nameof(message));
			}

			// Reverse order so that if a client gets removed, it will not skip an other client
			for (int i = Clients.Count - 1; i >= 0; i--)
			{
				if (Clients[i].IsRemoteClientAuthenticated)
				{
					Clients[i].SendEncrypted(message);
				}
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
