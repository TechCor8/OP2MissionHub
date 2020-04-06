using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace FlexAS
{
	public class GetProfileResponse : JsonResponse
	{
		[Serializable]
		public class GameCurrency
		{
			// JSON serialized fields
			[SerializeField] private uint CurrencyTypeID		= default;
			[SerializeField] private uint Amount				= default;

			// Accessors
			public uint currencyTypeID							{ get { return CurrencyTypeID;										} }
			public uint amount									{ get { return Amount;												} }
		}

		// JSON serialized fields
		[SerializeField] private string LastLoginTime		= default;
		[SerializeField] private GameCurrency[] UserCurrency = new GameCurrency[0];

		// Accessors
		public string lastLoginTime								{ get { return LastLoginTime;										} }
		public ReadOnlyCollection<GameCurrency> userCurrency	{ get { return new ReadOnlyCollection<GameCurrency>(UserCurrency);	} }
		

		public static new GetProfileResponse FromJson(string json)
		{
			return JsonResponse.FromJson<GetProfileResponse>(json);
		}
	}
}
