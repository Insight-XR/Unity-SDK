﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrKeyboardUI.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.System;
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateXR.UI.Helpers.Keyboard
{
    /// <summary>
    ///     Component that handles a keyboard in VR for user input
    /// </summary>
    public class UxrKeyboardUI : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool       _multiline = true;
        [SerializeField] private int        _maxLineLength;
        [SerializeField] private int        _maxLineCount;
        [SerializeField] private Text       _consoleDisplay;
        [SerializeField] private Text       _currentLineDisplay;
        [SerializeField] private bool       _consoleDisplayUsesCursor = true;
        [SerializeField] private bool       _lineDisplayUsesCursor    = true;
        [SerializeField] private GameObject _capsLockEnabledObject;
        [SerializeField] private bool       _capsLockEnabled;
        [SerializeField] private bool       _previewCaps;
        [SerializeField] private GameObject _passwordPreviewRootObject;
        [SerializeField] private GameObject _passwordPreviewEnabledObject;
        [SerializeField] private bool       _isPassword;
        [SerializeField] private bool       _hidePassword = true;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called on key presses/releases.
        /// </summary>
        public event EventHandler<UxrKeyboardKeyEventArgs> KeyPressed;

        /// <summary>
        ///     Event called on key presses/releases when the input is disabled using <see cref="AllowInput" />.
        /// </summary>
        public event EventHandler<UxrKeyboardKeyEventArgs> DisallowedKeyPressed;

        /// <summary>
        ///     Event we can subscribe to if we want notifications whenever the current line
        ///     being typed in using the keyboard changed.
        /// </summary>
        public event EventHandler<string> CurrentLineChanged;

        /// <summary>
        ///     Contains information about the key in our internal dictionary.
        /// </summary>
        public class KeyInfo
        {
        }

        /// <summary>
        ///     Gets whether a shift key is being pressed.
        /// </summary>
        public bool ShiftEnabled => _shiftEnabled > 0;

        /// <summary>
        ///     Gets whether a Control key is pressed.
        /// </summary>
        public bool ControlEnabled => _controlEnabled > 0;

        /// <summary>
        ///     Gets the current console text content including the cursor.
        /// </summary>
        public string ConsoleContentWithCursor => ConsoleContent + CurrentCursor;

        /// <summary>
        ///     Gets the current console line including the cursor.
        /// </summary>
        public string CurrentLineWithCursor => CurrentLine + CurrentCursor;

        /// <summary>
        ///     Gets the current console cursor (can be empty or the cursor character as a string).
        /// </summary>
        public string CurrentCursor => AllowInput && Mathf.RoundToInt(Time.time * 1000) / 200 % 2 == 0 ? "_" : string.Empty;

        /// <summary>
        ///     Gets whether caps lock is enabled.
        /// </summary>
        public bool CapsLockEnabled
        {
            get => _capsLockEnabled;
            set => _capsLockEnabled = value;
        }

        /// <summary>
        ///     Gets whether the Alt key is pressed.
        /// </summary>
        public bool AltEnabled { get; private set; }

        /// <summary>
        ///     Gets whether the Alt GR key is pressed.
        /// </summary>
        public bool AltGrEnabled { get; private set; }

        /// <summary>
        ///     Gets the current console text content.
        /// </summary>
        public string ConsoleContent { get; private set; }

        /// <summary>
        ///     Gets the current console line without the cursor.
        /// </summary>
        public string CurrentLine
        {
            get => _currentLine;
            private set
            {
                if (value != _currentLine)
                {
                    _currentLine = value;
                    OnCurrentLineChanged(value);
                }
            }
        }

        /// <summary>
        ///     Gets or sets whether keyboard input is allowed.
        /// </summary>
        public bool AllowInput { get; set; }

        /// <summary>
        ///     Gets or sets whether the key labels casing changes when the shift of caps lock key is pressed.
        /// </summary>
        public bool PreviewCaps
        {
            get => _previewCaps;
            set
            {
                _previewCaps = value;
                UpdateLabelsCase();
            }
        }

        /// <summary>
        ///     Gets or sets whether the keyboard is being used to type in a password. This can be used to hide the content behind
        ///     asterisk characters.
        /// </summary>
        public bool IsPassword
        {
            get => _isPassword;
            set => _isPassword = value;
        }

        /// <summary>
        ///     Gets or sets whether to hide password characters when <see cref="IsPassword" /> is used.
        /// </summary>
        public bool HidePassword
        {
            get => _hidePassword;
            set => _hidePassword = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Clears the console content.
        /// </summary>
        public void Clear()
        {
            _currentLineCount = 1;
            ConsoleContent    = string.Empty;
            CurrentLine       = string.Empty;
        }

        /// <summary>
        ///     If different symbols are present (through a ToggleSymbols keyboard key), sets the default symbols
        ///     as the currently enabled. Usually the default symbols are the regular alphabet letters.
        /// </summary>
        public void EnableDefaultSymbols()
        {
            if (_keyToggleSymbols != null)
            {
                _keyToggleSymbols.SetDefaultSymbols();
            }
        }

        /// <summary>
        ///     Adds content to the console. This method should be used instead of the <see cref="ConsoleContent" /> property since
        ///     <see cref="ConsoleContent" /> will not process lines.
        /// </summary>
        /// <param name="newContent">Text content to append</param>
        public void AddConsoleContent(string newContent)
        {
            if (string.IsNullOrEmpty(newContent))
            {
                return;
            }

            // Count the number of lines we are adding:
            int newLineCount = newContent.GetOccurrenceCount("\n", false);
            ConsoleContent    += newContent;
            _currentLineCount += newLineCount;

            // Check if we exceeded the maximum line amount
            CheckMaxLines();
        }

        /// <summary>
        ///     Called to register a new key in the keyboard.
        /// </summary>
        /// <param name="key">Key to register</param>
        public void RegisterKey(UxrKeyboardKeyUI key)
        {
            Debug.Assert(key != null,              "Keyboard key is null");
            Debug.Assert(key.ControlInput != null, "Keyboard key's ControlInput is null");

            if (!_keys.ContainsKey(key))
            {
                _keys.Add(key, new KeyInfo());

                key.ControlInput.Pressed  += KeyButton_KeyDown;
                key.ControlInput.Released += KeyButton_KeyUp;

                if (key.KeyType == UxrKeyType.ToggleSymbols)
                {
                    _keyToggleSymbols = key;
                }
            }
        }

        /// <summary>
        ///     Called to unregister a key from the keyboard.
        /// </summary>
        /// <param name="key">Key to unregister</param>
        public void UnregisterKey(UxrKeyboardKeyUI key)
        {
            Debug.Assert(key != null, "Keyboard key is null");

            if (_keys.ContainsKey(key))
            {
                _keys.Remove(key);

                key.ControlInput.Pressed  -= KeyButton_KeyDown;
                key.ControlInput.Released -= KeyButton_KeyUp;

                if (key == _keyToggleSymbols)
                {
                    _keyToggleSymbols = null;
                }
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the keyboard and clears the content.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            AllowInput = true;
            Clear();

            if (_previewCaps)
            {
                UpdateLabelsCase();
            }
        }

        /// <summary>
        ///     If there is a console display Text component specified, it becomes updated with the content plus the cursor.
        ///     If there is a caps lock GameObject specified it is updated to reflect the caps lock state as well.
        /// </summary>
        private void Update()
        {
            if (_consoleDisplay != null)
            {
                _consoleDisplay.text = FormatStringOutput(_consoleDisplayUsesCursor ? ConsoleContentWithCursor : ConsoleContent, _consoleDisplayUsesCursor);
            }

            if (_currentLineDisplay != null)
            {
                _currentLineDisplay.text = FormatStringOutput(_lineDisplayUsesCursor ? CurrentLineWithCursor : CurrentLine, _consoleDisplayUsesCursor);
            }

            if (_capsLockEnabledObject != null)
            {
                _capsLockEnabledObject.SetActive(_capsLockEnabled);
            }

            if (_passwordPreviewRootObject != null)
            {
                _passwordPreviewRootObject.SetActive(IsPassword);
            }

            if (_passwordPreviewEnabledObject != null)
            {
                _passwordPreviewEnabledObject.SetActive(!_hidePassword && _isPassword);
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when a keyboard key was pressed.
        /// </summary>
        /// <param name="controlInput">The control that was pressed</param>
        /// <param name="eventData">Event data</param>
        private void KeyButton_KeyDown(UxrControlInput controlInput, PointerEventData eventData)
        {
            UxrKeyboardKeyUI key = controlInput.GetComponent<UxrKeyboardKeyUI>();

            if (!AllowInput)
            {
                // Event notification
                DisallowedKeyPressed?.Invoke(this, new UxrKeyboardKeyEventArgs(key, true, null));
                return;
            }

            string lastLine = string.Empty;

            if (key.KeyType == UxrKeyType.Printable)
            {
                if (!(_maxLineLength > 0 && CurrentLine.Length >= _maxLineLength))
                {
                    if (key.KeyLayoutType == UxrKeyLayoutType.SingleChar)
                    {
                        if (!string.IsNullOrEmpty(key.ForceLabel))
                        {
                            ConsoleContent += key.GetSingleLayoutValueNoForceLabel(_capsLockEnabled || _shiftEnabled > 0, AltGrEnabled);
                            CurrentLine    += key.GetSingleLayoutValueNoForceLabel(_capsLockEnabled || _shiftEnabled > 0, AltGrEnabled);
                        }
                        else
                        {
                            if (char.IsLetter(key.SingleLayoutValue))
                            {
                                char newCar = _capsLockEnabled || _shiftEnabled > 0 ? char.ToUpper(key.SingleLayoutValue) : char.ToLower(key.SingleLayoutValue);

                                ConsoleContent += newCar;
                                CurrentLine    += newCar;
                            }
                            else
                            {
                                char newCar = key.GetSingleLayoutValueNoForceLabel(_shiftEnabled > 0 || _capsLockEnabled, AltGrEnabled);
                                ConsoleContent += newCar;
                                CurrentLine    += newCar;
                            }
                        }
                    }
                    else if (key.KeyLayoutType == UxrKeyLayoutType.MultipleChar)
                    {
                        if (_shiftEnabled > 0)
                        {
                            ConsoleContent += key.MultipleLayoutValueTopLeft;
                            CurrentLine    += key.MultipleLayoutValueTopLeft;
                        }
                        else if (AltGrEnabled)
                        {
                            if (key.HasMultipleLayoutValueBottomRight)
                            {
                                ConsoleContent += key.MultipleLayoutValueBottomRight;
                                CurrentLine    += key.MultipleLayoutValueBottomRight;
                            }
                        }
                        else
                        {
                            ConsoleContent += key.MultipleLayoutValueBottomLeft;
                            CurrentLine    += key.MultipleLayoutValueBottomLeft;
                        }
                    }
                }
            }
            else if (key.KeyType == UxrKeyType.Tab)
            {
                string tab             = "    ";
                int    charsAddedCount = _maxLineLength > 0 ? CurrentLine.Length + tab.Length > _maxLineLength ? _maxLineLength - CurrentLine.Length : tab.Length : tab.Length;

                ConsoleContent += tab.Substring(0, charsAddedCount);
                CurrentLine    += tab.Substring(0, charsAddedCount);
            }
            else if (key.KeyType == UxrKeyType.Shift)
            {
                _shiftEnabled++;

                if (_previewCaps)
                {
                    UpdateLabelsCase();
                }
            }
            else if (key.KeyType == UxrKeyType.CapsLock)
            {
                _capsLockEnabled = !_capsLockEnabled;

                if (_previewCaps)
                {
                    UpdateLabelsCase();
                }
            }
            else if (key.KeyType == UxrKeyType.Control)
            {
                _controlEnabled++;
            }
            else if (key.KeyType == UxrKeyType.Alt)
            {
                AltEnabled = true;
            }
            else if (key.KeyType == UxrKeyType.AltGr)
            {
                AltGrEnabled = true;
            }
            else if (key.KeyType == UxrKeyType.Enter)
            {
#if !UNITY_WSA
                lastLine = string.Copy(CurrentLine);
#else
                lastLine = string.Empty + CurrentLine;
#endif
                if (_multiline)
                {
                    ConsoleContent += "\n";
                    CurrentLine    =  string.Empty;
                    _currentLineCount++;
                    CheckMaxLines();
                }
            }
            else if (key.KeyType == UxrKeyType.Backspace)
            {
                if (CurrentLine.Length > 0)
                {
                    ConsoleContent = ConsoleContent.Substring(0, ConsoleContent.Length - 1);
                    CurrentLine    = CurrentLine.Substring(0, CurrentLine.Length - 1);
                }
            }
            else if (key.KeyType == UxrKeyType.Del)
            {
            }
            else if (key.KeyType == UxrKeyType.ToggleSymbols)
            {
                key.ToggleSymbols();
            }
            else if (key.KeyType == UxrKeyType.ToggleViewPassword)
            {
                _hidePassword = !_hidePassword;
            }
            else if (key.KeyType == UxrKeyType.Escape)
            {
            }

            // Event notification
            KeyPressed?.Invoke(this, new UxrKeyboardKeyEventArgs(key, true, key.KeyType == UxrKeyType.Enter ? lastLine : CurrentLine));
        }

        /// <summary>
        ///     Called when a keyboard keypress was released.
        /// </summary>
        /// <param name="controlInput">The control that was released</param>
        /// <param name="eventData">Event data</param>
        private void KeyButton_KeyUp(UxrControlInput controlInput, PointerEventData eventData)
        {
            UxrKeyboardKeyUI key = controlInput.GetComponent<UxrKeyboardKeyUI>();

            if (!AllowInput)
            {
                // Event notification
                DisallowedKeyPressed?.Invoke(this, new UxrKeyboardKeyEventArgs(key, false, null));
                return;
            }

            if (key.KeyType == UxrKeyType.Printable)
            {
            }
            else if (key.KeyType == UxrKeyType.Tab)
            {
            }
            else if (key.KeyType == UxrKeyType.Shift)
            {
                _shiftEnabled--;
            }
            else if (key.KeyType == UxrKeyType.CapsLock)
            {
            }
            else if (key.KeyType == UxrKeyType.Control)
            {
                _controlEnabled--;
            }
            else if (key.KeyType == UxrKeyType.Alt)
            {
                AltEnabled = false;
            }
            else if (key.KeyType == UxrKeyType.AltGr)
            {
                AltGrEnabled = false;
            }
            else if (key.KeyType == UxrKeyType.Enter)
            {
            }
            else if (key.KeyType == UxrKeyType.Backspace)
            {
            }
            else if (key.KeyType == UxrKeyType.Del)
            {
            }
            else if (key.KeyType == UxrKeyType.Escape)
            {
            }

            // Event notification
            KeyPressed?.Invoke(this, new UxrKeyboardKeyEventArgs(key, false, CurrentLine));
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for the <see cref="CurrentLineChanged" /> event.
        /// </summary>
        /// <param name="value">New line value</param>
        protected virtual void OnCurrentLineChanged(string value)
        {
            CurrentLineChanged?.Invoke(this, value);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Formats the given string to show it to the user. This is mainly used to make sure that passwords are hidden behind
        ///     asterisk characters.
        /// </summary>
        /// <param name="content">Content to format using the current settings</param>
        /// <param name="isUsingCursor">
        ///     Tells whether content is a string that may have a cursor appended
        /// </param>
        /// <returns>Formatted string ready to show to the user</returns>
        private string FormatStringOutput(string content, bool isUsingCursor)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            return _isPassword && _hidePassword ? new string('*', content.Length - CurrentCursor.Length) + CurrentCursor : content;
        }

        /// <summary>
        ///     Checks if the maximum number of lines was reached in the console and if so removes lines from the beginning.
        /// </summary>
        private void CheckMaxLines()
        {
            if (_maxLineCount > 0 && _currentLineCount > _maxLineCount)
            {
                int linesCounted = 0;

                for (int i = 0; i < ConsoleContent.Length; ++i)
                {
                    if (ConsoleContent[i] == '\n')
                    {
                        linesCounted++;

                        if (linesCounted == _currentLineCount - _maxLineCount)
                        {
                            ConsoleContent    =  ConsoleContent.Remove(0, i + 1);
                            _currentLineCount -= linesCounted;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Updates uppercase/lowercase labels depending on the shift and caps lock state.
        /// </summary>
        private void UpdateLabelsCase()
        {
            if (_keys == null)
            {
                return;
            }

            foreach (KeyValuePair<UxrKeyboardKeyUI, KeyInfo> keyPair in _keys)
            {
                if (keyPair.Key.IsLetterKey)
                {
                    keyPair.Key.UpdateLetterKeyLabel(ShiftEnabled || CapsLockEnabled);
                }
            }
        }

        #endregion

        #region Private Types & Data

        private readonly Dictionary<UxrKeyboardKeyUI, KeyInfo> _keys = new Dictionary<UxrKeyboardKeyUI, KeyInfo>();

        private string _currentLine;

        private int _currentLineCount;
        private int _shiftEnabled;
        private int _controlEnabled;

        private UxrKeyboardKeyUI _keyToggleSymbols;

        #endregion
    }
}