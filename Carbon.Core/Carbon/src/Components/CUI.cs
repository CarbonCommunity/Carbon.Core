using System.Collections.Generic;
using System.Globalization;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using static Carbon.CUI;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon;

public class CUI
{
	public static CUI Pool { get; set; } = new CUI();

	#region Properties

	internal static int _currentId { get; set; }

	internal static List<object> _queue = new List<object>();
	internal static List<CuiElementContainer> _containerPool = new List<CuiElementContainer>();

	internal static List<CuiElement> _elements = new List<CuiElement>();
	internal static List<ICuiComponent> _images = new List<ICuiComponent>();
	internal static List<ICuiComponent> _rawImages = new List<ICuiComponent>();
	internal static List<ICuiComponent> _texts = new List<ICuiComponent>();
	internal static List<ICuiComponent> _buttons = new List<ICuiComponent>();
	internal static List<ICuiComponent> _rects = new List<ICuiComponent>();
	internal static List<ICuiComponent> _needsCursors = new List<ICuiComponent>();
	internal static List<ICuiComponent> _needsKeyboards = new List<ICuiComponent>();

	#endregion

	#region Default Instances (for values)

	internal static CuiRectPosition _defaultPosition = new CuiRectPosition(0f, 0f, 1f, 1f);
	internal static CuiImageComponent _defaultImage = new CuiImageComponent();
	internal static CuiRawImageComponent _defaultRawImage = new CuiRawImageComponent();
	internal static CuiRectTransformComponent _defaultRectTransform = new CuiRectTransformComponent();
	internal static CuiTextComponent _defaultText = new CuiTextComponent();
	internal static CuiButtonComponent _defaultButton = new CuiButtonComponent();

	#endregion

	#region Pooling

	public static void SendToPool()
	{
		foreach (var entry in _queue)
		{
			SendToPool(entry);
		}

		_queue.Clear();
		_currentId = 0;
	}

	internal static string AppendId()
	{
		_currentId++;
		return _currentId.ToString();
	}
	internal static void SendToPool<T>(T element)
	{
		if (element == null) return;

		var componentElement = element as ICuiComponent;

		if (element is CuiImageComponent) _images.Add(componentElement);
		else if (element is CuiRawImageComponent) _rawImages.Add(componentElement);
		else if (element is CuiTextComponent) _texts.Add(componentElement);
		else if (element is CuiButtonComponent) _buttons.Add(componentElement);
		else if (element is CuiRectTransformComponent) _rects.Add(componentElement);
		else if (element is CuiNeedsCursorComponent) _needsCursors.Add(componentElement);
		else if (element is CuiNeedsKeyboardComponent) _needsKeyboards.Add(componentElement);
	}
	internal static CuiElement TakeFromPool(string name = null, string parent = "Hud")
	{
		var element = (CuiElement)null;

		if (_containerPool.Count == 0)
		{
			element = new CuiElement();
		}
		else
		{
			element = _elements[0];
			_elements.RemoveAt(0);
		}

		element.Name = name;
		element.Parent = parent;
		element.FadeOut = 0.1f;
		element.Components.Clear();

		_queue.Add(element);
		return element;
	}

	#endregion

	#region Pooled Elements

