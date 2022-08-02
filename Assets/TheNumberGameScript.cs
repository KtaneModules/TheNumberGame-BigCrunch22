using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using KModkit;
using Newtonsoft.Json.Linq;

public class TheNumberGameScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
	
	public AudioClip[] SFX;
	public KMSelectable[] Buttons;
	public KMSelectable MiddleDisplay, BottomDisplay;
	public TextMesh Maximum, ScoreCount, SubmitScore;
	public MeshRenderer Display;
	public GameObject[] ColoredCubes;
	public Material[] TempMaterial;
	public TextMesh[] ButtonNumbers;
	public AudioSource MusicManager;
	
	Coroutine PartTime;
	bool Interactable = false;
	int RandomMaxNumber;
    
    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;
	
	void Awake()
    {
		moduleId = moduleIdCounter++;
		for (int i = 0; i < Buttons.Length; i++)
		{
			int Press = i;
			Buttons[i].OnInteract += delegate ()
			{
				ButtonPress(Press);
				return false;
			};
		}
		
		MiddleDisplay.OnInteract += delegate () { MiddleDisplayPress(); return false; };
		BottomDisplay.OnInteract += delegate () { BottomDisplayPress(); return false; };
		
	}
	
	void Start()
	{
		Module.OnActivate += GenerateEverything;
	}
	
	void ButtonPress(int Press)
	{
		if (Interactable && !ModuleSolved)
		{
			Buttons[Press].AddInteractionPunch(0.25f);
			Audio.PlaySoundAtTransform(SFX[3].name, transform);
			if (SubmitScore.text.Length < 8)
			{
				if (SubmitScore.text == "0")
				{
					SubmitScore.text = ((Press)%10).ToString();
				}
				
				else
				{
					SubmitScore.text += ((Press)%10).ToString();
				}
			}
		}
	}
	
	void MiddleDisplayPress()
    {
		if (Interactable && !ModuleSolved)
		{
			MiddleDisplay.AddInteractionPunch(0.25f);
			Audio.PlaySoundAtTransform(SFX[3].name, transform);
			Interactable = false;
			StartCoroutine(CheckIfValid());
		}
    }
	
	void BottomDisplayPress()
    {
		if (Interactable && !ModuleSolved)
		{
			BottomDisplay.AddInteractionPunch(0.25f);
			Audio.PlaySoundAtTransform(SFX[3].name, transform);
			SubmitScore.text = "";
		}
    }
	
	void GenerateEverything()
	{
		StartCoroutine(GenerationMethod());
	}
	
	IEnumerator GenerationMethod()
	{
		Debug.LogFormat("[The Number Game #{0}] ----------------------------------------------------------------------", moduleId);
		RandomMaxNumber = UnityEngine.Random.Range(10000000, 100000000);
		Debug.LogFormat("[The Number Game #{0}] Maximum integer allowed: {1}", moduleId, RandomMaxNumber.ToString());
		Maximum.text = ""; ScoreCount.text = ""; SubmitScore.text = "";
		string TextOutput = "MAX: " + RandomMaxNumber.ToString();
		ScoreCount.color = new Color(165f/255f, 165f/255f, 165f/255f);
		yield return new WaitForSecondsRealtime(0.5f);
		ColoredCubes[0].GetComponent<Renderer>().material.color = new Color(70f/255f, 70f/255f, 70f/255f);
		Audio.PlaySoundAtTransform(SFX[1].name, transform);
		yield return new WaitForSecondsRealtime(0.5f);
		for (int x = 0; x < TextOutput.Length; x++)
		{
			Maximum.text += TextOutput[x].ToString();
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			yield return new WaitForSecondsRealtime(0.2f);
		}
		yield return new WaitForSecondsRealtime(0.5f);
		ColoredCubes[1].GetComponent<Renderer>().material = TempMaterial[1];
		ColoredCubes[1].GetComponent<Renderer>().material.color = new Color(0f/255f, 0f/255f, 255f/255f);
		Audio.PlaySoundAtTransform(SFX[1].name, transform);
		yield return new WaitForSecondsRealtime(0.5f);
		int MiddleDisplayNumber = ((RandomMaxNumber+1)*UnityEngine.Random.Range(8,11)) + UnityEngine.Random.Range(0,RandomMaxNumber) + 1;
		Debug.LogFormat("[The Number Game #{0}] Currently displayed number: {1}", moduleId, MiddleDisplayNumber.ToString());
		Debug.LogFormat("[The Number Game #{0}] What number should be sent for a correct answer: {1}", moduleId, (MiddleDisplayNumber%(RandomMaxNumber+1)).ToString());
		for (int x = 0; x < MiddleDisplayNumber.ToString().Length; x++)
		{
			ScoreCount.text += MiddleDisplayNumber.ToString()[x].ToString();
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			yield return new WaitForSecondsRealtime(0.2f);
		}
		yield return new WaitForSecondsRealtime(0.5f);
		ColoredCubes[2].GetComponent<Renderer>().material = TempMaterial[1];
		ColoredCubes[2].GetComponent<Renderer>().material.color = new Color(100f/255f, 100f/255f, 100f/255f);
		Audio.PlaySoundAtTransform(SFX[1].name, transform);
		yield return new WaitForSecondsRealtime(0.5f);
		for (int x = 0; x < ButtonNumbers.Count(); x++)
		{
			ButtonNumbers[x].color = new Color(120f/255f, 120f/255f, 120f/255f);
			Audio.PlaySoundAtTransform(SFX[2].name, transform);
			yield return new WaitForSecondsRealtime(0.2f);
		}
		Interactable = true;
	}
	
	IEnumerator CheckIfValid()
	{
		int MiddleDisplayToCheck = int.Parse(ScoreCount.text), BottomDisplayToCheck;
		if (SubmitScore.text.Length == 0)
		{
			BottomDisplayToCheck = -1;
		}
		
		else
		{
			BottomDisplayToCheck = int.Parse(SubmitScore.text);
		}
		
		ScoreCount.text = ""; SubmitScore.text = "";
		
		if (MiddleDisplayToCheck > RandomMaxNumber)
		{
			string Processing = "PROCESSING";
			for (int x = 0; x < Processing.Length; x++)
			{
				ScoreCount.text += Processing[x].ToString();
				Audio.PlaySoundAtTransform(SFX[0].name, transform);
				yield return new WaitForSecondsRealtime(0.1f);
			}
			yield return new WaitForSecondsRealtime(1.5f);
		}
		
		if (BottomDisplayToCheck == -1)
		{
			Debug.LogFormat("[The Number Game #{0}] You sent: [NULL]. That is invalid. A strike and a reset is given.", moduleId);
			StartCoroutine(Shutdown("NULL VALUE"));
		}
		
		else if (BottomDisplayToCheck == 0)
		{
			Debug.LogFormat("[The Number Game #{0}] You sent: 0. That is invalid. A strike and a reset is given.", moduleId);
			StartCoroutine(Shutdown("ZERO USAGE ERROR"));
		}
		
		else if (BottomDisplayToCheck > RandomMaxNumber)
		{
			Debug.LogFormat("[The Number Game #{0}] You sent: {1}, which is larger than the maximum value. That is invalid. A strike and a reset is given.", moduleId, BottomDisplayToCheck.ToString());
			StartCoroutine(Shutdown("MAXIMUM VALUE OVERFLOW"));
		}
		
		else if (BottomDisplayToCheck > MiddleDisplayToCheck)
		{
			Debug.LogFormat("[The Number Game #{0}] You sent: {1}, which is larger than the display value. That is invalid. A strike and a reset is given.", moduleId, BottomDisplayToCheck.ToString());
			StartCoroutine(Shutdown("DISPLAY VALUE OVERFLOW"));
		}
		
		else if ((MiddleDisplayToCheck - BottomDisplayToCheck) % (RandomMaxNumber+1) != 0)
		{
			Debug.LogFormat("[The Number Game #{0}] You sent: {1}, which causes an unavoilable loss. The game is halted. A strike and a reset is given.", moduleId, BottomDisplayToCheck.ToString());
			StartCoroutine(Shutdown("AUTOMATIC LOSS"));
		}
		
		else
		{
			if (MiddleDisplayToCheck - BottomDisplayToCheck == 0)
			{
				Debug.LogFormat("[The Number Game #{0}] You sent: {1}. That is the final number needed to be sent. Module solved.", moduleId, BottomDisplayToCheck.ToString());
				Debug.LogFormat("[The Number Game #{0}] ----------------------------------------------------------------------", moduleId);
				StartCoroutine(SolveAnimation());
			}
			
			else
			{
				ScoreCount.text = "";
				Debug.LogFormat("[The Number Game #{0}] You sent: {1}. That is correct.", moduleId, BottomDisplayToCheck.ToString());
				int NextValue = MiddleDisplayToCheck - BottomDisplayToCheck - (UnityEngine.Random.Range(0,RandomMaxNumber) + 1);
				for (int x = 0; x < NextValue.ToString().Length; x++)
				{
					ScoreCount.text += NextValue.ToString()[x].ToString();
					Audio.PlaySoundAtTransform(SFX[0].name, transform);
					yield return new WaitForSecondsRealtime(0.2f);
				}	
				Debug.LogFormat("[The Number Game #{0}] Currently displayed number: {1}", moduleId, NextValue.ToString());
				Debug.LogFormat("[The Number Game #{0}] What number should be sent for a correct answer: {1}", moduleId, (NextValue%(RandomMaxNumber+1)).ToString());
				Interactable = true;
			}
		}
	}
	
	IEnumerator Shutdown(string Error)
	{
		ScoreCount.text = ""; Maximum.text = ""; 
		Module.HandleStrike();
		for (int x = 0; x < 4; x++)
		{
			ColoredCubes[1].GetComponent<Renderer>().material.color = new Color(255f/255f, 255f/255f, 255f/255f);
			Audio.PlaySoundAtTransform(SFX[4].name, transform);
			yield return new WaitForSecondsRealtime(0.25f);
			ColoredCubes[1].GetComponent<Renderer>().material.color = new Color(255f/255f, 0f/255f, 0f/255f);
			Audio.PlaySoundAtTransform(SFX[5].name, transform);
			yield return new WaitForSecondsRealtime(0.75f);
			
		}
		ScoreCount.color = Color.black; SubmitScore.color = Color.white;
		string Processing = "GAME OVER";
		for (int x = 0; x < Processing.Length; x++)
		{
			ScoreCount.text += Processing[x].ToString();
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			yield return new WaitForSecondsRealtime(0.1f);
		}
		yield return new WaitForSecondsRealtime(1.5f);
		for (int x = 0; x < Error.Length; x++)
		{
			SubmitScore.text += Error[x].ToString();
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			yield return new WaitForSecondsRealtime(0.1f);
		}
		yield return new WaitForSecondsRealtime(1f);
		for (int x = 0; x < ButtonNumbers.Count(); x++)
		{
			ButtonNumbers[x].color = new Color(0f/255f, 0f/255f, 0f/255f);
			Audio.PlaySoundAtTransform(SFX[7].name, transform);
			yield return new WaitForSecondsRealtime(0.2f);
		}
		yield return new WaitForSecondsRealtime(0.5f);
		for (int x = ColoredCubes.Count(); x > 0; x--)
		{
			switch (x)
			{
				case 3:
					SubmitScore.text = "";
					break;
				case 2:
					ScoreCount.text = "";
					break;
				default:
					break;
			}
			ColoredCubes[x-1].GetComponent<Renderer>().material = TempMaterial[0];
			Audio.PlaySoundAtTransform(SFX[6].name, transform);
			yield return new WaitForSecondsRealtime(0.5f);
		}
		yield return new WaitForSecondsRealtime(1.5f);
		GenerateEverything();
	}
	
	IEnumerator SolveAnimation()
	{
		ScoreCount.text = ""; Maximum.text = "";
		MusicManager.clip = SFX[8];
		MusicManager.Play();
		for (int x = 0; x < ColoredCubes.Count(); x++)
		{
			ColoredCubes[x].GetComponent<Renderer>().material = TempMaterial[1];
			ColoredCubes[x].GetComponent<Renderer>().material.color = new Color(0f/255f, 0f/255f, 0f/255f);
		}
		for (int x = 0; x < ButtonNumbers.Count(); x++)
		{
			ButtonNumbers[x].color = new Color(0f/255f, 0f/255f, 0f/255f);
		}
		while (MusicManager.isPlaying)
		{
			yield return new WaitForSecondsRealtime(0.01f);
		}
		MusicManager.clip = SFX[9];
		MusicManager.Play();
		for (int x = 0; x < ColoredCubes.Count(); x++)
		{
			ColoredCubes[x].GetComponent<Renderer>().material.color = new Color(255f/255f, 255f/255f, 255f/255f);
		}
		for (int x = 0; x < ButtonNumbers.Count(); x++)
		{
			ButtonNumbers[x].color = new Color(255f/255f, 255f/255f, 255f/255f);
		}
		while (MusicManager.isPlaying)
		{
			yield return new WaitForSecondsRealtime(0.01f);
		}
		MusicManager.clip = SFX[10];
		MusicManager.Play();
		for (int x = 0; x < ColoredCubes.Count(); x++)
		{
			ColoredCubes[x].GetComponent<Renderer>().material.color = new Color(0f/255f, 255f/255f, 0f/255f);
		}
		for (int x = 0; x < ButtonNumbers.Count(); x++)
		{
			ButtonNumbers[x].color = new Color(0f/255f, 255f/255f, 0f/255f);
		}
		Module.HandlePass();
		ModuleSolved = true;
		while (MusicManager.isPlaying)
		{
			yield return new WaitForSecondsRealtime(0.01f);
		}
		yield return new WaitForSecondsRealtime(1f);
		ScoreCount.color = Color.black; SubmitScore.color = Color.white;
		string Processing = "MODULE SOLVED";
		for (int x = 0; x < Processing.Length; x++)
		{
			ScoreCount.text += Processing[x].ToString();
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			yield return new WaitForSecondsRealtime(0.1f);
		}
		yield return new WaitForSecondsRealtime(2f);
		for (int x = 0; x < ButtonNumbers.Count(); x++)
		{
			ButtonNumbers[x].color = new Color(0f/255f, 0f/255f, 0f/255f);
			Audio.PlaySoundAtTransform(SFX[7].name, transform);
			yield return new WaitForSecondsRealtime(0.2f);
		}
		yield return new WaitForSecondsRealtime(0.5f);
		for (int x = ColoredCubes.Count(); x > 0; x--)
		{
			switch (x)
			{
				case 3:
					SubmitScore.text = "";
					break;
				case 2:
					ScoreCount.text = "";
					break;
				default:
					break;
			}
			ColoredCubes[x-1].GetComponent<Renderer>().material = TempMaterial[0];
			Audio.PlaySoundAtTransform(SFX[6].name, transform);
			yield return new WaitForSecondsRealtime(0.5f);
		}
	}
    
    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To submit a number on the module, use the command !{0} submit <positive number>";
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
			
			if (parameters[1].Length < 1 || parameters[1].Length > 8)
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
			
			if (!Interactable)
			{
				yield return "sendtochaterror You can't interact with the module right now. Command ignored.";
				yield break;
			}
			
			for (int x = 0; x < parameters[1].Length; x++)
			{
				Buttons[Int32.Parse(parameters[1][x].ToString())].OnInteract();
				yield return new WaitForSecondsRealtime(0.1f);
			}
			
			yield return "strike";
			yield return "solve";
			MiddleDisplay.OnInteract();
		}
	}
}
