﻿using UnityEngine;
using UnityEngine.UI;

namespace SamSWAT.FireSupport
{
    public class FireSupportUIElement : MonoBehaviour
	{
		public Image Icon;
		public Image BackgroundImage;
		public Sprite DefaultSubColor;
		public Sprite SelectedSubColor;
		public Text AmountText;
		private bool _isUnderPointer;

		public bool IsUnderPointer
		{
			get
			{
				return _isUnderPointer;
			}
			set
			{
				if (_isUnderPointer == value)
				{
					return;
				}
				_isUnderPointer = value;
				UnderPointerChanged(_isUnderPointer);
			}
		}

		protected void UnderPointerChanged(bool isUnderPointer)
		{
			BackgroundImage.sprite = isUnderPointer? SelectedSubColor : DefaultSubColor;
		}
	}
}