	internal static CuiElementContainer TakeFromPoolContainer()
	{
		var element = (CuiElementContainer)null;

		if (_containerPool.Count == 0)
		{
			element = new CuiElementContainer();
		}
		else
		{
			element = _containerPool[0];
			element.Clear();

			_containerPool.RemoveAt(0);
		}

		_queue.Add(element);
		return element;
	}
	internal static CuiRectPosition TakeFromPoolDimensions()
	{
		var element = new CuiRectPosition(_defaultPosition.xMin, _defaultPosition.yMin, _defaultPosition.xMax, _defaultPosition.yMax);

		return element;
	}
	internal static CuiImageComponent TakeFromPoolImage()
	{
		var element = (CuiImageComponent)null;

		if (_images.Count == 0)
		{
			element = new CuiImageComponent();
		}
		else
		{
			element = _images[0] as CuiImageComponent;
			element.SkinId = _defaultImage.SkinId;
			element.Png = _defaultImage.Png;
			element.ItemId = _defaultImage.ItemId;
			element.FadeIn = _defaultImage.FadeIn;
			element.Sprite = _defaultImage.Sprite;
			element.Color = _defaultImage.Color;
			_images.RemoveAt(0);
		}

		_queue.Add(element);
		return element;
	}
	internal static CuiRawImageComponent TakeFromPoolRawImage()
	{
		var element = (CuiRawImageComponent)null;

		if (_rawImages.Count == 0)
		{
			element = new CuiRawImageComponent();
		}
		else
		{
			element = _rawImages[0] as CuiRawImageComponent;
			element.Url = _defaultRawImage.Url;
			element.Png = _defaultRawImage.Png;
			element.FadeIn = _defaultRawImage.FadeIn;
			element.Sprite = _defaultRawImage.Sprite;
			element.Color = _defaultRawImage.Color;
			_rawImages.RemoveAt(0);
		}

		_queue.Add(element);
		return element;
	}
	internal static CuiRectTransformComponent TakeFromPoolRect()
	{
		var element = (CuiRectTransformComponent)null;

		if (_rects.Count == 0)
		{
			element = new CuiRectTransformComponent();
		}
		else
		{
			element = _rects[0] as CuiRectTransformComponent;
			element.AnchorMin = _defaultRectTransform.AnchorMin;
			element.AnchorMax = _defaultRectTransform.AnchorMax;
			element.OffsetMin = _defaultRectTransform.OffsetMin;
			element.OffsetMax = _defaultRectTransform.OffsetMax;
			_rects.RemoveAt(0);
		}

		_queue.Add(element);
		return element;
	}
	internal static CuiTextComponent TakeFromPoolText()
	{
		var element = (CuiTextComponent)null;

		if (_texts.Count == 0)
		{
			element = new CuiTextComponent();
		}
		else
		{
			element = _texts[0] as CuiTextComponent;
			element.Color = _defaultText.Color;
			element.FontSize = _defaultText.FontSize;
			element.Font = _defaultText.Font;
			_texts.RemoveAt(0);
		}

		_queue.Add(element);
		return element;
	}
	internal static CuiButtonComponent TakeFromPoolButton()
	{
		var element = (CuiButtonComponent)null;

		if (_buttons.Count == 0)
		{
			element = new CuiButtonComponent();
		}
		else
		{
			element = _buttons[0] as CuiButtonComponent;
			element.Color = _defaultButton.Color;
			element.Material = _defaultButton.Material;
			element.Sprite = _defaultButton.Sprite;
			_buttons.RemoveAt(0);
		}

		_queue.Add(element);
		return element;
	}
	internal static CuiNeedsCursorComponent TakeFromPoolNeedsCursor()
	{
		var element = (CuiNeedsCursorComponent)null;

		if (_needsCursors.Count == 0)
		{
			element = new CuiNeedsCursorComponent();
		}
		else
		{
			element = _needsCursors[0] as CuiNeedsCursorComponent;
			_needsCursors.RemoveAt(0);
		}

		_queue.Add(element);
		return element;
	}
	internal static CuiNeedsKeyboardComponent TakeFromPoolNeedsKeyboard()
	{
		var element = (CuiNeedsKeyboardComponent)null;

		if (_needsKeyboards.Count == 0)
		{
			element = new CuiNeedsKeyboardComponent();
		}
		else
		{
			element = _needsKeyboards[0] as CuiNeedsKeyboardComponent;
			_needsKeyboards.RemoveAt(0);
		}

		_queue.Add(element);
		return element;
	}

	#endregion

	#region Public Elements

	public static CuiElementContainer Container(string panel, string color, float xMin = 0f, float yMin = 1f, float xMax = 0f, float yMax = 1f, bool useCursor = false, string parent = "Overlay")
	{
		var container = TakeFromPoolContainer();
		container.Name = panel;

		var element = TakeFromPool(panel, parent);

		var image = TakeFromPoolImage();
		image.Color = color;
		element.Components.Add(image);

		var rect = TakeFromPoolRect();
		rect.AnchorMin = $"{xMin} {yMin}";
		rect.AnchorMax = $"{xMax} {yMax}";
		element.Components.Add(rect);

		if (useCursor) element.Components.Add(TakeFromPoolNeedsCursor());

		container.Add(element);
		container.Add(TakeFromPool(AppendId(), parent));
		return container;
	}

	public static string Color(string hexColor, float alpha)
	{
		if (hexColor.StartsWith("#")) hexColor = hexColor.Substring(1);

		var red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
		var green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
		var blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);

		return $"{(double)red / 255} {(double)green / 255} {(double)blue / 255} {alpha}";
	}

	#endregion

	#region Classes

	public class CuiRectPosition
	{
		public float xMin { get; set; }
		public float yMin { get; set; }
		public float xMax { get; set; }
		public float yMax { get; set; }

		public CuiRectPosition(float xMin, float yMin, float xMax, float yMax)
		{
			this.xMin = xMin;
			this.yMin = yMin;
			this.xMax = xMax;
			this.yMax = yMax;
		}

		public string GetMin() => $"{xMin} {yMin}";
		public string GetMax() => $"{xMax} {yMax}";
	}

	#endregion
}

