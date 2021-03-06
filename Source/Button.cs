﻿namespace DestroyEverythingYouCan
{
	using ColossalFramework.UI;
	using UnityEngine;
	using ICities;

	public class Button : LoadingExtensionBase
	{
		public UIButton button;

		public override void OnLevelLoaded(LoadMode mode)
		{
			// Get the UIView object. This seems to be the top-level object for most
			// of the UI.
			var uiView = UIView.GetAView();

			// Add a new button to the view.
			button = (UIButton)uiView.AddUIComponent(typeof(UIButton));

			// Set the text to show on the button.
			button.text = "Destroy everything!";

			// Set the button dimensions.
			button.width = 175;
			button.height = 30;

			// Style the button to look like a menu button.
			button.normalBgSprite = "ButtonMenu";
			button.disabledBgSprite = "ButtonMenuDisabled";
			button.hoveredBgSprite = "ButtonMenuHovered";
			button.focusedBgSprite = "ButtonMenuFocused";
			button.pressedBgSprite = "ButtonMenuPressed";
			button.textColor = new Color32(255, 255, 255, 255);
			button.disabledTextColor = new Color32(7, 7, 7, 255);
			button.hoveredTextColor = new Color32(7, 132, 255, 255);
			button.focusedTextColor = new Color32(255, 255, 255, 255);
			button.pressedTextColor = new Color32(30, 30, 44, 255);

			// Enable button sounds.
			button.playAudioEvents = true;

			// Place the button near the bulldoze icon
			button.transformPosition = new Vector3(1.3f, -0.85f);

			// Respond to button click.
			button.eventClick += ButtonClick;
			Debug.Log("DEYC: UI button created");
		}

		public override void OnLevelUnloading()
		{
			// Apparently does nothing :D
			Object.Destroy(button);
		}

		private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam)
		{
			Destroyer.destroyEverything = true;
			Debug.Log("Pressed destroyAll button");
		}
	}
}

