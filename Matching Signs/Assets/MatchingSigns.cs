using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class MatchingSigns : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public SpriteRenderer[] HoroscopesSR;
   public Sprite[] Horoscopes;
   public KMSelectable[] Tiles;
   public Material[] Colors;

   public GameObject[] ThingsToDisappear;

   int[] ShownSigns = new int[12];

   int[][] Matches = {
      new int[] { 1, 8 },
      new int[] { 4, 0 },
      new int[] { 10, 11 },

      new int[] { 9, 7 },
      new int[] { 1, 5 },
      new int[] { 4, 6 },

      new int[] { 5, 8 },
      new int[] { 10, 3 },
      new int[] { 6, 0 },

      new int[] { 11, 3 },
      new int[] { 2, 7 },
      new int[] { 2, 9 },
   };

   enum Validity {
      Unpaired = 0,
      Invalid = 1,
      Valid = 2
   };


   bool[] CantChangeColors = new bool[12];

   int[] ActualColorOfTiles = new int[12];

   string[] Names = { "Aquarius", "Aries", "Cancer", "Capricorn", "Gemini", "Leo", "Libra", "Pisces", "Saggitarius", "Scorpio", "Taurus", "Virgo"};

   Validity[] TileValidities = new Validity[12];
   int[] PairedWith = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

   int PrevSelected;
   bool HasSelected;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

   void Awake () {
      ModuleId = ModuleIdCounter++;

      foreach (KMSelectable Button in Tiles) {
         Button.OnInteract += delegate () { TilePress(Button); return false; };
         Button.OnHighlight += delegate () { TileHighlight(Button); };
         //Button.OnHighlight += GameFixes.OnDefocus(() => { TileHighlight(Button); });
         //Button.OnHighlightEnded += GameFixes.OnDefocus(() => { TileDeHighlight(Button); });
         Button.OnHighlightEnded += delegate () { TileDeHighlight(Button); };
      }
   }

   void TileHighlight (KMSelectable Tile) {
      for (int i = 0; i < 12; i++) {
         if (Tile == Tiles[i]) {
            if (!CantChangeColors[i]) {
               if (ActualColorOfTiles[i] == 0) { //Because of the game doing some techno bullshit I don't care about, the color changes have to be like this
                  ActualColorOfTiles[i] = 1;
                  Tile.GetComponent<MeshRenderer>().material = Colors[ActualColorOfTiles[i]];
               }
               else if (ActualColorOfTiles[i] == 2) {
                  ActualColorOfTiles[i] = 3;
                  Tile.GetComponent<MeshRenderer>().material = Colors[ActualColorOfTiles[i]];
               }
               else if (ActualColorOfTiles[i] == 4) {
                  ActualColorOfTiles[i] = 5;
                  Tile.GetComponent<MeshRenderer>().material = Colors[ActualColorOfTiles[i]];
               }
            }
         }
      }
   }

   void TileDeHighlight (KMSelectable Tile) {
      for (int i = 0; i < 12; i++) {
         if (Tile == Tiles[i]) {
            if (!CantChangeColors[i]) {
               if (ActualColorOfTiles[i] == 1) {
                  ActualColorOfTiles[i] = 0;
                  Tile.GetComponent<MeshRenderer>().material = Colors[ActualColorOfTiles[i]];
               }
               else if (ActualColorOfTiles[i] == 3) {
                  ActualColorOfTiles[i] = 2;
                  Tile.GetComponent<MeshRenderer>().material = Colors[ActualColorOfTiles[i]];
               }
               else if (ActualColorOfTiles[i] == 5) {
                  ActualColorOfTiles[i] = 4;
                  Tile.GetComponent<MeshRenderer>().material = Colors[ActualColorOfTiles[i]];
               }
            }
         }
      }
   }

   void TilePress (KMSelectable Tile) {
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Tile.transform);
      //StartCoroutine(Solve());
      for (int i = 0; i < 12; i++) {
         if (Tile == Tiles[i]) {
            if (!HasSelected) {
               switch (TileValidities[i]) {
                  case Validity.Unpaired:
                     PrevSelected = i;
                     CantChangeColors[i] = true;
                     HasSelected = true;
                     ActualColorOfTiles[i] = 1;
                     Tiles[i].GetComponent<MeshRenderer>().material = Colors[1];
                     break;
                  case Validity.Valid:
                  case Validity.Invalid:
                     TileValidities[PairedWith[i]] = Validity.Unpaired;
                     ActualColorOfTiles[PairedWith[i]] = 0;
                     ActualColorOfTiles[i] = 1;
                     TileValidities[i] = Validity.Unpaired;
                     Tiles[i].GetComponent<MeshRenderer>().material = Colors[0];
                     Tiles[PairedWith[i]].GetComponent<MeshRenderer>().material = Colors[0];
                     PairedWith[PairedWith[i]] = -1;
                     PairedWith[i] = -1;
                     break;
               }
            }
            else {
               if (CheckMatch(ShownSigns[i], ShownSigns[PrevSelected])) {
                  TileValidities[i] = Validity.Valid;
                  TileValidities[PrevSelected] = Validity.Valid;
                  PairedWith[i] = PrevSelected;
                  PairedWith[PrevSelected] = i;
                  ActualColorOfTiles[PrevSelected] = 4;
                  ActualColorOfTiles[i] = 5;
               }
               else {
                  TileValidities[i] = Validity.Invalid;
                  TileValidities[PrevSelected] = Validity.Invalid;
                  PairedWith[i] = PrevSelected;
                  PairedWith[PrevSelected] = i;
                  ActualColorOfTiles[PrevSelected] = 2;
                  ActualColorOfTiles[i] = 3;
               }
               Tiles[i].GetComponent<MeshRenderer>().material = Colors[ActualColorOfTiles[i]];
               Tiles[PrevSelected].GetComponent<MeshRenderer>().material = Colors[ActualColorOfTiles[PrevSelected]];
               HasSelected = false;
               for (int j = 0; j < 12; j++) {
                  CantChangeColors[j] = false;
               }
            }
         }
      }

      for (int i = 0; i < 12; i++) {
         if (TileValidities[i] == Validity.Invalid || TileValidities[i] == Validity.Unpaired) {
            return;
         }
      }
      ModuleSolved = true;
      StartCoroutine(Solve());
   }

   IEnumerator Solve () {
      List<int> RandomTiles = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};
      List<int> Amounts = new List<int> { 3, 2, 1, 3, 1, 1, 1};
      int ProgressionA = 0;
      int ProgressionR = 0;
      Amounts = Amounts.Shuffle();
      RandomTiles = RandomTiles.Shuffle();
      Audio.PlaySoundAtTransform("Solve", transform);

      for (int j = 0; j < 4; j++) {
         for (int i = 0; i < Amounts[ProgressionA]; i++) {
            ThingsToDisappear[RandomTiles[ProgressionR]].SetActive(false);
            ProgressionR++;
         }
         ProgressionA++;
         yield return new WaitForSeconds(.5f);
      }
      for (int i = 0; i < Amounts[ProgressionA]; i++) {
         ThingsToDisappear[RandomTiles[ProgressionR]].SetActive(false);
         ProgressionR++;
      }
      ProgressionA++;
      yield return new WaitForSeconds(.9f);
      for (int j = 0; j < 2; j++) {
         for (int i = 0; i < Amounts[ProgressionA]; i++) {
            ThingsToDisappear[RandomTiles[ProgressionR]].SetActive(false);
            ProgressionR++;
         }
         ProgressionA++;
         yield return new WaitForSeconds(.2f);
      }
      GetComponent<KMBombModule>().HandlePass();
   }

   void Start () {
      for (int i = 0; i < 12; i += 2) {
         ShownSigns[i] = Rnd.Range(0, 12);
         ShownSigns[i + 1] = CreateMatch(ShownSigns[i]);
         Debug.LogFormat("[Matching Signs #{0}] {1} -> {2}", ModuleId, Names[ShownSigns[i]], Names[ShownSigns[i + 1]]);
      }
      
      ShownSigns = ShownSigns.Shuffle();
      for (int i = 0; i < 12; i++) {
         HoroscopesSR[i].GetComponent<SpriteRenderer>().sprite = Horoscopes[ShownSigns[i]];
      }
   }

   int CreateMatch (int x) {
      return Matches[x][Rnd.Range(0, 2)];
   }

   bool CheckMatch (int x, int y) {
      return Matches[x][0] == y || Matches[x][1] == y;
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} X# to select that tile.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      Command = Command.Trim().ToUpper();
      yield return null;
      if (!"ABCD".Contains(Command[0]) || !"123".Contains(Command[1]) || Command.Length != 2) {
         yield return "sendtochaterror I don't understand!";
      }
      else {
         Tiles[Array.IndexOf("ABCD".ToCharArray(), Command[0]) * 4 + int.Parse(Command[1].ToString()) - 1].OnInteract();
         yield return new WaitForSeconds(.1f);
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return StartCoroutine(Solve());
   }
}