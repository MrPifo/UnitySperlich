using System;
using System.Collections.Generic;
using UnityEngine;

public class Randomizer {

	static System.Random _random;
	static System.Random random {
		get {
			if(_random == null) {
				_random = new System.Random((int)DateTime.Now.Ticks);
			}
			return _random;
		}
	}


	/// <summary>
	/// Returns an inclusive int within the Range of Min,Max
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public static int Range(int min, int max) {
		return random.Next(min, (max + 1));
	}

	/// <summary>
	/// Returns an inclusive float within the Range of Min,Max
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public static float Range(float min, float max) {
		return (float)random.NextDouble() * ((max + 1) - min) + min;
	}

	public static void RenewRandom() => _random = new System.Random((int)DateTime.Now.Ticks);

	/*public static Vector3 GetRandomVelocity(this Vector3 velocity, Vector3Int mask) {
		Vector3 vel = new Vector3(Range(-velocity.x, velocity.x) * mask.x, Range(-velocity.y, velocity.y) * mask.y, Range(-velocity.z, velocity.z) * mask.z);

		return vel;
	}*/
}
