using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.DnDigital.Datamodels
{
	/// <summary>
	/// A structure to represent a point / location
	/// </summary>
	[ProtoContract]
    public struct Point
    {
		[ProtoMember(1)]
		public double X { get; private set; }
		[ProtoMember(2)]
		public double Y { get; private set; }

		public Point(double X, double Y)
		{
			this.X = X;
			this.Y = Y;
		}

		/// <summary>
		/// Returns a new point that is translated a certain amount
		/// </summary>
		/// <param name="X">Amount to translate on the X axis</param>
		/// <param name="Y">Amount to translate on the Y axis</param>
		public Point Translate(double X, double Y)
		{
			return new Point(this.X + X, this.Y + Y);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Point))
			{
				return false;
			}

			Point point = (Point)obj;
			return X == point.X &&
				   Y == point.Y;
		}

		public override int GetHashCode()
		{
			int hashCode = 1861411795;
			hashCode = hashCode * -1521134295 + base.GetHashCode();
			hashCode = hashCode * -1521134295 + X.GetHashCode();
			hashCode = hashCode * -1521134295 + Y.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(Point A, Point B) => A.X == B.X && A.Y == B.Y;
		public static bool operator !=(Point A, Point B) => A.X != B.X || A.Y != B.Y;
		public static bool operator >(Point A, Point B) => A.X > B.X && A.Y > B.Y;
		public static bool operator <(Point A, Point B) => A.X < B.X && A.Y < B.Y;
		public static bool operator >=(Point A, Point B) => A.X >= B.X && A.Y >= B.Y;
		public static bool operator <=(Point A, Point B) => A.X <= B.X && A.Y <= B.Y;
	}
}
