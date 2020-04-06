using System.Collections.ObjectModel;
using UnityEngine;

namespace OP2MissionHub.Data.Json
{
	/// <summary>
	/// Contains data about a mission.
	/// </summary>
	[System.Serializable]
	public class MissionData
	{
		[SerializeField] private uint MissionID					= default;
		[SerializeField] private string MissionName				= default;
		[SerializeField] private string MissionDescription		= default;
		[SerializeField] private string GitHubLink				= default;
		[SerializeField] private int AuthorID					= default;
		[SerializeField] private string AuthorName				= default;
		[SerializeField] private string CertifyingAdminName		= default;
		[SerializeField] private int Version					= default;
		[SerializeField] private string[] FileNames				= default;

		public uint missionID				{ get { return MissionID;				}	}
		public string missionName			{ get { return MissionName;				}	}
		public string missionDescription	{ get { return MissionDescription;		}	}
		public string gitHubLink			{ get { return GitHubLink;				}	}
		public int authorID					{ get { return AuthorID;				}	}
		public string authorName			{ get { return AuthorName;				}	}
		public string certifyingAdminName	{ get { return CertifyingAdminName;		}	}
		public int version					{ get { return Version;					}	}
		public ReadOnlyCollection<string> fileNames			{ get { return new ReadOnlyCollection<string>(FileNames);				}	}
	}

	/// <summary>
	/// Contains response data for all missions.
	/// </summary>
	[System.Serializable]
	public class MissionResponse : JsonResponse
	{
		[SerializeField] private MissionData[] Missions			= new MissionData[0];

		public ReadOnlyCollection<MissionData> missions			{ get { return new ReadOnlyCollection<MissionData>(Missions);				}	}


		public static new MissionResponse FromJson(string json)
		{
			return JsonResponse.FromJson<MissionResponse>(json);
		}
	}
}
