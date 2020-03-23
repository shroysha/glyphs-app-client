using System.Collections.Generic;
using UnityEngine;

public class ScrollableGlyphList : MonoBehaviour {

	private static readonly int GLYPHS_IN_COLUMN = 3;
	private static readonly float GLYPH_PADDING = 1.0f;
	private static readonly float INSET = 2.0f;
	private static readonly float PLACE_HOLDER_ASPECT_RATIO = 1.0f;

	public GameObject glyphPlaceHolderPrefab;
	public GameObject dateSeparatorPrefab;

	public RectTransform contentTransform;

	public GlyphContentDisplay contentDisplay;

	private float nextY = -DateSeparator.HEIGHT / 2.0f;
	private float contentHeight = 0.0f;

	// Use this for initialization
	void Start () {
		
	}

	public void setGlyphs(Glyph[] glyphs) {
		GlyphPlaceHolder[] placeholders = new GlyphPlaceHolder[glyphs.Length];

		for (int i = 0; i < glyphs.Length; i++) {
			GameObject go = Instantiate (glyphPlaceHolderPrefab);
			GlyphPlaceHolder placeholder = go.GetComponent<GlyphPlaceHolder> ();
			placeholder.setGlyph (glyphs[i]);
			placeholder.contentDisplay = contentDisplay;

			placeholders [i] = placeholder;
		}

		Dictionary<string, List<GlyphPlaceHolder>> dateSeparatedHolders = new Dictionary<string, List<GlyphPlaceHolder>> ();
		foreach (GlyphPlaceHolder holder in placeholders) {
			if (!dateSeparatedHolders.ContainsKey (holder.getTimeSince())) {
				dateSeparatedHolders [holder.getTimeSince ()] = new List<GlyphPlaceHolder> ();
			}
			dateSeparatedHolders[holder.getTimeSince()].Add(holder);
		}

		List<DateSeparator> dateSeparators = new List<DateSeparator>();

		foreach(string date in dateSeparatedHolders.Keys) {
			GameObject go = Instantiate (dateSeparatorPrefab);
			DateSeparator separator = go.GetComponent<DateSeparator> ();
			separator.setDateText (date);

			dateSeparators.Add(separator);
		}

		for (int i = 0; i < dateSeparators.Count; i++) {
			string nextDate = dateSeparators [i].dateText.text;
			List<GlyphPlaceHolder> holders = dateSeparatedHolders[nextDate];
			addSeparatedGlyphs (dateSeparators [i], holders);
		}
		contentHeight += DateSeparator.HEIGHT;
		contentTransform.sizeDelta = new Vector2 (contentTransform.sizeDelta.x, contentHeight);
	}

	private void addSeparatedGlyphs(DateSeparator separator, List<GlyphPlaceHolder> holders) {

		float CONSTANT_X = 0.0f;

		separator.separatorTransform.SetParent (contentTransform);
		separator.separatorTransform.anchoredPosition = new Vector2 (CONSTANT_X, nextY);
		separator.separatorTransform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
		separator.separatorTransform.anchorMin = new Vector2 (0.0f, 1.0f);
		separator.separatorTransform.anchorMax = new Vector2 (1.0f, 1.0f);
		separator.separatorTransform.offsetMin = new Vector2 (0.0f, separator.separatorTransform.offsetMin.y);
		separator.separatorTransform.offsetMax = new Vector2 (0.0f, separator.separatorTransform.offsetMax.y);

		contentHeight += DateSeparator.HEIGHT;
		nextY -= DateSeparator.HEIGHT / 2.0f;
			
		float contentPanelWidth = contentTransform.rect.width;
		float placeHolderWidth = (contentPanelWidth - INSET * 2.0f - GLYPH_PADDING * 2.0f) / (float)GLYPHS_IN_COLUMN;
		float placeHolderHeight = placeHolderWidth * PLACE_HOLDER_ASPECT_RATIO;

		for (int i = 0; i < holders.Count; i += GLYPHS_IN_COLUMN) {
			 nextY -= placeHolderHeight / 2.0f;

			GlyphPlaceHolder[] nextHolders = new GlyphPlaceHolder[GLYPHS_IN_COLUMN];
			for(int j = 0; j < GLYPHS_IN_COLUMN; j++) {
				if(!(i + j >= holders.Count))
					nextHolders [j] = holders [i + j];
			}

			float nextX = INSET + placeHolderWidth / 2.0f;

			foreach (GlyphPlaceHolder nextHolder in nextHolders) {
				if (nextHolder != null) {
					nextHolder.placeHolderTransform.SetParent (contentTransform);
					nextHolder.setPlaceHolderHeightForImage (placeHolderHeight);

					nextHolder.placeHolderTransform.anchorMin = new Vector2 (0.0f, 1.0f);
					nextHolder.placeHolderTransform.anchorMax = new Vector2 (0.0f, 1.0f);
					nextHolder.placeHolderTransform.sizeDelta = new Vector2 (placeHolderWidth, placeHolderHeight);
					nextHolder.placeHolderTransform.anchoredPosition = new Vector2 (nextX, nextY);
					nextHolder.placeHolderTransform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);

					nextX += placeHolderWidth + GLYPH_PADDING;
				}
			}

			contentHeight += placeHolderHeight;
			nextY -= placeHolderHeight / 2.0f + GLYPH_PADDING;
		}

		nextY -= DateSeparator.HEIGHT / 2.0f + GLYPH_PADDING * 4.0f;
	}


}
