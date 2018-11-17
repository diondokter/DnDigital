using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CarmineCrystal.Networking
{
	public class NetworkClient : IDisposable
	{
		public IPAddress ConnectedIP => ((IPEndPoint)Client?.Client.RemoteEndPoint).Address;
		public bool IsConnected => Client.Connected;
		public bool HasEncryptedConnection => Cryptor != null;
		public bool IsLocalClientAuthenticated { get; private set; }
		public bool IsRemoteClientAuthenticated { get; private set; }

		private static readonly DelegateMessageProcessingModule<PingRequest> PingProcessingModule = new DelegateMessageProcessingModule<PingRequest>((RunTarget, Sender) => Sender.Send(new PingResponse() { Time = RunTarget.Time, ID = RunTarget.ID }));
		private static readonly DelegateMessageProcessingModule<KeyExchangeRequest> KeyExchangeProcessingModule = new DelegateMessageProcessingModule<KeyExchangeRequest>((RunTarget, Sender) => Sender.ProcessKeyExchange(RunTarget));
		private static readonly DelegateMessageProcessingModule<AuthenticationRequest> AuthenticationProcessingModule = new DelegateMessageProcessingModule<AuthenticationRequest>((RunTarget, Sender) => Sender.ProcessAuthentication(RunTarget));

		private List<Response> ResponseBuffer = new List<Response>();
		private TcpClient Client;
		private AesManaged Cryptor;

		private NetworkStream _NetworkConnection;
		private Stream Connection
		{
			get
			{
				if (Disposed)
				{
					return null;
				}

				if (_NetworkConnection == null)
				{
					try
					{
						_NetworkConnection = Client?.GetStream();
					}
					catch (InvalidOperationException)
					{
						Dispose();
						return null;
					}
				}

				return _NetworkConnection;
			}
		}
		private bool DataAvailable => _NetworkConnection?.DataAvailable ?? false;

		private MessageProcessingModule[] ProcessingModules;

		public string AuthenticatedUsername { get; private set; }
		public delegate (bool Accepted, string Reason) AuthenticationHandler(NetworkClient Caller, string Username, string Password);
		private AuthenticationHandler AuthenticationCallback;

		public NetworkClient(TcpClient Client, AuthenticationHandler AuthenticationCallback = null, params MessageProcessingModule[] ProcessingModules)
		{
			this.Client = Client;
			this.ProcessingModules = ProcessingModules;
			this.AuthenticationCallback = AuthenticationCallback;

			new Task(LoopReceive).Start();
		}

		public NetworkClient(string Host, int Port, AuthenticationHandler AuthenticationCallback = null, params MessageProcessingModule[] ProcessingModules)
		{
			Client = new TcpClient(Host, Port);
			this.ProcessingModules = ProcessingModules;
			this.AuthenticationCallback = AuthenticationCallback;

			new Task(LoopReceive).Start();
		}

		public void SendEncrypted(Message Value)
		{
			Send(Value, true);
		}

		public void Send(Message Value, bool Encrypted = false)
		{
			lock (Client)
			{
				if (!Client.Connected || Connection == null)
				{
					throw new SocketException((int)SocketError.NotConnected);
				}

				try
				{
					if (Encrypted)
					{
						if (Cryptor == null)
						{
							throw new CryptographicUnexpectedOperationException("Encryption is not initialized...");
						}

						EncryptedMessage EncryptedValue = new EncryptedMessage();
						EncryptedValue.SetPayload(Value, Cryptor);
						Value = EncryptedValue;
					}

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

		public async Task<T> SendEncrypted<T>(Request Value, long WaitTime = 2000) where T : Response
		{
			return await Send<T>(Value, true, WaitTime);
		}

		public async Task<T> Send<T>(Request Value, bool Encrypted = false, long WaitTime = 2000) where T : Response
		{
			Send(Value, Encrypted);

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

		public async Task<bool> InitializeEncryption(RSAParameters? Parameters = null, int AESKeySize = 256)
		{
			RSA RSAEncryption = RSA.Create();
			RSAParameters PublicParameters;
			RSAEncryption.KeySize = Parameters?.D?.Length * 8 ?? 4096;

			if (Parameters != null)
			{
				RSAEncryption.ImportParameters(Parameters.Value);
			}

			PublicParameters = RSAEncryption.ExportParameters(false);

			KeyExchangeRequest Request = new KeyExchangeRequest() { RSAKeySize = RSAEncryption.KeySize, AESKeySize = AESKeySize, RSAExponent = PublicParameters.Exponent, RSAModulus = PublicParameters.Modulus };
			KeyExchangeResponse Response = await Send<KeyExchangeResponse>(Request);

			if (Response?.Accepted ?? false)
			{
				Cryptor = new AesManaged();
				Cryptor.KeySize = AESKeySize;
				Cryptor.IV = RSAEncryption.Decrypt(Response.EncryptedAESIV, RSAEncryptionPadding.Pkcs1);
				Cryptor.Key = RSAEncryption.Decrypt(Response.EncryptedAESKey, RSAEncryptionPadding.Pkcs1);
				Cryptor.Mode = CipherMode.CBC;
				Cryptor.Padding = PaddingMode.PKCS7;

				return true;
			}

			return false;
		}

		private void ProcessKeyExchange(KeyExchangeRequest Request)
		{
			RSA RSAEncryption = RSA.Create();
			RSAEncryption.KeySize = Request.RSAKeySize;

			RSAParameters PublicParameters = new RSAParameters() { Exponent = Request.RSAExponent, Modulus = Request.RSAModulus };
			RSAEncryption.ImportParameters(PublicParameters);

			Cryptor = new AesManaged();
			Cryptor.KeySize = Request.AESKeySize;
			Cryptor.GenerateIV();
			Cryptor.GenerateKey();
			Cryptor.Mode = CipherMode.CBC;
			Cryptor.Padding = PaddingMode.PKCS7;

			KeyExchangeResponse Response = new KeyExchangeResponse() { Accepted = true, EncryptedAESIV = RSAEncryption.Encrypt(Cryptor.IV, RSAEncryptionPadding.Pkcs1), EncryptedAESKey = RSAEncryption.Encrypt(Cryptor.Key, RSAEncryptionPadding.Pkcs1), ID = Request.ID };
			Send(Response);
		}

		public async Task<(bool Accepted, string Reason)> Authenticate(string Username, string Password)
		{
			if (IsLocalClientAuthenticated)
			{
				return (true, "Local client already has been authenticated.");
			}

			if (!HasEncryptedConnection)
			{
				return (false, "No encrypted connection has been established.");
			}

			AuthenticationRequest Request = new AuthenticationRequest() { Username = Username, Password = Password };
			AuthenticationResponse Response = await SendEncrypted<AuthenticationResponse>(Request);

			if (Response == null)
			{
				return (false, "No response was given.");
			}

			IsLocalClientAuthenticated = Response.Accepted;

			return (Response.Accepted, Response.Reason);
		}

		private void ProcessAuthentication(AuthenticationRequest Request)
		{
			if (IsRemoteClientAuthenticated)
			{
				SendEncrypted(new AuthenticationResponse() { ID = Request.ID, Accepted = true, Reason = "Remote client is already authenticated." });
				return;
			}

			(bool Accepted, string Reason)? Result = AuthenticationCallback?.Invoke(this, Request.Username, Request.Password);

			if (Result?.Accepted ?? true)
			{
				AuthenticatedUsername = Request.Username;
			}

			IsRemoteClientAuthenticated = Result?.Accepted ?? true;
			SendEncrypted(new AuthenticationResponse() { ID = Request.ID, Accepted = Result?.Accepted ?? true, Reason = Result?.Reason ?? "No authentication method is available. Any authentication request will be accepted." });
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

				if (ReceivedMessage is EncryptedMessage ReceivedEncryptedMessage)
				{
					if (Cryptor == null)
					{
						continue;
					}

					ReceivedMessage = ReceivedEncryptedMessage.GetPayload(Cryptor);
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
						KeyExchangeProcessingModule.Process(ReceivedMessage, this);
						AuthenticationProcessingModule.Process(ReceivedMessage, this);
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

				if (!DataAvailable)
				{
					return null;
				}

				try
				{
					return Message.DeserializeFrom(Connection);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					Console.WriteLine("Response buffer count: " + ResponseBuffer.Count);

					return null;
				}
			}
		}

		private bool Disposed = false;
		public void Dispose()
		{
			if (Disposed)
			{
				return;
			}

			if (Client != null)
			{
				lock (Client)
				{
					Client.Dispose();
				}
			}

			Disposed = true;
		}

		public async Task<TimeSpan> GetPing()
		{
			PingResponse Response = await Send<PingResponse>(new PingRequest() { Time = DateTime.Now });
			return (DateTime.Now - Response?.Time) ?? TimeSpan.MaxValue;
		}

		public async Task<bool> CheckConnection()
		{
			return await GetPing() != TimeSpan.MaxValue;
		}

		public override string ToString()
		{
			return $"Network client {{{ConnectedIP}}} [{AuthenticatedUsername}]";
		}
	}
}
