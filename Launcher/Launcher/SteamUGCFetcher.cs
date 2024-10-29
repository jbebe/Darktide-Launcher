using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;

namespace Launcher;

internal class SteamUGCFetcher
{
	public enum UGCStatus : uint
	{
		NOT_FOUND,
		QUERYING_STEAM,
		FAILED,
		DONE
	}

	private CallResult<SteamUGCQueryCompleted_t> _ugc_query_call_result;

	private Callback<PersonaStateChange_t> _persona_state_change_callback;

	private PublishedFileId_t[] _pending_items;

	private List<object> _pending_mod_list;

	private Dictionary<ulong, bool> _pending_authors;

	public SteamUGCFetcher()
	{
		_ugc_query_call_result = CallResult<SteamUGCQueryCompleted_t>.Create(OnUGCQueryResult);
		_persona_state_change_callback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
		_pending_authors = new Dictionary<ulong, bool>();
	}

	private void OnUGCQueryResult(SteamUGCQueryCompleted_t query, bool failure)
	{
		UGCQueryHandle_t handle = query.m_handle;
		List<object> pending_mod_list = _pending_mod_list;
		for (uint num = 0u; num < query.m_unNumResultsReturned; num++)
		{
			SteamUGC.GetQueryUGCResult(handle, num, out var pDetails);
			string pchURL;
			bool queryUGCPreviewURL = SteamUGC.GetQueryUGCPreviewURL(handle, num, out pchURL, 200u);
			if (pDetails.m_eResult == EResult.k_EResultOK)
			{
				string id = pDetails.m_nPublishedFileId.ToString();
				Dictionary<string, object> dictionary = (Dictionary<string, object>)pending_mod_list.Find((object x) => ((string)((Dictionary<string, object>)x)["id"]).Equals(id));
				if (dictionary == null)
				{
					continue;
				}
				dictionary["last_updated"] = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(pDetails.m_rtimeUpdated).ToString();
				dictionary["banned"] = pDetails.m_bBanned;
				if (pDetails.m_bBanned)
				{
					dictionary["banned"] = true;
					dictionary["enabled"] = false;
				}
				else
				{
					dictionary["banned"] = false;
				}
				dictionary["name"] = pDetails.m_rgchTitle;
				dictionary["sanctioned"] = false;
				if (!pDetails.m_bTagsTruncated)
				{
					string rgchTags = pDetails.m_rgchTags;
					List<object> list = new List<object>();
					if (!rgchTags.Equals(""))
					{
						string[] array = rgchTags.Split(',');
						foreach (string text in array)
						{
							if (text.ToLower().Equals("approved") || text.ToLower().Equals("sanctioned"))
							{
								dictionary["sanctioned"] = true;
							}
							list.Add(text);
						}
					}
					dictionary["tags"] = list;
				}
				dictionary["description"] = pDetails.m_rgchDescription;
				if (queryUGCPreviewURL)
				{
					dictionary["url"] = pchURL;
				}
				else
				{
					dictionary["url"] = "";
				}
				dictionary["ugc_status"] = 3u;
				ulong ulSteamIDOwner = pDetails.m_ulSteamIDOwner;
				dictionary["author_id"] = ulSteamIDOwner;
				if (_pending_authors.ContainsKey(ulSteamIDOwner))
				{
					dictionary["author"] = "fetching...";
				}
				else
				{
					CSteamID cSteamID = new CSteamID(ulSteamIDOwner);
					if (SteamFriends.RequestUserInformation(cSteamID, bRequireNameOnly: true))
					{
						_pending_authors[ulSteamIDOwner] = true;
						dictionary["author"] = "fetching...";
					}
					else
					{
						string friendPersonaName = SteamFriends.GetFriendPersonaName(cSteamID);
						dictionary["author"] = friendPersonaName;
					}
				}
				uint unNumChildren = pDetails.m_unNumChildren;
				dictionary["num_children"] = unNumChildren;
				if (pDetails.m_unNumChildren == 0)
				{
					continue;
				}
				PublishedFileId_t[] array2 = new PublishedFileId_t[unNumChildren];
				if (SteamUGC.GetQueryUGCChildren(handle, num, array2, unNumChildren))
				{
					List<string> list2 = new List<string>();
					for (int j = 0; j < unNumChildren; j++)
					{
						list2.Add(array2[j].ToString());
					}
					dictionary["children"] = list2;
				}
			}
			else
			{
				string id = pDetails.m_nPublishedFileId.ToString();
				Dictionary<string, object> dictionary2 = (Dictionary<string, object>)pending_mod_list.Find((object x) => ((string)((Dictionary<string, object>)x)["id"]).Equals(id));
				if (dictionary2 != null)
				{
					dictionary2["author"] = "unknown";
					dictionary2["ugc_status"] = 2u;
					dictionary2["enabled"] = false;
					dictionary2["url"] = "";
					dictionary2["banned"] = true;
				}
			}
		}
		_pending_items = null;
	}

