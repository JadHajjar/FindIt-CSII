﻿using Colossal.UI.Binding;

using Game.UI;

using System;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public abstract partial class ExtendedUISystemBase : UISystemBase
	{
		public ValueBindingHelper<T> CreateBinding<T>(string key, T initialValue)
		{
			var helper = new ValueBindingHelper<T>(new(Mod.Id, key, initialValue));

			AddBinding(helper.Binding);

			return helper;
		}

		public ValueBindingHelper<T> CreateBinding<T>(string key, string setterKey, T initialValue, Action<T> updateCallBack = null)
		{
			var helper = new ValueBindingHelper<T>(new(Mod.Id, key, initialValue), updateCallBack);
			var trigger = new TriggerBinding<T>(Mod.Id, setterKey, helper.UpdateCallback);

			AddBinding(helper.Binding);
			AddBinding(trigger);

			return helper;
		}

		public ValueBindingHelper<T[]> CreateBinding<T>(string key, T[] initialValue) where T : IJsonWritable
		{
			var helper = new ValueBindingHelper<T[]>(new(Mod.Id, key, initialValue, new ArrayWriter<T>(new ValueWriter<T>())));

			AddBinding(helper.Binding);

			return helper;
		}

		public ValueBindingHelper<T[]> CreateBinding<T>(string key, string setterKey, T[] initialValue, Action<T[]> updateCallBack = null) where T : IJsonWritable
		{
			var helper = new ValueBindingHelper<T[]>(new(Mod.Id, key, initialValue, new ArrayWriter<T>(new ValueWriter<T>())), updateCallBack);
			var trigger = new TriggerBinding<T[]>(Mod.Id, setterKey, helper.UpdateCallback);

			AddBinding(helper.Binding);
			AddBinding(trigger);

			return helper;
		}

		public GetterValueBinding<T> CreateBinding<T>(string key, Func<T> getterFunc)
		{
			var binding = new GetterValueBinding<T>(Mod.Id, key, getterFunc);

			AddBinding(binding);

			return binding;
		}

		public GetterValueBinding<T[]> CreateBinding<T>(string key, Func<T[]> getterFunc) where T : IJsonWritable
		{
			var binding = new GetterValueBinding<T[]>(Mod.Id, key, getterFunc, new ArrayWriter<T>(new ValueWriter<T>()));

			AddBinding(binding);

			return binding;
		}

		public TriggerBinding CreateTrigger(string key, Action action)
		{
			var binding = new TriggerBinding(Mod.Id, key, action);

			AddBinding(binding);

			return binding;
		}

		public TriggerBinding<T1> CreateTrigger<T1>(string key, Action<T1> action)
		{
			var binding = new TriggerBinding<T1>(Mod.Id, key, action);

			AddBinding(binding);

			return binding;
		}

		public TriggerBinding<T1, T2> CreateTrigger<T1, T2>(string key, Action<T1, T2> action)
		{
			var binding = new TriggerBinding<T1, T2>(Mod.Id, key, action);

			AddBinding(binding);

			return binding;
		}

		public TriggerBinding<T1, T2, T3> CreateTrigger<T1, T2, T3>(string key, Action<T1, T2, T3> action)
		{
			var binding = new TriggerBinding<T1, T2, T3>(Mod.Id, key, action);

			AddBinding(binding);

			return binding;
		}

		public TriggerBinding<T1, T2, T3, T4> CreateTrigger<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> action)
		{
			var binding = new TriggerBinding<T1, T2, T3, T4>(Mod.Id, key, action);

			AddBinding(binding);

			return binding;
		}
	}

	public class ValueBindingHelper<T>
	{
		private readonly ValueBinding<T> _binding;
		private readonly Action<T> _updateCallBack;

		public ValueBinding<T> Binding => _binding;

		public T Value { get => _binding.value; set => _binding.Update(value); }

		public ValueBindingHelper(ValueBinding<T> binding, Action<T> updateCallBack = null)
		{
			_binding = binding;
			_updateCallBack = updateCallBack;
		}

		public void UpdateCallback(T value)
		{
			_binding.Update(value);
			_updateCallBack?.Invoke(value);
		}

		public static implicit operator T(ValueBindingHelper<T> helper)
		{
			return helper._binding.value;
		}
	}
}
