﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityExplorer.UI.Models;

namespace UnityExplorer.UI.Utility
{
    // To fix an issue with Input Fields and allow them to go inside a ScrollRect nicely.

    public class InputFieldScroller : UIBehaviourModel
    {
        public override GameObject UIRoot
        {
            get
            {
                if (InputField)
                    return InputField.gameObject;
                return null;
            }
        }

        internal AutoSliderScrollbar Slider;
        internal InputField InputField;

        internal RectTransform ContentRect;
        internal RectTransform ViewportRect;

        internal static CanvasScaler RootScaler;

        public InputFieldScroller(AutoSliderScrollbar sliderScroller, InputField inputField)
        {
            this.Slider = sliderScroller;
            this.InputField = inputField;

            inputField.onValueChanged.AddListener(OnTextChanged);

            ContentRect = inputField.GetComponent<RectTransform>();
            ViewportRect = ContentRect.transform.parent.GetComponent<RectTransform>();

            if (!RootScaler)
                RootScaler = UIManager.CanvasRoot.GetComponent<CanvasScaler>();
        }

        internal string m_lastText;
        internal bool m_updateWanted;
        internal bool m_wantJumpToBottom;
        private float m_desiredContentHeight;

        public override void Update()
        {
            if (m_updateWanted)
            {
                m_updateWanted = false;
                ProcessInputText();
            }

            float desiredHeight = Math.Max(m_desiredContentHeight, ViewportRect.rect.height);

            if (ContentRect.rect.height < desiredHeight)
            {
                ContentRect.sizeDelta = new Vector2(0, desiredHeight);
                this.Slider.UpdateSliderHandle();
            }
            else if (ContentRect.rect.height > desiredHeight)
            {
                ContentRect.sizeDelta = new Vector2(0, desiredHeight);
                this.Slider.UpdateSliderHandle();
            }

            if (m_wantJumpToBottom)
            {
                Slider.Slider.value = 1f;
                m_wantJumpToBottom = false;
            }
        }

        internal void OnTextChanged(string text)
        {
            m_lastText = text;
            m_updateWanted = true;
        }

        internal void ProcessInputText()
        {
            var curInputRect = InputField.textComponent.rectTransform.rect;
            var scaleFactor = RootScaler.scaleFactor;

            // Current text settings
            var texGenSettings = InputField.textComponent.GetGenerationSettings(curInputRect.size);
            texGenSettings.generateOutOfBounds = false;
            texGenSettings.scaleFactor = scaleFactor;

            // Preferred text rect height
            var textGen = InputField.textComponent.cachedTextGeneratorForLayout;
            m_desiredContentHeight = textGen.GetPreferredHeight(m_lastText, texGenSettings) + 10;

            // jump to bottom
            if (InputField.caretPosition == InputField.text.Length
                && InputField.text.Length > 0
                && InputField.text[InputField.text.Length - 1] == '\n')
            {
                m_wantJumpToBottom = true;
            }
        }

        public override void ConstructUI(GameObject parent)
        {
            throw new NotImplementedException();
        }
    }
}