
/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Fun
{
	public partial class Fun_AnimalBrain
	{
		[HookAttribute.Patch("OnChickenScared", typeof(AnimalBrain.FleeState), "StateEnter", new System.Type[] { typeof(BaseAIBrain), typeof(BaseEntity) })]
		[HookAttribute.Identifier("4af63eb71cfc44f7a66cb1c16974a5c7")]

		// Gets triggered when a chicken gets scared by something.

		public class Fun_AnimalBrain_FleeState_StateEnter_4af63eb71cfc44f7a66cb1c16974a5c7
		{
			public static void Postfix(BaseAIBrain brain, BaseEntity entity)
			{
				try
				{
					if (brain != null && brain.baseEntity is Chicken chicken)
					{
						HookCaller.CallStaticHook("OnChickenScared", chicken, brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot));
					}
				}
				catch { }
			}
		}
	}
}
