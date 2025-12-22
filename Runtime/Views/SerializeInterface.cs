using System;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Kryz.RPG.Items.Views
{
	[Serializable]
	public struct SerializeInterface<T> where T : class
	{
		[SerializeField, FormerlySerializedAs("component")]
		private Object? unityObject;

		private T? value;

		public T? Value => value ??= unityObject as T;
	}
}