public static class CUIStatics
{
	public static CuiElementContainer Panel(this CuiElementContainer container, string panel, string color, float xMin = 0f, float yMin = 1f, float xMax = 0f, float yMax = 1f, bool cursor = false)
	{
		var element = TakeFromPool(AppendId(), panel);

		var image = TakeFromPoolImage();
		image.Color = color;
		image.FadeIn = 0.1f;
		element.Components.Add(image);

		var rect = TakeFromPoolRect();
		rect.AnchorMin = $"{xMin} {yMin}";
		rect.AnchorMax = $"{xMax} {yMax}";
		element.Components.Add(rect);

		if (cursor) element.Components.Add(TakeFromPoolNeedsCursor());

		container.Add(element);

		return container;
	}
	public static CuiElementContainer Label(this CuiElementContainer container, string panel, string text, int size, float xMin = 0f, float yMin = 1f, float xMax = 0f, float yMax = 1f, TextAnchor align = TextAnchor.MiddleCenter, string font = "robotocondensed-bold.ttf")
	{
		var element = TakeFromPool(AppendId(), panel);

		var label = TakeFromPoolText();
		label.Text = text;
		label.FontSize = size;
		label.Align = align;
		label.Font = font;
		element.Components.Add(label);

		var rect = TakeFromPoolRect();
		rect.AnchorMin = $"{xMin} {yMin}";
		rect.AnchorMax = $"{xMax} {yMax}";
		element.Components.Add(rect);

		container.Add(element);
		return container;
	}
	public static CuiElementContainer Button(this CuiElementContainer container, string panel, string color, string text, int size, float xMin = 0f, float yMin = 1f, float xMax = 0f, float yMax = 1f, string command = null, TextAnchor align = TextAnchor.MiddleCenter)
	{
		var buttonElement = TakeFromPool(AppendId(), panel);

		var button = TakeFromPoolButton();
		button.Color = color;
		button.Command = command;
		buttonElement.Components.Add(button);

		var rect = TakeFromPoolRect();
		rect.AnchorMin = $"{xMin} {yMin}";
		rect.AnchorMax = $"{xMax} {yMax}";
		buttonElement.Components.Add(rect);

		container.Add(buttonElement);

		if (!string.IsNullOrEmpty(text))
		{
			var textElement = TakeFromPool(AppendId(), buttonElement.Name);

			var ptext = TakeFromPoolText();
			ptext.Text = text;
			ptext.FontSize = size;
			ptext.Align = align;
			ptext.Color = "1 1 1 1";
			textElement.Components.Add(ptext);

			var prect = TakeFromPoolRect();
			textElement.Components.Add(prect);

			container.Add(textElement);
		}

		return container;
	}
	public static CuiElementContainer Image(this CuiElementContainer container, string panel, string png, float xMin = 0f, float yMin = 1f, float xMax = 0f, float yMax = 1f)
	{
		var element = TakeFromPool(AppendId(), panel);

		var rawImage = TakeFromPoolRawImage();
		rawImage.Png = png;
		rawImage.FadeIn = 0.1f;
		element.Components.Add(rawImage);

		var rect = TakeFromPoolRect();
		rect.AnchorMin = $"{xMin} {yMin}";
		rect.AnchorMax = $"{xMax} {yMax}";
		element.Components.Add(rect);

		container.Add(element);
		return container;
	}
	public static CuiElementContainer ItemImage(this CuiElementContainer container, string panel, int itemID, float xMin = 0f, float yMin = 1f, float xMax = 0f, float yMax = 1f)
	{
		var element = TakeFromPool(AppendId(), panel);

		var rawImage = TakeFromPoolImage();
		rawImage.ItemId = itemID;
		element.Components.Add(rawImage);

		var rect = TakeFromPoolRect();
		rect.AnchorMin = $"{xMin} {yMin}";
		rect.AnchorMax = $"{xMax} {yMax}";
		element.Components.Add(rect);

		container.Add(element);
		return container;
	}

	public static void Send(this CuiElementContainer container, BasePlayer player)
	{
		CuiHelper.AddUi(player, container);
	}
	public static void Destroy(this CuiElementContainer container, BasePlayer player)
	{
		CuiHelper.DestroyUi(player, container.Name);
	}
}
