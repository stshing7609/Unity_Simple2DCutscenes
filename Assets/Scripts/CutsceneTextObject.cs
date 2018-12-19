using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;

/* A object that stores, types letter by letter, and manipulates text for a cutscene
 * Fonts should be stored in directory Assets/Resources/Fonts
 * 
 * Created by Steven Shing, 12/2018
 * Open Source
 */

public class CutsceneTextObject : MonoBehaviour
{
    Text text;                          // the text this handler is editing
    string textToShow;                  // the string that the text will display
    float holdTime;                     // the amount of time that the text will delay spawning the next text or moving to the next batch or frame
    CutsceneManager cutsceneManager;    // the CutsceneManager handling this object

    void Awake()
    {
        cutsceneManager = transform.parent.GetComponent<CutsceneManager>(); // get the CutsceneManager from this object's parent

        text = GetComponent<Text>();                        // grab the text component
        text.horizontalOverflow = HorizontalWrapMode.Wrap;  // set the horizontal wrap mode to wrap
        text.verticalOverflow = VerticalWrapMode.Truncate;  // set the vertical wrap mode to truncate (this way the user can know if their text would overflow when creating a cutscene
    }

    // Initialize this object given specific CutsceneTextData and the amount of time to delay typing each letter (the higher this value, the slower it types
    public void Init(CutsceneTextData ctd, float typeDelaySpeed)
    {

        // check for default font
        if (!ctd.font.Equals("Arial"))
        {
            // attempt to find the font in Assets/Resources/Fonts
            try
            {
                text.font = Resources.Load<Font>("Fonts/" + ctd.font);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find font. Escaping CutsceneTextObject Init.\n" + e.Message);
                return;
            }
        }

        text.text = ""; // set the text to empty for now
        text.fontSize = ctd.fontSize;   // set the font size

        // attemp to parse the enum for the TextAnchor
        try
        {
            text.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), ctd.textAnchor);
        }
        catch (Exception e)
        {
            Debug.LogError("TextAnchor string to enum parse failed. Escaping CutsceneTextObject Init.\n" + e.Message);
            return;
        }

        text.color = new Color(ctd.fontColor[0], ctd.fontColor[1], ctd.fontColor[2], ctd.fontColor[3]); // set the text color

        textToShow = ctd.textToShow;    // save the full text we're going to display in the end
        holdTime = ctd.holdTime;        // save the hold time we'll wait before displaying the next text

        RectTransform rectTransform = GetComponent<RectTransform>();    // grab the rect transform
        rectTransform.localPosition = new Vector3(ctd.position[0], ctd.position[1], ctd.position[2]);   // set the position of the text in the scene
        rectTransform.sizeDelta = new Vector2(ctd.sizeDelta[0], ctd.sizeDelta[1]);  // set the size of the text field in the scene

        TypeSentence(typeDelaySpeed);   // begin typing the sentence with the set typeDelaySpeed
    }

    // types out a new sentence
    // @typeDelaySpeed - how long to wait before typing a new letter - the higher this value, the slower it types
    public void TypeSentence(float typeDelaySpeed)
    {
        StopAllCoroutines();                        // stop all coroutines in case we force advance before any coroutines finish
        StartCoroutine(TypeText(typeDelaySpeed));   // start the text typing coroutine
    }

    // displays the whole sentence
    public void DisplayWholeSentence()
    {
        StopAllCoroutines();                    // stop all coroutines as the player force advanced past them
        text.text = textToShow;                 // reveal the entire sentence

        cutsceneManager.TextComplete(holdTime); // let the manager know that this text is completed
    }

    // reset text to blank
    public void SetTextToBlank()
    {
        text.text = "";
    }

    // Coroutine for typing out text letter by letter
    IEnumerator TypeText(float typeDelaySpeed)
    {
        text.text = "";              // set text of the current textArea in the pattern to empty

        // reveal each character letter by letter from the final string of textToShow
        foreach (char letter in textToShow.ToCharArray())
        {
            text.text += letter;    // add a single letter to the text
            yield return new WaitForSeconds(typeDelaySpeed);    // delay by typeDelaySpeed
        }

        cutsceneManager.TextComplete(holdTime); // when we're done, let the CutsceneManager know
    }
}
