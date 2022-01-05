using UnityEngine;
using System.Collections;


namespace GeoJSON {

	[System.Serializable]
	public class PositionObject {
		public float latitude;
		public float longitude;

		public PositionObject() {
		}

		public PositionObject(float pointLongitude, float pointLatitude) {
			this.longitude = pointLongitude;
			this.latitude = pointLatitude;
		}

		public PositionObject(JSONObject jsonObject) {
			longitude = jsonObject.list [0].f;
			latitude = jsonObject.list [1].f;
		}

		public virtual JSONObject Serialize() {

			JSONObject jsonObject = new JSONObject (JSONObject.Type.ARRAY);
			jsonObject.Add (longitude);
			jsonObject.Add (latitude);

			return jsonObject;
		}

		public override string ToString() {
			return longitude + "," + latitude;
		}

		public virtual float[] ToArray() {

			float[] array = new float[2];

			array [0] = longitude;
			array [1] = latitude;

			return array;
		}
	}

	[System.Serializable]
	public class PositionObjectV3 : PositionObject {
		public Vector3 position;

		public PositionObjectV3() {
		}

		public PositionObjectV3(float x, float y, float z)
        {
            this.position = new Vector3(x, z, y);
        }

		public PositionObjectV3(JSONObject jsonObject)
        {
            this.position = new Vector3(jsonObject.list[0].f, jsonObject.list[2].f, jsonObject.list[1].f);
		}

		public override JSONObject Serialize() {

			JSONObject jsonObject = new JSONObject (JSONObject.Type.ARRAY);
			jsonObject.Add (position.x);
			jsonObject.Add (position.y);
			jsonObject.Add (position.z);

			return jsonObject;
		}

		public override string ToString() {
			return position.ToString();
		}

		public override float[] ToArray() {

			float[] array = new float[3];

			array [0] = position.x;
			array [1] = position.y;
			array [2] = position.z;

			return array;
		}
	}
}
