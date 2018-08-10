using CarmineCrystal.DnDigital.Datamodels;
using CarmineCrystal.Networking;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.DnDigital.Messages
{
	[ProtoContract]
	public class AddCharacterMessage : Message
	{
		[ProtoMember(1)]
		public Character NewCharacter { get; set; }
	}

	[ProtoContract]
	public class RemoveCharacterMessage : Message
	{
		[ProtoMember(1)]
		public Character OldCharacter { get; set; }
	}

	[ProtoContract]
	public class MoveCharacterMessage : Message
	{
		[ProtoMember(1)]
		public Character TargetCharacter { get; set; }
	}
}
