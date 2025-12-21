using System;
using UnityEngine;

namespace Kryz.RPG.Items.Views
{
	[Serializable]
	public struct Serialize<T> : ISerializationCallbackReceiver where T : class
	{
		[SerializeField]
		private Component? component;

		public T? Value { get; private set; }

		public void OnAfterDeserialize()
		{
			Value = component as T;
		}

		public void OnBeforeSerialize()
		{
			if (component != null && component is not T)
			{
				Debug.LogWarning($"Component '{component.GetType().Name}' is not of type '{typeof(T).Name}'.", component);
				component = null;
			}
		}
	}
}