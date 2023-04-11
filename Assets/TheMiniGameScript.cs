using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using KModkit;
using Newtonsoft.Json.Linq;


public class TheMiniGameScript : MonoBehaviour
{
	public KMNeedyModule Needy;
	public KMAudio Audio;
	
	public Renderer[] SquareRenderers;
	public Material[] StaticVariables;
	public TextMesh[] Numbers;
	public AudioClip[] SFX;
	public AudioSource MusicPlayer;
	
	private KeyCode[] TypableKeys =
    {
        KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9, KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Backspace
    };
	bool focused, Active, Activated;
	int Stages, RandomMaxNumber;
	int[] PreviousNumbers = {0, 0};
	Coroutine StaticLoop, FlashingNumber, StaticNoise;
	string InputString = "";
	
	//Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;
	
	#pragma warning disable 0649
    private bool TwitchPlaysActive;
    #pragma warning restore 0649
	int waitTime = 10;
	
	void Awake()
	{
		moduleId = moduleIdCounter++;
		Needy.OnNeedyActivation += Activate;
        Needy.OnNeedyDeactivation += Deactivate;
		Needy.OnTimerExpired += Expired;
		GetComponent<KMSelectable>().OnFocus += delegate () { focused = true; };
        GetComponent<KMSelectable>().OnDefocus += delegate () { focused = false; };
        if (Application.isEditor)
            focused = true;
		
	}
	
	void Start()
    {
        Generate();
        Needy.OnActivate += TheMiniGameOnTP;	}
	
	
	void Generate()
    {
        SquareRenderers[0].material.color = SquareRenderers[1].material.color = new Color32(0, 0, 0, 255);
        Stages = UnityEngine.Random.Range(3, 6);
    }
	
	
	void Update()
	{
		for (int i = 0; i < TypableKeys.Count(); i++)
        {
            if (Input.GetKeyDown(TypableKeys[i]) && focused)
            {
                if (i < 20) { PressKey(i); }
                else if (i < 22) { PressEnter(); }
                else { PressBack(); }
            }
        }
	}
	
	void TheMiniGameOnTP()
	{
		waitTime = TwitchPlaysActive ? 12 : 10;
	}
	
	void PressKey(int Key)
	{
		if (Active && InputString.Length < 2)
		{
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			InputString = InputString == "0" ? (Key % 10).ToString() : InputString + (Key % 10).ToString();
			Numbers[0].text = Numbers[1].text = InputString;
		}
	}
	
	void PressEnter()
	{
		if (Active)
		{
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			if (InputString == "")
			{
				Debug.LogFormat("[The Mini Game #{0}] You sent: [NULL]. That is invalid. A strike is given. Needy deactivated.", moduleId);
				Needy.HandleStrike();
				Needy.HandlePass();
				Deactivate();
			}
			
			else if (int.Parse(InputString) == 0)
			{
				Debug.LogFormat("[The Mini Game #{0}] You sent: 0. That is invalid. A strike is given. Needy deactivated.", moduleId);
				Needy.HandleStrike();
				Needy.HandlePass();
				Deactivate();
			}
			
			else if (int.Parse(InputString) > RandomMaxNumber)
			{
				Debug.LogFormat("[The Mini Game #{0}] You sent: {1}, which is larger than the maximum value. That is invalid. A strike is given. Needy deactivated.", moduleId, int.Parse(InputString).ToString());
				Needy.HandleStrike();
				Needy.HandlePass();
				Deactivate();
			}
			
			else if (int.Parse(InputString) > int.Parse(Numbers[2].text))
			{
				Debug.LogFormat("[The Mini Game #{0}] You sent: {1}, which is larger than the display value. That is invalid. A strike is given. Needy deactivated.", moduleId, int.Parse(InputString).ToString());
				Needy.HandleStrike();
				Needy.HandlePass();
				Deactivate();
			}
			
			else if ((int.Parse(Numbers[2].text) - int.Parse(InputString)) % (RandomMaxNumber+1) != 0)
			{
				Debug.LogFormat("[The Mini Game #{0}] You sent: {1}, which causes an unavoilable loss. The game is halted. A strike is given. Needy deactivated.", moduleId, int.Parse(InputString).ToString());
				Needy.HandleStrike();
				Needy.HandlePass();
				Deactivate();
			}
		
			else
			{
				if (int.Parse(Numbers[2].text) - int.Parse(InputString) == 0)
				{
					Debug.LogFormat("[The Mini Game #{0}] You sent: {1}. That is the final number needed to be sent. Needy deactivated.", moduleId, int.Parse(InputString).ToString());
					Needy.HandlePass();
					Deactivate();
				}
				
				else
				{
					Debug.LogFormat("[The Mini Game #{0}] You sent: {1}. That is correct.", moduleId, int.Parse(InputString).ToString());
					int NextValue = int.Parse(Numbers[2].text) - int.Parse(InputString) - (UnityEngine.Random.Range(0,RandomMaxNumber) + 1);
					Numbers[2].text = Numbers[3].text = NextValue.ToString();
					Debug.LogFormat("[The Mini Game #{0}] Currently displayed number: {1}", moduleId, NextValue.ToString());
					Debug.LogFormat("[The Mini Game #{0}] What number should be sent for a correct answer: {1}", moduleId, (NextValue%(RandomMaxNumber+1)).ToString());
				}
			}
			
			InputString = "";
		}
	}
	
