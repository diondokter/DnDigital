using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;
using System.Reflection;
using ProtoBuf.Meta;

namespace CarmineCrystal.Networking
{
	[ProtoContract]
	public abstract class Message
	{
		public static bool IsInitialized { get; private set; }

		public static void Initialize(params Assembly[] AdditionalAssemblies)
		{
			if (IsInitialized)
			{
				return;
			}

			List<Assembly> TargetAssemblies = AdditionalAssemblies.ToList();
			TargetAssemblies.Add(typeof(Message).GetTypeInfo().Assembly);

			TypeInfo[] MessageTypes = TargetAssemblies.SelectMany(x => x.ExportedTypes).Select(x => x.GetTypeInfo()).Where(x => x.IsSubclassOf(typeof(Message))).ToArray();

			Dictionary<Type, int> SubTypeCount = new Dictionary<Type, int>();

			for (int i = 0; i < MessageTypes.Length; i++)
			{
				MetaType ProtobufType;

				if (!SubTypeCount.ContainsKey(MessageTypes[i].BaseType))
				{
					ProtobufType = RuntimeTypeModel.Default.Add(MessageTypes[i].BaseType, true);
					SubTypeCount[MessageTypes[i].BaseType] = 101;
				}
				else
				{
					ProtobufType = RuntimeTypeModel.Default[MessageTypes[i].BaseType];
				}

				ProtobufType.AddSubType(SubTypeCount[MessageTypes[i].BaseType], MessageTypes[i].AsType());
				SubTypeCount[MessageTypes[i].BaseType]++;
			}

			IsInitialized = true;
		}

		public void SerializeInto(Stream TargetStream)
		{
			if (!IsInitialized)
			{
				throw new NotSupportedException($"The message system has not been initialized yet. Call {nameof(Message)}.{nameof(Initialize)} first.");
			}

			if (!TargetStream.CanWrite)
			{
				throw new NotSupportedException($"{nameof(TargetStream)} can't be written to. Provide a write enabled stream.");
			}

			Serializer.SerializeWithLengthPrefix(TargetStream, this, PrefixStyle.Base128);
		}

		public static Message DeserializeFrom(Stream TargetStream)
		{
			return DeserializeFrom<Message>(TargetStream);
		}

		public static T DeserializeFrom<T>(Stream TargetStream) where T:Message
		{
			if (!IsInitialized)
			{
				throw new NotSupportedException($"The message system has not been initialized yet. Call {nameof(Message)}.{nameof(Initialize)} first.");
			}

			if (!TargetStream.CanRead)
			{
				throw new NotSupportedException($"{nameof(TargetStream)} can't be read from. Provide a read enabled stream.");
			}

			return Serializer.DeserializeWithLengthPrefix<T>(TargetStream, PrefixStyle.Base128);
		}
	}
}
