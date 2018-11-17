using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarmineCrystal.Networking.Tests
{
	[TestClass]
	public class ProtobufTest
	{
		[TestMethod]
		public void HasProtoContractTest()
		{
			Type[] AllMessageTypes = typeof(Message).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Message)) || x == typeof(Message)).ToArray();

			for (int i = 0; i < AllMessageTypes.Length; i++)
			{
				Assert.IsTrue(AllMessageTypes[i].CustomAttributes.Any(x => x.AttributeType == typeof(ProtoContractAttribute)), $"{AllMessageTypes[i].FullName} does not have a protocontract attribute.");
			}
		}

		[TestMethod]
		public void ProtoSubTypeTest()
		{
			Message.Initialize();
			Type[] AllMessageTypes = typeof(Message).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Message))).ToArray();

			for (int i = 0; i < AllMessageTypes.Length; i++)
			{
				Assert.IsTrue(RuntimeTypeModel.Default[AllMessageTypes[i].BaseType].GetSubtypes().Any(x => x.DerivedType.Type == AllMessageTypes[i]), $"{AllMessageTypes[i].BaseType.FullName} does not have {AllMessageTypes[i].FullName} added as a subtype in the default {nameof(RuntimeTypeModel)}.");
			}
		}
	}
}
