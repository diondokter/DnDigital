using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CarmineCrystal.Networking.Tests
{
    [TestClass]
    public class NetworkTest
    {
		[TestMethod]
		public void ServerConnectionLocalhostTest()
		{
			NetworkServer.Start(5000);

			NetworkClient TestClient = new NetworkClient("localhost", 5000);

			Thread.Sleep(20);

			int ClientCount = NetworkServer.Clients.Count;

			NetworkServer.Stop();
			TestClient.Dispose();

			Assert.AreEqual(1, ClientCount);
		}

		[TestMethod]
		public void ServerConnectionLoopbackV6Test()
		{
			NetworkServer.Start(5000);

			NetworkClient TestClient = new NetworkClient("::1", 5000);

			Thread.Sleep(20);

			int ClientCount = NetworkServer.Clients.Count;

			NetworkServer.Stop();
			TestClient.Dispose();

			Assert.AreEqual(1, ClientCount);
		}

		[TestMethod]
		public async Task PingTest()
		{
			Message.Initialize();
			NetworkServer.Start(5000);

			NetworkClient TestClient = new NetworkClient("localhost", 5000);
			PingResponse Response = await TestClient.Send<PingResponse>(new PingRequest() { Time = DateTime.FromBinary(12) });
			PingResponse Response2 = await TestClient.Send<PingResponse>(new PingRequest() { Time = DateTime.FromBinary(13) });
			PingResponse Response3 = await TestClient.Send<PingResponse>(new PingRequest() { Time = DateTime.FromBinary(14) });
			TimeSpan Ping = await TestClient.GetPing();

			NetworkServer.Stop();
			TestClient.Dispose();

			Assert.AreEqual(12, Response?.Time.ToBinary());
			Assert.AreEqual(13, Response2?.Time.ToBinary());
			Assert.AreEqual(14, Response3?.Time.ToBinary());

			Console.WriteLine(Ping);
			Assert.AreNotEqual(TimeSpan.MaxValue, Ping);
		}

		[TestMethod]
		public async Task EncryptionTest()
		{
			Message.Initialize();
			NetworkServer.Start(5000);

			NetworkClient TestClient = new NetworkClient("localhost", 5000);
			bool EncryptionEnabled = await TestClient.InitializeEncryption();
			bool EncryptionClientEnabled = TestClient.HasEncryptedConnection;

			PingResponse Response = await TestClient.SendEncrypted<PingResponse>(new PingRequest() { Time = DateTime.FromBinary(12) });
			Response = await TestClient.SendEncrypted<PingResponse>(new PingRequest() { Time = DateTime.FromBinary(12) });
			Response = await TestClient.SendEncrypted<PingResponse>(new PingRequest() { Time = DateTime.FromBinary(12) });

			NetworkServer.Stop();
			TestClient.Dispose();

			Assert.AreEqual(true, EncryptionEnabled);
			Assert.AreEqual(true, EncryptionClientEnabled);
			Assert.AreEqual(true, TestClient.HasEncryptedConnection);
			Assert.AreEqual(12, Response?.Time.ToBinary());
		}

		[TestMethod]
		public async Task AuthenticationTest()
		{
			Message.Initialize();
			NetworkServer.Start(5000, (Sender, Username, Password) => (Username == "User2" && Password == "P@ssw1rd!", ""));

			NetworkClient TestClient = new NetworkClient("localhost", 5000);

			(bool Accepted, string Reason) AuthenticationResult1 = await TestClient.Authenticate("User", "P@ssw0rd!");
			(bool Accepted, string Reason) AuthenticationResult2 = await TestClient.Authenticate("User2", "P@ssw0rd!");
			(bool Accepted, string Reason) AuthenticationResult3 = await TestClient.Authenticate("User", "P@ssw1rd!");
			(bool Accepted, string Reason) AuthenticationResult4 = await TestClient.Authenticate("User2", "P@ssw1rd!");

			bool EncryptionEnabled = await TestClient.InitializeEncryption();

			(bool Accepted, string Reason) AuthenticationResult5 = await TestClient.Authenticate("User", "P@ssw0rd!");
			(bool Accepted, string Reason) AuthenticationResult6 = await TestClient.Authenticate("User2", "P@ssw0rd!");
			(bool Accepted, string Reason) AuthenticationResult7 = await TestClient.Authenticate("User", "P@ssw1rd!");
			(bool Accepted, string Reason) AuthenticationResult8 = await TestClient.Authenticate("User2", "P@ssw1rd!");

			NetworkServer.Stop();
			TestClient.Dispose();

			Assert.IsFalse(AuthenticationResult1.Accepted);
			Assert.IsFalse(AuthenticationResult2.Accepted);
			Assert.IsFalse(AuthenticationResult3.Accepted);
			Assert.IsFalse(AuthenticationResult4.Accepted);

			Assert.IsFalse(AuthenticationResult5.Accepted);
			Assert.IsFalse(AuthenticationResult6.Accepted);
			Assert.IsFalse(AuthenticationResult7.Accepted);
			Assert.IsTrue(AuthenticationResult8.Accepted);

			Assert.IsTrue(TestClient.IsLocalClientAuthenticated);
		}
	}
}
