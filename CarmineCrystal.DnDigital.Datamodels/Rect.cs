using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.DnDigital.Datamodels
{
	[ProtoContract]
	public struct Rect
	{
		[ProtoMember(1)]
		public Point CenterPosition { get; private set; }
		[ProtoMember(2)]
		public double Width { get; private set; }
		[ProtoMember(3)]
		public double Height { get; private set; }

		public Point TopLeftPosition => CenterPosition.Translate(-Width / 2, -Height / 2);
		public Point TopRightPosition => CenterPosition.Translate(Width / 2, -Height / 2);
		public Point BottomLeftPosition => CenterPosition.Translate(-Width / 2, Height / 2);
		public Point BottomRightPosition => CenterPosition.Translate(Width / 2, Height / 2);

		public Rect(Point CenterPosition, double Width, double Height)
		{
			this.CenterPosition = CenterPosition;
			this.Width = Width;
			this.Height = Height;
		}

		/// <summary>
		/// Multiplies the Width with X and Height with Y
		/// </summary>
		public Rect Scale(double X, double Y)
		{
			return new Rect(CenterPosition, this.Width * X, this.Height * Y);
		}

		/// <summary>
		/// Sets the Width and Height
		/// </summary>
		public Rect Resize(double Width, double Height)
		{
			return new Rect(CenterPosition, Width, Height);
		}

		public Rect Translate(double X, double Y)
		{
			return new Rect(CenterPosition.Translate(X, Y), Width, Height);
		}

		public bool ContainsPoint(Point TargetPoint)
		{
			return TargetPoint >= TopLeftPosition && TargetPoint <= BottomRightPosition;
		}
	}
}
