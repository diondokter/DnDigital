using CarmineCrystal.Networking;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.DnDigital.Messages
{
	[ProtoContract]
    public class CameraMovedMessage : Message
    {
		[ProtoMember(1)]
		public double HorizontalOffset { get; set; }
		[ProtoMember(2)]
		public double VerticalOffset { get; set; }
		[ProtoMember(3)]
		public float ZoomFactor { get; set; }
	}
}
