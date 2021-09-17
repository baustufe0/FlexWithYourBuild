using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.GUI;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Logger = Jotunn.Logger;

namespace FlexWithYourBuildMod
{

    [BepInDependency(Jotunn.Main.ModGuid)]
    class PrintRessourceUsageCommand : ConsoleCommand
    {
        public static GameObject TextPanel;

        public override string Name => "build_info";

        public override string Help => "Prints all materials used for build";

        public override void Run(string[] args)
        {
            if (args.Length != 1)
            {
                Console.instance.Print("Usage: build_info [Radius]");
                return;
            }
            float radius = float.Parse(args[0]);

            Logger.LogInfo("Finding all pieces near to the player with radius " + radius);
            var resourceTotalDict = new Dictionary<string, int>();
            var pieceTotalDict = new Dictionary<string, int>();
            List<Piece> piecesInRadius = GetPiecesInRadius(Player.m_localPlayer.transform.position, radius);
            Logger.LogInfo(" -> Identified " + piecesInRadius + " pieces");
            foreach (var placedPiece in piecesInRadius)
            {
                string pieceName = RemoveSuffixFromName(placedPiece.name);
                if (pieceTotalDict.ContainsKey(pieceName))
                {
                    pieceTotalDict[pieceName] += 1;
                }
                else
                {
                    pieceTotalDict[pieceName] = 1;
                }

                foreach (var resource in placedPiece.m_resources)
                {
                    // for each piece identify which resources are required and add them to the dictionary in the specific amount
                    ItemDrop item = resource.m_resItem;
                    int amount = resource.m_amount;
                    string resourceName = RemoveSuffixFromName(item.name);
                    Logger.LogInfo("-> Resource:" + resourceName + ": " + amount + "");
                    if (resourceTotalDict.ContainsKey(resourceName))
                    {
                        resourceTotalDict[resourceName] += amount;
                    }
                    else
                    {
                        resourceTotalDict[resourceName] = amount;
                    }
                }
            }

            LinkedList<String> resourceText = new LinkedList<String>();
            resourceText.AddLast("Resources:");
            foreach (var resource in resourceTotalDict)
            {
                resourceText.AddLast(resource.Value + " x " + resource.Key);
                //Traverse.Create(Console.instance).Method("AddString", new object[] { $"Resource: {resource.Value}x {resource.Key}" }).GetValue();
            }

            LinkedList<String> pieceText = new LinkedList<String>();
            pieceText.AddLast("Pieces:");
            foreach (var piece in pieceTotalDict)
            {
                pieceText.AddLast(piece.Value + " x " + piece.Key);
            }

            print2Panel(pieceText, resourceText);
        }

        private void print2Panel(LinkedList<String> pieceText, LinkedList<String> resourceText)
        {
            if (TextPanel != null)
            {
                // panel already open...
                return;
            }

            if (GUIManager.Instance == null)
            {
                Logger.LogError("GUIManager instance is null");
                return;
            }

            TextPanel = GUIManager.Instance.CreateWoodpanel(
            parent: GUIManager.CustomGUIFront.transform,
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            position: new Vector2(0, 0),
            width: 800,
            height: 800,
            draggable: false);

            TextPanel.SetActive(false);
            DragWindowCntrl.ApplyDragWindowCntrl(TextPanel);

            float position = -50;
            foreach (String textEntry in pieceText)
            {
                var text = GUIManager.Instance.CreateText(
                    text: textEntry,
                    parent: TextPanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(-200f, position),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 15,
                    color: Color.white,
                    outline: true,
                    outlineColor: Color.black,
                    width: 350f,
                    height: 20f,
                    addContentSizeFitter: false);
                position -= 20f;
            }

            position = -50;
            foreach (String textEntry in resourceText)
            {
                var text = GUIManager.Instance.CreateText(
                    text: textEntry,
                    parent: TextPanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(200f, position),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 15,
                    color: Color.white,
                    outline: true,
                    outlineColor: Color.black,
                    width: 350f,
                    height: 20f,
                    addContentSizeFitter: false);
                position -= 20f;
            }

            // Create the button object
            GameObject buttonObject = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: TextPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(320, 360),
                width: 100,
                height: 60);
            buttonObject.SetActive(true);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(closeTextPanel);

            TextPanel.SetActive(true);
            GUIManager.BlockInput(true);
        }

        private void closeTextPanel()
        {
            // Set the active state of the panel
            TextPanel.SetActive(false);

            // Toggle input for the player and camera while displaying the GUI
            GUIManager.BlockInput(false);

            TextPanel = null;
        }

        private static string RemoveSuffixFromName(string input)
        {
            int startingIdx = input.IndexOf('(');
            if (startingIdx == -1)
                return input;

            // avoid possible errors with naming inconsistencies
            int endingIdx = input.IndexOf(')');
            if (endingIdx == -1)
                return input;

            return input.Substring(0, startingIdx).ToLower();
        }


        public List<Piece> GetPiecesInRadius(Vector3 position, float radius)
        {
            List<Piece> result = new List<Piece>();
            foreach (var piece in Piece.m_allPieces)
            {
                Vector3 piecePos = piece.transform.position;
                if (Vector2.Distance(new Vector2(position.x, position.z), new Vector2(piecePos.x, piecePos.z)) <= radius)
                {
                    result.Add(piece);
                }
            }
            return result;
        }


    }
}