	private void OnPersonaStateChanged(PersonaStateChange_t state)
	{
		ulong author_id = state.m_ulSteamID;
		if ((state.m_nChangeFlags & EPersonaChange.k_EPersonaChangeNameFirstSet) <= (EPersonaChange)0 && (state.m_nChangeFlags & EPersonaChange.k_EPersonaChangeName) <= (EPersonaChange)0)
		{
			return;
		}
		string friendPersonaName = SteamFriends.GetFriendPersonaName(new CSteamID(author_id));
		_pending_authors.Remove(author_id);
		foreach (Dictionary<string, object> item in _pending_mod_list.FindAll((object x) => (ulong)((Dictionary<string, object>)x)["author_id"] == author_id))
		{
			item["author"] = friendPersonaName;
		}
	}

	public void UpdateModList(List<object> mods)
	{
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		foreach (Dictionary<string, object> mod in mods)
		{
			mod["ugc_status"] = 0u;
		}
		for (int i = 0; i < numSubscribedItems; i++)
		{
			PublishedFileId_t nPublishedFileID = array[i];
			EItemState itemState = (EItemState)SteamUGC.GetItemState(nPublishedFileID);
			bool flag = (itemState & EItemState.k_EItemStateInstalled) != 0;
			string id = nPublishedFileID.ToString();
			Dictionary<string, object> dictionary = (Dictionary<string, object>)mods.Find((object x) => ((string)((Dictionary<string, object>)x)["id"]).Equals(id));
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, object>();
				dictionary["enabled"] = false;
				dictionary["name"] = "...";
				dictionary["id"] = id;
				mods.Add(dictionary);
			}
			else
			{
				dictionary["enabled"] = dictionary.ContainsKey("enabled") && (bool)dictionary["enabled"] && flag;
			}
			dictionary["installed"] = flag;
			dictionary["out_of_date"] = (itemState & EItemState.k_EItemStateNeedsUpdate) > EItemState.k_EItemStateNone;
			dictionary["downloading"] = (itemState & EItemState.k_EItemStateDownloading) > EItemState.k_EItemStateNone;
			dictionary["download_pending"] = (itemState & EItemState.k_EItemStateDownloadPending) > EItemState.k_EItemStateNone;
			dictionary["ugc_status"] = 1u;
			dictionary["description"] = "";
			dictionary["last_updated"] = "";
			dictionary["url"] = "";
			dictionary["author"] = "";
		}
		if (false)
		{
			foreach (Dictionary<string, object> mod2 in mods)
			{
				if ((uint)mod2["ugc_status"] == 0)
				{
					mod2["enabled"] = false;
					mod2["description"] = "";
					mod2["last_updated"] = "";
					mod2["url"] = "";
					mod2["author"] = "";
				}
			}
		}
		else
		{
			mods.RemoveAll((object item) => (uint)((Dictionary<string, object>)item)["ugc_status"] == 0);
		}
		if (numSubscribedItems != 0)
		{
			UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(array, numSubscribedItems);
			SteamUGC.SetReturnChildren(handle, bReturnChildren: true);
			SteamUGC.SetReturnLongDescription(handle, bReturnLongDescription: true);
			SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(handle);
			_ugc_query_call_result.Set(hAPICall);
			_pending_items = array;
			_pending_mod_list = mods;
			while (_pending_items != null || _pending_authors.Count() != 0)
			{
				SteamAPI.RunCallbacks();
			}
			_pending_mod_list = null;
		}
	}
}