	void PressBack()
	{
		if (Active && InputString.Length > 0)
		{
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			InputString = InputString.Remove(InputString.Length - 1);
			Numbers[0].text = Numbers[1].text = InputString;
		}
	}
	
	protected void Activate()
	{
		Active = true;
		Activated = true;
		Needy.SetNeedyTimeRemaining((waitTime * Stages) + (waitTime / 2));
		RandomMaxNumber = UnityEngine.Random.Range(10, 100);
		StaticLoop = StartCoroutine(Static());
		FlashingNumber = StartCoroutine(NumberFlashing());
		StaticNoise = StartCoroutine(KeepPlaying());
		Numbers[2].text = Numbers[3].text = (((RandomMaxNumber+1)*Stages) + UnityEngine.Random.Range(0,RandomMaxNumber) + 1).ToString();
		Debug.LogFormat("[The Mini Game #{0}] ----------------------------------------------------------------------", moduleId);
		Debug.LogFormat("[The Mini Game #{0}] Amount of stages: {1} + extra free answer", moduleId, Stages.ToString());
		Debug.LogFormat("[The Mini Game #{0}] Maximum integer allowed: {1}", moduleId, RandomMaxNumber.ToString());
		Debug.LogFormat("[The Mini Game #{0}] Currently displayed number: {1}", moduleId, Numbers[2].text);
		Debug.LogFormat("[The Mini Game #{0}] What number should be sent for a correct answer: {1}", moduleId, (int.Parse(Numbers[2].text)%(RandomMaxNumber+1)).ToString());
	}
	
	protected void Deactivate()
	{
		Active = false;
		MusicPlayer.Stop();
		Numbers[1].text = Numbers[2].text = Numbers[3].text = Numbers[0].text = "";
		if (Activated)
		{
			StopCoroutine(FlashingNumber);
			StopCoroutine(StaticLoop);
			StopCoroutine(StaticNoise);
		}
		SquareRenderers[0].material.color = SquareRenderers[1].material.color = new Color32(0, 0, 0, 255);
		Stages = UnityEngine.Random.Range(3, 6);
		Needy.CountdownTime = waitTime * Stages;
		Debug.LogFormat("[The Mini Game #{0}] ----------------------------------------------------------------------", moduleId);
	}
	
	protected void Expired()
	{
		Needy.HandleStrike();
		Debug.LogFormat("[The Mini Game #{0}] You ran out of time. A strike is given. Needy deactivated.", moduleId);
		Deactivate();
	}
	
	IEnumerator KeepPlaying()
	{
		while (true)
		{
			MusicPlayer.clip = SFX[1];
			MusicPlayer.Play();
			while (MusicPlayer.isPlaying)
			{
				yield return new WaitForSecondsRealtime(0.01f);
			}
		}
	}
	
	IEnumerator NumberFlashing()
	{
		int TempNumber = 0;
		while (true)
		{
			if (InputString == "")
			{
				Numbers[0].text = Numbers[1].text = TempNumber == 0 ? RandomMaxNumber.ToString() : "";
			}
			TempNumber = (TempNumber + 1) % 2;
			yield return new WaitForSecondsRealtime(0.25f);
		}
	}
	
	IEnumerator Static()
	{
		while (true)
		{
			for (int x = 0; x < SquareRenderers.Count(); x++)
			{
				SquareRenderers[x].material = StaticVariables[PreviousNumbers[x]];
				PreviousNumbers[x] = (PreviousNumbers[x] + 1) % 10;
			}
				
			SquareRenderers[0].material.color = new Color32(255, 0, 0, 255);
			SquareRenderers[1].material.color = new Color32(0, 0, 255, 255);
			yield return new WaitForSecondsRealtime(0.05f);
		}
	}
	
	 //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To submit a number on the needy, use the command !{0} submit <positive number>";
    #pragma warning restore 414
	
	string[] ValidNumber = {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0"};
	
	IEnumerator ProcessTwitchCommand(string command)
    {
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (parameters.Length != 2)
			{
				yield return "sendtochaterror Parameter length invalid. Command ignored.";
				yield break;
			}
			
			if (parameters[1].Length < 1 || parameters[1].Length > 2)
			{
				yield return "sendtochaterror Number length invalid. Command ignored.";
				yield break;
			}
			
			for (int x = 0; x < parameters[1].Length; x++)
			{
				if (!ValidNumber.Contains(parameters[1][x].ToString()))
				{
					yield return "sendtochaterror Number contains an invalid character. Command ignored.";
					yield break;
				}
			}
			
			if (!Active)
			{
				yield return "sendtochaterror You can't interact with the needy right now. Command ignored.";
				yield break;
			}
			
			for (int x = 0; x < parameters[1].Length; x++)
			{
				PressKey(Int32.Parse(parameters[1][x].ToString()));
				yield return new WaitForSecondsRealtime(0.1f);
			}
			
			PressEnter();
		}
	}
}
