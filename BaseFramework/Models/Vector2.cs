using Newtonsoft.Json;
using System.Windows;

namespace BaseFramework.Models {

	[Serializable]
	[JsonObject]
	public struct Vector2 : IEquatable<Vector2>, ICloneable {
		public static readonly Vector2 Zero = default;

		public static readonly Vector2 One = new(1.0, 1.0);

		public static readonly Vector2 Max = new(double.MaxValue, double.MaxValue);

		public double X { get; set; }

		public double Y { get; set; }

		[JsonIgnore]
		public readonly int X_Int => (int)Math.Floor(X);

		[JsonIgnore]
		public readonly int Y_Int => (int)Math.Floor(Y);

		[JsonIgnore]
		public readonly double Magnitude => Math.Sqrt(X * X + Y * Y);

		public static double Distance(Vector2 a, Vector2 b) {
			return Math.Sqrt(Math.Pow(a.X - b.X, 2.0) + Math.Pow(a.Y - b.Y, 2.0));
		}

		public Vector2(double size) {
			X = size;
			Y = size;
		}

		public Vector2(double x, double y) {
			X = x;
			Y = y;
		}

		public Vector2(Point point) {
			X = point.X;
			Y = point.Y;
		}

		object ICloneable.Clone() => Clone();

		public readonly Vector2 Clone() => new(X, Y);

		public Vector2 Bound(Vector2 from, Vector2 to) {
			Vector2 result = Clone();
			if (result.X < from.X) {
				result.X = from.X;
			} else if (result.X > to.X) {
				result.X = to.X;
			}

			if (result.Y < from.Y) {
				result.Y = from.Y;
			} else if (result.Y > to.Y) {
				result.Y = to.Y;
			}

			return result;
		}

		public readonly Vector2 ToInt() => new(Math.Floor(X), Math.Floor(Y));

		public override readonly string ToString() => $"Vector2: ({X}, {Y})";

		public override readonly bool Equals(object? obj) => obj is Vector2 vector && Equals(vector);

		public readonly bool Equals(Vector2 other) => X == other.X && Y == other.Y;

		public override readonly int GetHashCode() => HashCode.Combine(X, Y);

		public static implicit operator Vector2(Point point) {
			return new Vector2(point.X, point.Y);
		}

		public static implicit operator Vector2(Size size) {
			return new Vector2(size.Width, size.Height);
		}

		public static implicit operator string(Vector2 vec) {
			return vec.ToString();
		}

		public static bool operator ==(Vector2 v1, Vector2 v2) {
			return v1.Equals(v2);
		}

		public static bool operator !=(Vector2 v1, Vector2 v2) {
			return !v1.Equals(v2);
		}

		public static Vector2 operator +(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
		}

		public static Vector2 operator -(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
		}

		public static Vector2 operator -(Vector2 v) {
			return new Vector2(0.0 - v.X, 0.0 - v.Y);
		}

		public static Vector2 operator *(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.X * v2.X, v1.Y * v2.Y);
		}

		public static Vector2 operator /(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.X / v2.X, v1.Y / v2.Y);
		}

		public static Vector2 operator *(Vector2 v1, double d) {
			return new Vector2(v1.X * d, v1.Y * d);
		}

		public static Vector2 operator /(Vector2 v1, double d) {
			return new Vector2(v1.X / d, v1.Y / d);
		}
	}
}
